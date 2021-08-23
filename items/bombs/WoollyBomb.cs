using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.mobs.npcs;
using spdd.sprites;
using spdd.scenes;
using spdd.effects;
using spdd.utils;

namespace spdd.items.bombs
{
    public class WoollyBomb : Bomb
    {
        public WoollyBomb()
        {
            image = ItemSpriteSheet.WOOLY_BOMB;
        }

        public override void Explode(int cell)
        {
            base.Explode(cell);

            PathFinder.BuildDistanceMap(cell, BArray.Not(Dungeon.level.solid, null), 2);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue)
                {
                    if (Dungeon.level.InsideMap(i) &&
                        Actor.FindChar(i) == null &&
                        !Dungeon.level.pit[i])
                    {
                        var sheep = new Sheep();
                        sheep.lifespan = Rnd.NormalIntRange(8, 16);
                        sheep.pos = i;
                        Dungeon.level.OccupyCell(sheep);
                        GameScene.Add(sheep);
                        CellEmitter.Get(i).Burst(Speck.Factory(Speck.WOOL), 4);
                    }
                }
            }

            Sample.Instance.Play(Assets.Sounds.PUFF);
            Sample.Instance.Play(Assets.Sounds.SHEEP);
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (20 + 30);
        }
    }
}