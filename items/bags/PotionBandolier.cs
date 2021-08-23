using spdd.items.potions;
using spdd.sprites;

namespace spdd.items.bags
{
    public class PotionBandolier : Bag
    {
        public PotionBandolier()
        {
            image = ItemSpriteSheet.BANDOLIER;
        }

        public override bool CanHold(Item item)
        {
            if (item is Potion)
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
            return 40;
        }
    }
}