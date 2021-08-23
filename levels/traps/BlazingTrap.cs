using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.blobs;
using spdd.effects;
using spdd.effects.particles;
using spdd.scenes;
using spdd.utils;

namespace spdd.levels.traps
{
    public class BlazingTrap : Trap
    {
        public BlazingTrap()
        {
            color = ORANGE;
            shape = STARS;
        }

        public override void Activate()
        {
            PathFinder.BuildDistanceMap(pos, BArray.Not(Dungeon.level.solid, null), 2);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue)
                {
                    if (Dungeon.level.pit[i] || Dungeon.level.water[i])
                        GameScene.Add(Blob.Seed(i, 1, typeof(Fire)));
                    else
                        GameScene.Add(Blob.Seed(i, 5, typeof(Fire)));

                    CellEmitter.Get(i).Burst(FlameParticle.Factory, 5);
                }
            }
            Sample.Instance.Play(Assets.Sounds.BURNING);
        }
    }
}