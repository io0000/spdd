using System;
using watabou.utils;
using watabou.noosa.audio;
using spdd.items;
using spdd.mechanics;
using spdd.actors.buffs;
using spdd.scenes;
using spdd.sprites;
using spdd.effects;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class Guard : Mob
    {
        //they can only use their chains once
        private bool chainsUsed;

        public Guard()
        {
            spriteClass = typeof(GuardSprite);

            HP = HT = 40;
            defenseSkill = 10;

            EXP = 7;
            maxLvl = 14;

            loot = Generator.Category.ARMOR;
            lootChance = 0.2f; //by default, see rollToDropLoot()

            properties.Add(Property.UNDEAD);

            HUNTING = new HuntingGuard(this);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(4, 12);
        }

        private bool Chain(int target)
        {
            if (chainsUsed || enemy.Properties().Contains(Property.IMMOVABLE))
                return false;

            Ballistic chain = new Ballistic(pos, target, Ballistic.PROJECTILE);

            if (chain.collisionPos != enemy.pos ||
                chain.path.Count < 2 ||
                Dungeon.level.pit[chain.path[1]])
            {
                return false;
            }
            else
            {
                int newPos = -1;
                foreach (int i in chain.SubPath(1, chain.dist))
                {
                    if (!Dungeon.level.solid[i] && Actor.FindChar(i) == null)
                    {
                        newPos = i;
                        break;
                    }
                }

                if (newPos == -1)
                {
                    return false;
                }
                else
                {
                    int newPosFinal = newPos;
                    this.target = newPos;
                    Yell(Messages.Get(this, "scorpion"));
                    new Item().ThrowSound();
                    Sample.Instance.Play(Assets.Sounds.CHAINS);

                    var chainCallback = new ActionCallback();
                    chainCallback.action = () =>
                    {
                        var pushCallback = new ActionCallback();
                        pushCallback.action = () =>
                        {
                            enemy.pos = newPosFinal;
                            Dungeon.level.OccupyCell(enemy);
                            Cripple.Prolong<Cripple>(enemy, 4f);
                            if (enemy == Dungeon.hero)
                            {
                                Dungeon.hero.Interrupt();
                                Dungeon.Observe();
                                GameScene.UpdateFog();
                            }
                        };

                        Actor.AddDelayed(new Pushing(enemy, enemy.pos, newPosFinal, pushCallback), -1);
                        Next();
                    };

                    var chains = new Chains(sprite.Center(), enemy.sprite.Center(), chainCallback);
                    sprite.parent.Add(chains);
                }
            }
            chainsUsed = true;
            return true;
        }

        public override int AttackSkill(Character target)
        {
            return 12;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 7);
        }

        public override void RollToDropLoot()
        {
            //each drop makes future drops 1/2 as likely
            // so loot chance looks like: 1/5, 1/10, 1/20, 1/40, etc.
            lootChance *= (float)Math.Pow(1 / 2f, Dungeon.LimitedDrops.GUARD_ARM.count);
            base.RollToDropLoot();
        }

        public override Item CreateLoot()
        {
            ++Dungeon.LimitedDrops.GUARD_ARM.count;
            return base.CreateLoot();
        }

        private const string CHAINSUSED = "chainsused";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(CHAINSUSED, chainsUsed);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            chainsUsed = bundle.GetBoolean(CHAINSUSED);
        }

        public class HuntingGuard : Mob.Hunting
        {
            public HuntingGuard(Mob mob)
                : base(mob)
            { }

            public override bool Act(bool enemyInFOV, bool justAlerted)
            {
                Guard guard = (Guard)mob;

                bool callBase = false;
                bool result = guard.HuntingGuardAct(enemyInFOV, justAlerted, ref callBase);
                if (callBase)
                    return base.Act(enemyInFOV, justAlerted);

                return result;
            }
        }

        public bool HuntingGuardAct(bool enemyInFOV, bool justAlerted, ref bool callBase)
        {
            enemySeen = enemyInFOV;

            if (!chainsUsed &&
                enemyInFOV &&
                !IsCharmedBy(enemy) &&
                !CanAttack(enemy) &&
                Dungeon.level.Distance(pos, enemy.pos) < 5 &&
                Rnd.Int(3) == 0 &&
                Chain(enemy.pos))
            {
                return false;
            }
            else
            {
                callBase = true;
                return true;
            }
        }
    }
}