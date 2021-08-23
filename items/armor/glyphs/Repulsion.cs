using System;
using watabou.utils;
using spdd.actors;
using spdd.items.wands;
using spdd.sprites;
using spdd.mechanics;

namespace spdd.items.armor.glyphs
{
    public class Repulsion : Armor.Glyph
    {
        private static readonly ItemSprite.Glowing WHITE = new ItemSprite.Glowing(new Color(0xFF, 0xFF, 0xFF, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            // lvl 0 - 20%
            // lvl 1 - 33%
            // lvl 2 - 43%
            int level = Math.Max(0, armor.BuffedLvl());

            if (Rnd.Int(level + 5) >= 4)
            {
                int oppositeHero = attacker.pos + (attacker.pos - defender.pos);
                Ballistic trajectory = new Ballistic(attacker.pos, oppositeHero, Ballistic.MAGIC_BOLT);
                WandOfBlastWave.ThrowChar(attacker, trajectory, 2, true);
            }

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return WHITE;
        }
    }
}