using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.effects;
using spdd.effects.particles;
using spdd.sprites;

namespace spdd.plants
{
    public class Sorrowmoss : Plant
    {
        public Sorrowmoss()
        {
            image = 6;
            seedClass = typeof(Seed);
        }

        public override void Activate(Character ch)
        {
            if (ch is Hero && ((Hero)ch).subClass == HeroSubClass.WARDEN)
            {
                Buff.Affect<ToxicImbue>(ch).Set(ToxicImbue.DURATION * 0.3f);
            }

            if (ch != null)
            {
                Buff.Affect<Poison>(ch).Set(5 + (int)Math.Round(2 * Dungeon.depth / 3f, MidpointRounding.AwayFromZero));
            }

            if (Dungeon.level.heroFOV[pos])
            {
                CellEmitter.Center(pos).Burst(PoisonParticle.Splash, 3);
            }
        }

        [SPDStatic]
        public new class Seed : Plant.Seed
        {
            public Seed()
            {
                image = ItemSpriteSheet.SEED_SORROWMOSS;

                plantClass = typeof(Sorrowmoss);
            }
        }
    }
}