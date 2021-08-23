using watabou.utils;
using spdd.actors;
using spdd.sprites;

namespace spdd.items.armor.glyphs
{
    public class Obfuscation : Armor.Glyph
    {
        private static readonly ItemSprite.Glowing GREY = new ItemSprite.Glowing(new Color(0x88, 0x88, 0x88, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            //no proc effect, see armor.stealthfactor for effect.
            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return GREY;
        }
    }
}