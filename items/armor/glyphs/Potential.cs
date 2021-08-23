using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.effects.particles;
using spdd.sprites;

namespace spdd.items.armor.glyphs
{
    public class Potential : Armor.Glyph
    {
        private static readonly ItemSprite.Glowing WHITE = new ItemSprite.Glowing(new Color(0xFF, 0xFF, 0xFF, 0xFF), 0.6f);

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            int level = Math.Max(0, armor.BuffedLvl());

            // lvl 0 - 16.7%
            // lvl 1 - 28.6%
            // lvl 2 - 37.5%
            if (defender is Hero && Rnd.Int(level + 6) >= 5)
            {
                int wands = ((Hero)defender).belongings.Charge(1f);
                if (wands > 0)
                {
                    defender.sprite.CenterEmitter().Burst(EnergyParticle.Factory, 10);
                }
            }

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return WHITE;
        }
    }
}