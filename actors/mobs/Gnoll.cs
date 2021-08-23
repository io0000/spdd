using watabou.utils;
using spdd.items;
using spdd.sprites;

namespace spdd.actors.mobs
{
    public class Gnoll : Mob
    {
        public Gnoll()
        {
            spriteClass = typeof(GnollSprite);

            HP = HT = 12;
            defenseSkill = 4;

            EXP = 2;
            maxLvl = 8;

            loot = typeof(Gold);
            lootChance = 0.5f;
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(1, 6);
        }

        public override int AttackSkill(Character target)
        {
            return 10;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 2);
        }
    }
}