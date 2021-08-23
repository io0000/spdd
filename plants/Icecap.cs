using watabou.utils;
using spdd.actors;
using spdd.actors.blobs;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.sprites;
using spdd.utils;

namespace spdd.plants
{
    public class Icecap : Plant
    {
        public Icecap()
        {
            image = 4;
            seedClass = typeof(Seed);
        }

        public override void Activate(Character ch)
        {
            if (ch is Hero && ((Hero)ch).subClass == HeroSubClass.WARDEN)
            {
                Buff.Affect<FrostImbue>(ch, FrostImbue.DURATION * 0.3f);
            }

            PathFinder.BuildDistanceMap(pos, BArray.Not(Dungeon.level.losBlocking, null), 1);

            Fire fire = (Fire)Dungeon.level.GetBlob(typeof(Fire));

            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue)
                {
                    Freezing.Affect(i, fire);
                }
            }
        }

        [SPDStatic]
        public new class Seed : Plant.Seed
        {
            public Seed()
            {
                image = ItemSpriteSheet.SEED_ICECAP;

                plantClass = typeof(Icecap);
            }
        }
    }
}