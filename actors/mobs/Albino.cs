using watabou.utils;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.items.food;

namespace spdd.actors.mobs
{
    public class Albino : Rat
    {
        public Albino()
        {
            spriteClass = typeof(AlbinoSprite);

            HP = HT = 15;
            EXP = 2;

            loot = new MysteryMeat();
            lootChance = 1f;
        }

        public override int AttackProc(Character enemy, int damage)
        {
            damage = base.AttackProc(enemy, damage);
            if (Rnd.Int(2) == 0)
                Buff.Affect<Bleeding>(enemy).Set(damage);

            return damage;
        }
    }
}