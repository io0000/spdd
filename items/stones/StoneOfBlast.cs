using spdd.sprites;
using spdd.items.bombs;

namespace spdd.items.stones
{
    public class StoneOfBlast : Runestone
    {
        public StoneOfBlast()
        {
            image = ItemSpriteSheet.STONE_BLAST;
        }

        protected override void Activate(int cell)
        {
            (new Bomb()).Explode(cell);
        }
    }
}