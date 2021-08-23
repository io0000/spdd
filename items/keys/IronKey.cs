using spdd.sprites;

namespace spdd.items.keys
{
    public class IronKey : Key
    {
        public IronKey()
           : this(0)
        { }

        public IronKey(int depth)
        {
            image = ItemSpriteSheet.IRON_KEY;
            this.depth = depth;
        }
    }
}