using watabou.utils;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.sprites;

namespace spdd.actors.mobs
{
    public class Bandit : Thief
    {
        //public Item item;

        public Bandit()
        {
            spriteClass = typeof(BanditSprite);

            //guaranteed first drop, then 1/3, 1/9, etc.
            lootChance = 1f;
        }

        public override bool Steal(Hero hero)
        {
            if (!base.Steal(hero))
                return false;

            Buff.Prolong<Blindness>(enemy, Blindness.DURATION / 2f);
            Buff.Affect<Poison>(enemy).Set(Rnd.Int(5, 7)); ;
            Buff.Prolong<Cripple>(enemy, Cripple.DURATION / 2f);
            Dungeon.Observe();

            return true;
        }
    }
}