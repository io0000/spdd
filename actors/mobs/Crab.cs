using watabou.utils;
using spdd.items.food;
using spdd.sprites;

namespace spdd.actors.mobs
{
    public class Crab : Mob
    {
        public Crab()
        {
            spriteClass = typeof(CrabSprite);

            HP = HT = 15;
            defenseSkill = 5;
            baseSpeed = 2f;

            EXP = 4;
            maxLvl = 9;

            loot = new MysteryMeat();
            lootChance = 0.167f;
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(1, 7);
        }

        public override int AttackSkill(Character target)
        {
            return 12;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 4);
        }
    }
}