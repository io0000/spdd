using watabou.utils;
using spdd.sprites;

namespace spdd.actors.mobs
{
    public class Rat : Mob
    {
        public Rat()
        {
            spriteClass = typeof(RatSprite);

            HP = HT = 8;
            defenseSkill = 2;

            maxLvl = 5;
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(1, 4);
        }

        public override int AttackSkill(Character target)
        {
            return 8;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 1);
        }
    }
}