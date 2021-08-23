using spdd.sprites;

namespace spdd.items.keys
{
    public class GoldenKey : Key
    {
        public GoldenKey()
           : this(0)
        { }

        public GoldenKey(int depth)
        {
            image = ItemSpriteSheet.GOLDEN_KEY;
            this.depth = depth;
        }
    }
}