using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.plants
{
    public class Stormvine : Plant
    {
        public Stormvine()
        {
            image = 5;
            seedClass = typeof(Seed);
        }

        public override void Activate(Character ch)
        {
            if (ch != null)
            {
                if (ch is Hero && ((Hero)ch).subClass == HeroSubClass.WARDEN)
                {
                    Buff.Affect<Levitation>(ch, Levitation.DURATION / 2f);
                }
                else
                {
                    Buff.Affect<Vertigo>(ch, Vertigo.DURATION);
                }
            }
        }

        [SPDStatic]
        public new class Seed : Plant.Seed
        {
            public Seed()
            {
                image = ItemSpriteSheet.SEED_STORMVINE;

                plantClass = typeof(Stormvine);
            }
        }
    }
}