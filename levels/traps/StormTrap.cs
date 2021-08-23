using watabou.utils;
using watabou.noosa.audio;
using spdd.scenes;
using spdd.actors.blobs;
using spdd.utils;

namespace spdd.levels.traps
{
    public class StormTrap : Trap
    {
        public StormTrap()
        {
            color = YELLOW;
            shape = STARS;
        }

        public override void Activate()
        {
            if (Dungeon.level.heroFOV[pos])
                Sample.Instance.Play(Assets.Sounds.LIGHTNING);

            PathFinder.BuildDistanceMap(pos, BArray.Not(Dungeon.level.solid, null), 2);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue)
                    GameScene.Add(Blob.Seed(i, 20, typeof(Electricity)));
            }
        }
    }
}