using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.effects;
using spdd.effects.particles;
using spdd.sprites;

namespace spdd.plants
{
    public class Rotberry : Plant
    {
        public Rotberry()
        {
            image = 0;
            seedClass = typeof(Seed);
        }

        public override void Activate(Character ch)
        {
            if (ch is Hero && ((Hero)ch).subClass == HeroSubClass.WARDEN)
            {
                Buff.Affect<AdrenalineSurge>(ch).Reset(1, 200f);
            }

            Dungeon.level.Drop(new Seed(), pos).sprite.Drop();
        }

        public override void Wither()
        {
            Dungeon.level.Uproot(pos);

            if (Dungeon.level.heroFOV[pos])
            {
                CellEmitter.Get(pos).Burst(LeafParticle.General, 6);
            }

            //no warden benefit
        }

        [SPDStatic]
        public new class Seed : Plant.Seed
        {
            public Seed()
            {
                image = ItemSpriteSheet.SEED_ROTBERRY;

                plantClass = typeof(Rotberry);

                unique = true;
            }

            public override int Value()
            {
                return 30 * quantity;
            }
        }
    }
}