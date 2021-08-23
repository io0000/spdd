using watabou.utils;
using spdd.actors;
using spdd.sprites;

namespace spdd.items.armor.glyphs
{
    public class Swiftness : Armor.Glyph
    {
        private static readonly ItemSprite.Glowing YELLOW = new ItemSprite.Glowing(new Color(0xFF, 0xFF, 0x00, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            //no proc effect, see armor.speedfactor for effect.
            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return YELLOW;
        }
    }
}