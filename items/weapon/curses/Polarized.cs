using System;
using watabou.utils;
using spdd.actors;
using spdd.sprites;

namespace spdd.items.weapon.curses
{
    public class Polarized : Weapon.Enchantment
    {
        private static ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x00, 0x00, 0x00, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            if (Rnd.Int(2) == 0)
            {
                return (int)Math.Round(1.5f * damage, MidpointRounding.AwayFromZero);
            }
            else
            {
                return 0;
            }
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