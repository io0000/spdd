using watabou.utils;
using spdd.actors;
using spdd.sprites;

namespace spdd.items.weapon.enchantments
{
    public class Projecting : Weapon.Enchantment
    {
        private static ItemSprite.Glowing PURPLE = new ItemSprite.Glowing(new Color(0x88, 0x44, 0xCC, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            //Does nothing as a proc, instead increases weapon range.
            //See weapon.reachFactor, and MissileWeapon.throwPos;
            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return PURPLE;
        }
    }
}