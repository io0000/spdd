using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.utils;
using spdd.mechanics;
using spdd.effects;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class RipperDemon : Mob
    {
        public RipperDemon()
        {
            spriteClass = typeof(RipperSprite);

            HP = HT = 60;
            defenseSkill = 22;
            viewDistance = Light.DISTANCE;

            EXP = 9; //for corrupting
            maxLvl = -2;

            HUNTING = new RipperDemonHunting(this);

            baseSpeed = 1f;

            properties.Add(Property.DEMONIC);
            properties.Add(Property.UNDEAD);
        }

        public override float SpawningWeight()
        {
            return 0;
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(12, 25);
        }

        public override int AttackSkill(Character target)
        {
            return 30;
        }

        protected override float AttackDelay()
        {
            return base.AttackDelay() * 0.5f;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 4);
        }

        private const string LAST_ENEMY_POS = "last_enemy_pos";
        private const string LEAP_POS = "leap_pos";
        private const string LEAP_CD = "leap_cd";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(LAST_ENEMY_POS, lastEnemyPos);
            bundle.Put(LEAP_POS, leapPos);
            bundle.Put(LEAP_CD, leapCooldown);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            lastEnemyPos = bundle.GetInt(LAST_ENEMY_POS);
            leapPos = bundle.GetInt(LEAP_POS);
            leapCooldown = bundle.GetFloat(LEAP_CD);
        }

        private int lastEnemyPos = -1;

        public override bool Act()
        {
            var lastState = state;
            bool result = base.Act();
            if (paralysed <= 0)
                --leapCooldown;

            //if state changed from wandering to hunting, we haven't acted yet, don't update.
            if (!(lastState == WANDERING && state == HUNTING))
            {
                if (enemy != null)
                {
                    lastEnemyPos = enemy.pos;
                }
                else
                {
                    lastEnemyPos = Dungeon.hero.pos;
                }
            }

            return result;
        }

        private int leapPos = -1;
        private float leapCooldown;

        public class RipperDemonHunting : Mob.Hunting
        {
            public RipperDemonHunting(Mob mob)
                : base(mob)
            { }

            public override bool Act(bool enemyInFOV, bool justAlerted)
            {
                RipperDemon rd = (RipperDemon)mob;
                return rd.RipperDemonHuntingAct(enemyInFOV, justAlerted);
            }
        }

        public bool RipperDemonHuntingAct(bool enemyInFOV, bool justAlerted)
        {
            if (leapPos != -1)
            {
                //do leap
                sprite.visible = Dungeon.level.heroFOV[pos] || Dungeon.level.heroFOV[leapPos];

                var callback = new ActionCallback();
                callback.action = () =>
                {
                    Character ch = Actor.FindChar(leapPos);
                    if (ch != null)
                    {
                        if (alignment != ch.alignment)
                        {
                            float value = 0.75f * DamageRoll();
                            Buff.Affect<Bleeding>(ch).Set((int)value);
                            ch.sprite.Flash();
                            Sample.Instance.Play(Assets.Sounds.HIT);
                        }
                        //bounce to a random safe pos(if possible)
                        int bouncepos = -1;
                        foreach (int i in PathFinder.NEIGHBORS8)
                        {
                            if ((bouncepos == -1 || Dungeon.level.TrueDistance(pos, leapPos + i) < Dungeon.level.TrueDistance(pos, bouncepos))
                                    && Actor.FindChar(leapPos + i) == null && Dungeon.level.passable[leapPos + i])
                            {
                                bouncepos = leapPos + i;
                            }
                        }
                        if (bouncepos != -1)
                        {
                            pos = bouncepos;
                            Actor.AddDelayed(new Pushing(this, leapPos, bouncepos), -1);
                        }
                        else
                        {
                            pos = leapPos;
                        }
                    }
                    else
                    {
                        pos = leapPos;
                    }

                    leapPos = -1;
                    leapCooldown = Rnd.NormalIntRange(2, 4);
                    sprite.Idle();
                    Dungeon.level.OccupyCell(this);
                    Next();
                };

                sprite.Jump(pos, leapPos, callback);
                return false;
            }

            enemySeen = enemyInFOV;
            if (enemyInFOV && !IsCharmedBy(enemy) && CanAttack(enemy))
            {
                return DoAttack(enemy);
            }
            else
            {
                if (enemyInFOV)
                {
                    target = enemy.pos;
                }
                else if (enemy == null)
                {
                    state = WANDERING;
                    target = Dungeon.level.RandomDestination(this);
                    return true;
                }

                if (leapCooldown <= 0 && enemyInFOV && Dungeon.level.Distance(pos, enemy.pos) >= 3)
                {
                    int targetPos = enemy.pos;
                    if (lastEnemyPos != enemy.pos)
                    {
                        int closestIdx = 0;
                        for (int i = 1; i < PathFinder.CIRCLE8.Length; ++i)
                        {
                            if (Dungeon.level.TrueDistance(lastEnemyPos, enemy.pos + PathFinder.CIRCLE8[i])
                                    < Dungeon.level.TrueDistance(lastEnemyPos, enemy.pos + PathFinder.CIRCLE8[closestIdx]))
                            {
                                closestIdx = i;
                            }
                        }
                        targetPos = enemy.pos + PathFinder.CIRCLE8[(closestIdx + 4) % 8];
                    }

                    Ballistic b = new Ballistic(pos, targetPos, Ballistic.STOP_TARGET | Ballistic.STOP_TERRAIN);
                    //try aiming directly at hero if aiming near them doesn't work
                    if (b.collisionPos != targetPos && targetPos != enemy.pos)
                    {
                        targetPos = enemy.pos;
                        b = new Ballistic(pos, targetPos, Ballistic.STOP_TARGET | Ballistic.STOP_TERRAIN);
                    }
                    if (b.collisionPos == targetPos)
                    {
                        //get ready to leap
                        leapPos = targetPos;
                        //don't want to overly punish players with slow move or attack speed
                        Spend(GameMath.Gate(TICK, enemy.Cooldown(), 3 * TICK));
                        if (Dungeon.level.heroFOV[pos] || Dungeon.level.heroFOV[leapPos])
                        {
                            GLog.Warning(Messages.Get(this, "leap"));
                            sprite.parent.AddToBack(new TargetedCell(leapPos, new Color(0xFF, 0x00, 0x00, 0xFF)));
                            ((RipperSprite)sprite).LeapPrep(leapPos);
                            Dungeon.hero.Interrupt();
                        }
                        return true;
                    }
                }

                int oldPos = pos;
                if (target != -1 && GetCloser(target))
                {
                    Spend(1 / Speed());
                    return MoveSprite(oldPos, pos);
                }
                else
                {
                    Spend(TICK);
                    if (!enemyInFOV)
                    {
                        sprite.ShowLost();
                        state = WANDERING;
                        target = Dungeon.level.RandomDestination(this);
                    }
                    return true;
                }
            }
        } // HuntingAct()
    } // class RipperDemon
}