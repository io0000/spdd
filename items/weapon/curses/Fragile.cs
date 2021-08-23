using watabou.utils;
using spdd.actors;
using spdd.sprites;

namespace spdd.items.weapon.curses
{
    public class Fragile : Weapon.Enchantment
    {
        private static ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x00, 0x00, 0x00, 0xFF));
        private int hits;

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            //degrades from 100% to 25% damage over 150 hits
            damage = (int)(damage * (1f - hits * 0.005f));
            if (hits < 150)
                ++hits;

            return damage;
        }

        public override bool Curse()
        {
            return true;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return BLACK;
        }

        private const string HITS = "hits";

        public override void RestoreFromBundle(Bundle bundle)
        {
            hits = bundle.GetInt(HITS);
        }

        public override void StoreInBundle(Bundle bundle)
        {
            bundle.Put(HITS, hits);
        }
    }
}