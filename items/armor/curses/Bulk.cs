using watabou.utils;
using spdd.actors;
using spdd.sprites;

namespace spdd.items.armor.curses
{
    public class Bulk : Armor.Glyph
    {
        private static ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x00, 0x00, 0x00, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            //no proc effect, see armor.speedfactor
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