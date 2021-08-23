using System;
using watabou.utils;
using watabou.noosa;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;
using spdd.plants;
using spdd.sprites;

using EarthParticle = spdd.effects.particles.EarthParticle;

namespace spdd.items.armor.glyphs
{
    public class Entanglement : Armor.Glyph
    {
        private static readonly ItemSprite.Glowing BROWN = new ItemSprite.Glowing(new Color(0x66, 0x33, 0x00, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            if (Rnd.Int(4) == 0)
            {
                int level = Math.Max(0, armor.GetLevel());

                Buff.Affect<Earthroot.Armor>(defender).Level(5 + 2 * level);
                CellEmitter.Bottom(defender.pos).Start(EarthParticle.Factory, 0.05f, 8);
                Camera.main.Shake(1, 0.4f);
            }

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return BROWN;
        }
    }
}