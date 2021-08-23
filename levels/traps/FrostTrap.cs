using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.blobs;
using spdd.effects;
using spdd.scenes;
using spdd.utils;

namespace spdd.levels.traps
{
    public class FrostTrap : Trap
    {
        public FrostTrap()
        {
            color = WHITE;
            shape = STARS;
        }

        public override void Activate()
        {
            if (Dungeon.level.heroFOV[pos])
            {
                Splash.At(pos, new Color(0xB2, 0xD6, 0xFF, 0xFF), 5);
                Sample.Instance.Play(Assets.Sounds.SHATTER);
            }

            PathFinder.BuildDistanceMap(pos, BArray.Not(Dungeon.level.solid, null), 2);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue)
                {
                    GameScene.Add(Blob.Seed(i, 20, typeof(Freezing)));
                }
            }
        }
    }
}