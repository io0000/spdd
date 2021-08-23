using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.blobs;
using spdd.sprites;
using spdd.scenes;
using spdd.utils;

namespace spdd.items.bombs
{
    public class FrostBomb : Bomb
    {
        public FrostBomb()
        {
            image = ItemSpriteSheet.FROST_BOMB;
        }

        public override void Explode(int cell)
        {
            base.Explode(cell);
            PathFinder.BuildDistanceMap(cell, BArray.Not(Dungeon.level.solid, null), 2);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue)
                {
                    GameScene.Add(Blob.Seed(i, 10, typeof(Freezing)));
                    var ch = Actor.FindChar(i);
                    if (ch != null)
                    {
                        Buff.Affect<Frost>(ch, 2f);
                    }
                }
            }
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (20 + 30);
        }
    }
}