using spdd.sprites;

namespace spdd.items.keys
{
    public class CrystalKey : Key
    {
        public CrystalKey()
            : this(0)
        { }

        public CrystalKey(int depth)
        {
            image = ItemSpriteSheet.CRYSTAL_KEY;
            this.depth = depth;
        }
    }
}