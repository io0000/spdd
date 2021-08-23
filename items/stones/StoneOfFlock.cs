using watabou.utils;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.mobs.npcs;
using spdd.sprites;
using spdd.scenes;
using spdd.effects;

namespace spdd.items.stones
{
    public class StoneOfFlock : Runestone
    {
        public StoneOfFlock()
        {
            image = ItemSpriteSheet.STONE_FLOCK;
        }

        protected override void Activate(int cell)
        {
            foreach (int i in PathFinder.NEIGHBORS9)
            {
                if (!Dungeon.level.solid[cell + i] &&
                    !Dungeon.level.pit[cell + i] &&
                    Actor.FindChar(cell + i) == null)
                {
                    Sheep sheep = new Sheep();
                    sheep.lifespan = Rnd.IntRange(5, 8);
                    sheep.pos = cell + i;
                    GameScene.Add(sheep);
                    Dungeon.level.OccupyCell(sheep);

                    CellEmitter.Get(sheep.pos).Burst(Speck.Factory(Speck.WOOL), 4);
                }
            }
            CellEmitter.Get(cell).Burst(Speck.Factory(Speck.WOOL), 4);

            Sample.Instance.Play(Assets.Sounds.PUFF);
            Sample.Instance.Play(Assets.Sounds.SHEEP);
        }
    }
}