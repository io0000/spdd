using spdd.plants;
using spdd.sprites;
using spdd.items.quest;
using spdd.items.stones;

namespace spdd.items.bags
{
    public class VelvetPouch : Bag
    {
        public VelvetPouch()
        {
            image = ItemSpriteSheet.POUCH;
        }

        public override bool CanHold(Item item)
        {
            if (item is Plant.Seed ||
                 item is Runestone ||
                 item is GooBlob ||
                 item is MetalShard)
            {
                return base.CanHold(item);
            }
            else
            {
                return false;
            }
        }

        public override int Capacity()
        {
            return 19;
        }

        public override int Value()
        {
            return 30;
        }
    }
}