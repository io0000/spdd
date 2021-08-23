using watabou.utils;
using spdd.actors.blobs;
using spdd.actors.buffs;
using spdd.actors.mobs.npcs;
using spdd.scenes;
using spdd.sprites;

namespace spdd.actors.mobs
{
    public class FetidRat : Mob
    {
        public FetidRat()
        {
            InitInstance();

            spriteClass = typeof(FetidRatSprite);

            HP = HT = 20;
            defenseSkill = 5;

            EXP = 4;

            state = WANDERING;

            properties.Add(Property.MINIBOSS);
            properties.Add(Property.DEMONIC);
        }

        public override int AttackSkill(Character target)
        {
            return 12;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 2);
        }

        public override int AttackProc(Character enemy, int damage)
        {
            damage = base.AttackProc(enemy, damage);
            if (Rnd.Int(3) == 0)
                Buff.Affect<Ooze>(enemy).Set(Ooze.DURATION);

            return damage;
        }

        public override int DefenseProc(Character enemy, int damage)
        {
            GameScene.Add(Blob.Seed(pos, 20, typeof(StenchGas)));

            return base.DefenseProc(enemy, damage);
        }

        public override void Die(object cause)
        {
            base.Die(cause);

            Ghost.Quest.Process();
        }

        private void InitInstance()
        {
            immunities.Add(typeof(StenchGas));
        }
    }
}