using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.plants
{
    public class Starflower : Plant
    {
        public Starflower()
        {
            image = 9;
            seedClass = typeof(Seed);
        }

        public override void Activate(Character ch)
        {
            if (ch != null)
            {
                Buff.Prolong<Bless>(ch, Bless.DURATION);
                if (ch is Hero && ((Hero)ch).subClass == HeroSubClass.WARDEN)
                {
                    Buff.Prolong<Recharging>(ch, Recharging.DURATION);
                }
            }
        }

        [SPDStatic]
        public new class Seed : Plant.Seed
        {
            public Seed()
            {
                image = ItemSpriteSheet.SEED_STARFLOWER;

                plantClass = typeof(Starflower);
            }

            public override int Value()
            {
                return 30 * quantity;
            }
        }
    }
}