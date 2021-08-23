using spdd.items.scrolls;
using spdd.items.spells;
using spdd.sprites;

namespace spdd.items.bags
{
    public class ScrollHolder : Bag
    {
        public ScrollHolder()
        {
            image = ItemSpriteSheet.HOLDER;
        }

        public override bool CanHold(Item item)
        {
            if (item is Scroll ||
                item is Spell)
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

        protected override void OnDetach()
        {
            base.OnDetach();

            foreach (var item in items)
            {
                if (item is BeaconOfReturning)
                    ((BeaconOfReturning)item).returnDepth = -1;
            }
        }

        public override int Value()
        {
            return 40;
        }
    }
}