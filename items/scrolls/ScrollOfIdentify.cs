using spdd.effects;
using spdd.utils;
using spdd.windows;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.scrolls
{
    public class ScrollOfIdentify : InventoryScroll
    {
        public ScrollOfIdentify()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_IDENTIFY;
            mode = WndBag.Mode.UNIDENTIFED;

            bones = true;
        }

        public override void OnItemSelected(Item item)
        {
            curUser.sprite.parent.Add(new Identification(curUser.sprite.Center().Offset(0, -16)));

            item.Identify();
            GLog.Information(Messages.Get(this, "it_is", item));

            BadgesExtensions.ValidateItemLevelAquired(item);
        }

        public override int Value()
        {
            return IsKnown() ? 30 * quantity : base.Value();
        }
    }
}