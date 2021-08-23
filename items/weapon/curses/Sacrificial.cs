using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.weapon.curses
{
    public class Sacrificial : Weapon.Enchantment
    {
        private static ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x00, 0x00, 0x00, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            if (Rnd.Int(12) == 0)
            {
                Buff.Affect<Bleeding>(attacker).Set(Math.Max(1, attacker.HP / 6));
            }

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