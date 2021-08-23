using watabou.utils;
using spdd.actors;
using spdd.items.scrolls;
using spdd.sprites;

namespace spdd.items.armor.curses
{
    public class Displacement : Armor.Glyph
    {
        private static ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x00, 0x00, 0x00, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            if (defender == Dungeon.hero && Rnd.Int(20) == 0)
            {
                ScrollOfTeleportation.TeleportHero(Dungeon.hero);
                return 0;
            }

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return BLACK;
        }

        public override bool Curse()
        {
            return true;
        }
    }
}