using spdd.sprites;

namespace spdd.items.quest
{
    public class GooBlob : Item
    {
        public GooBlob()
        {
            image = ItemSpriteSheet.BLOB;
            stackable = true;
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public override int Value()
        {
            return quantity * 50;
        }
    }
}