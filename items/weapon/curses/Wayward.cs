using watabou.utils;
using spdd.actors;
using spdd.sprites;

namespace spdd.items.weapon.curses
{
    public class Wayward : Weapon.Enchantment
    {
        private static ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x00, 0x00, 0x00, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            //no proc effect, see weapon.accuracyFactor for effect
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
    }
}