using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.armor.glyphs
{
    public class Thorns : Armor.Glyph
    {
        private static readonly ItemSprite.Glowing RED = new ItemSprite.Glowing(new Color(0x66, 0x00, 0x22, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            int level = Math.Max(0, armor.BuffedLvl());

            // lvl 0 - 16.7%
            // lvl 1 - 23.1%
            // lvl 2 - 28.5%
            if (Rnd.Int(level + 12) >= 10)
            {
                Buff.Affect<Bleeding>(attacker).Set(4 + level);
            }

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return RED;
        }
    }
}