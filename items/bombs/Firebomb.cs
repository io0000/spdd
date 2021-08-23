using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.blobs;
using spdd.sprites;
using spdd.scenes;
using spdd.effects;
using spdd.effects.particles;
using spdd.utils;

namespace spdd.items.bombs
{
    public class Firebomb : Bomb
    {
        public Firebomb()
        {
            image = ItemSpriteSheet.FIRE_BOMB;
        }

        public override void Explode(int cell)
        {
            base.Explode(cell);

            PathFinder.BuildDistanceMap(cell, BArray.Not(Dungeon.level.solid, null), 2);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue)
                {
                    if (Dungeon.level.pit[i])
                        GameScene.Add(Blob.Seed(i, 2, typeof(Fire)));
                    else
                        GameScene.Add(Blob.Seed(i, 10, typeof(Fire)));

                    CellEmitter.Get(i).Burst(FlameParticle.Factory, 5);
                }
            }
            Sample.Instance.Play(Assets.Sounds.BURNING);
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (20 + 30);
        }
    }
}