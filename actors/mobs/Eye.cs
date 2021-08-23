using watabou.utils;
using spdd.actors.buffs;
using spdd.scenes;
using spdd.effects;
using spdd.effects.particles;
using spdd.items;
using spdd.items.wands;
using spdd.mechanics;
using spdd.sprites;
using spdd.utils;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class Eye : Mob
    {
        public Eye()
        {
            InitInstance();

            spriteClass = typeof(EyeSprite);

            HP = HT = 100;
            defenseSkill = 20;
            viewDistance = Light.DISTANCE;

            EXP = 13;
            maxLvl = 26;

            flying = true;

            HUNTING = new HuntingEye(this);

            loot = new Dewdrop();
            lootChance = 1.0f;

            properties.Add(Property.DEMONIC);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(20, 30);
        }

        public override int AttackSkill(Character target)
        {
            return 30;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 10);
        }

        private Ballistic beam;
        private int beamTarget = -1;
        private int beamCooldown;
        public bool beamCharged;

        protected override bool CanAttack(Character enemy)
        {
            if (beamCooldown == 0)
            {
                Ballistic aim = new Ballistic(pos, enemy.pos, Ballistic.STOP_TERRAIN);

                if (enemy.invisible == 0 && !IsCharmedBy(enemy) && fieldOfView[enemy.pos] && aim.SubPath(1, aim.dist).Contains(enemy.pos))
                {
                    beam = aim;
                    beamTarget = aim.collisionPos;
                    return true;
                }
                else
                {
                    //if the beam is charged, it has to attack, will aim at previous location of target.
                    return beamCharged;
                }
            }
            else
            {
                return base.CanAttack(enemy);
            }
        }

        public override bool Act()
        {
            if (beamCharged && state != HUNTING)
            {
                beamCharged = false;
                sprite.Idle();
            }
            if (beam == null && beamTarget != -1)
            {
                beam = new Ballistic(pos, beamTarget, Ballistic.STOP_TERRAIN);
                sprite.TurnTo(pos, beamTarget);
            }
            if (beamCooldown > 0)
                --beamCooldown;

            return base.Act();
        }

        protected override bool DoAttack(Character enemy)
        {
            if (beamCooldown > 0)
            {
                return base.DoAttack(enemy);
            }
            else if (!beamCharged)
            {
                ((EyeSprite)sprite).Charge(enemy.pos);
                Spend(AttackDelay() * 2f);
                beamCharged = true;
                return true;
            }
            else
            {
                Spend(AttackDelay());

                beam = new Ballistic(pos, beamTarget, Ballistic.STOP_TERRAIN);
                if (Dungeon.level.heroFOV[pos] || Dungeon.level.heroFOV[beam.collisionPos])
                {
                    sprite.Zap(beam.collisionPos);
                    return false;
                }
                else
                {
                    sprite.Idle();
                    DoDeathGaze();
                    return true;
                }
            }
        }

        public override void Damage(int dmg, object src)
        {
            if (beamCharged)
                dmg /= 4;
            base.Damage(dmg, src);
        }

        //used so resistances can differentiate between melee and magical attacks
        public class DeathGaze
        { }

        // public void deathGaze()
        public void DoDeathGaze()
        {
            if (!beamCharged || beamCooldown > 0 || beam == null)
                return;

            beamCharged = false;
            beamCooldown = Rnd.IntRange(4, 6);

            bool terrainAffected = false;

            foreach (int pos in beam.SubPath(1, beam.dist))
            {

                if (Dungeon.level.flamable[pos])
                {
                    Dungeon.level.Destroy(pos);
                    GameScene.UpdateMap(pos);
                    terrainAffected = true;
                }

                var ch = Actor.FindChar(pos);
                if (ch == null)
                    continue;

                if (Hit(this, ch, true))
                {
                    ch.Damage(Rnd.NormalIntRange(30, 50), new DeathGaze());

                    if (Dungeon.level.heroFOV[pos])
                    {
                        ch.sprite.Flash();
                        CellEmitter.Center(pos).Burst(PurpleParticle.Burst, Rnd.IntRange(1, 2));
                    }

                    if (!ch.IsAlive() && ch == Dungeon.hero)
                    {
                        Dungeon.Fail(GetType());
                        GLog.Negative(Messages.Get(this, "deathgaze_kill"));
                    }
                }
                else
                {
                    ch.sprite.ShowStatus(CharSprite.NEUTRAL, ch.DefenseVerb());
                }
            }

            if (terrainAffected)
                Dungeon.Observe();

            beam = null;
            beamTarget = -1;
        }

        //generates an average of 1 dew, 0.25 seeds, and 0.25 stones
        public override Item CreateLoot()
        {
            Item loot;
            switch (Rnd.Int(4))
            {
                case 0:
                case 1:
                default:
                    loot = new Dewdrop().Quantity(2);
                    break;
                case 3:     // TODO확인 2?
                    loot = Generator.Random(Generator.Category.SEED);
                    break;
                case 4:     // TODO확인 3?
                    loot = Generator.Random(Generator.Category.STONE);
                    break;
            }
            return loot;
        }

        private const string BEAM_TARGET = "beamTarget";
        private const string BEAM_COOLDOWN = "beamCooldown";
        private const string BEAM_CHARGED = "beamCharged";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(BEAM_TARGET, beamTarget);
            bundle.Put(BEAM_COOLDOWN, beamCooldown);
            bundle.Put(BEAM_CHARGED, beamCharged);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            if (bundle.Contains(BEAM_TARGET))
                beamTarget = bundle.GetInt(BEAM_TARGET);
            beamCooldown = bundle.GetInt(BEAM_COOLDOWN);
            beamCharged = bundle.GetBoolean(BEAM_CHARGED);
        }

        private void InitInstance()
        {
            resistances.Add(typeof(WandOfDisintegration));
        }

        class HuntingEye : Mob.Hunting
        {
            public HuntingEye(Mob mob)
                : base(mob)
            { }

            public override bool Act(bool enemyInFOV, bool justAlerted)
            {
                Eye eye = (Eye)mob;

                //even if enemy isn't seen, attack them if the beam is charged
                if (eye.beamCharged && eye.enemy != null && eye.CanAttack(eye.enemy))
                {
                    eye.enemySeen = enemyInFOV;
                    return eye.DoAttack(eye.enemy);
                }
                return base.Act(enemyInFOV, justAlerted);
            }
        }
    }
}