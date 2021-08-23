using watabou.utils;
using spdd.actors;
using spdd.sprites;

namespace spdd.items.armor.glyphs
{
    public class Camouflage : Armor.Glyph
    {
        private static readonly ItemSprite.Glowing GREEN = new ItemSprite.Glowing(new Color(0x44, 0x88, 0x22, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            //no proc effect, see HighGrass.trample
            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return GREEN;
        }
    }
}