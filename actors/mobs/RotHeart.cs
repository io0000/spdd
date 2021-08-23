using System.Linq;
using watabou.utils;
using spdd.actors.buffs;
using spdd.actors.blobs;
using spdd.plants;
using spdd.sprites;
using spdd.scenes;

namespace spdd.actors.mobs
{
    public class RotHeart : Mob
    {
        public RotHeart()
        {
            InitInstance();

            spriteClass = typeof(RotHeartSprite);

            HP = HT = 80;
            defenseSkill = 0;

            EXP = 4;

            state = PASSIVE;

            properties.Add(Property.IMMOVABLE);
            properties.Add(Property.MINIBOSS);
        }

        public override void Damage(int dmg, object src)
        {
            //TODO: when effect properties are done, change this to FIRE
            if (src is Burning)
            {
                Destroy();
                sprite.Die();
            }
            else
            {
                base.Damage(dmg, src);
            }
        }

        public override int DefenseProc(Character enemy, int damage)
        {
            GameScene.Add(Blob.Seed(pos, 20, typeof(ToxicGas)));

            return base.DefenseProc(enemy, damage);
        }

        public override void Beckon(int cell)
        {
            //do nothing
        }

        public override bool GetCloser(int target)
        {
            return false;
        }

        public override void Destroy()
        {
            base.Destroy();
            foreach (Mob mob in Dungeon.level.mobs.ToArray())
            {
                if (mob is RotLasher)
                {
                    mob.Die(null);
                }
            }
        }

        public override void Die(object cause)
        {
            base.Die(cause);
            Dungeon.level.Drop(new Rotberry.Seed(), pos).sprite.Drop();
        }

        public override bool Reset()
        {
            return true;
        }

        public override int DamageRoll()
        {
            return 0;
        }

        public override int AttackSkill(Character target)
        {
            return 0;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 5);
        }

        private void InitInstance()
        {
            immunities.Add(typeof(Paralysis));
            immunities.Add(typeof(Amok));
            immunities.Add(typeof(Sleep));
            immunities.Add(typeof(ToxicGas));
            immunities.Add(typeof(Terror));
            immunities.Add(typeof(Vertigo));
        }
    }
}