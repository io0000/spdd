using watabou.utils;
using spdd.actors;
using spdd.sprites;

namespace spdd.items.armor.glyphs
{
    public class Brimstone : Armor.Glyph
    {
        private static readonly ItemSprite.Glowing ORANGE = new ItemSprite.Glowing(new Color(0xFF, 0x44, 0x00, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            //no proc effect, see Hero.isImmune and GhostHero.isImmune
            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return ORANGE;
        }

        // public static class BrimstoneShield 
    }
}