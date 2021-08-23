using watabou.noosa.audio;
using spdd.scenes;
using spdd.windows;
using spdd.messages;

namespace spdd.items.scrolls
{
    public abstract class InventoryScroll : Scroll
    {
        public string inventoryTitle;
        public WndBag.Mode mode = WndBag.Mode.ALL;

        protected InventoryScroll()
        {
            inventoryTitle = Messages.Get(this, "inv_title");
        }

        public override void DoRead()
        {
            if (!IsKnown())
            {
                SetKnown();
                identifiedByUse = true;
            }
            else
            {
                identifiedByUse = false;
            }

            GameScene.SelectItem(itemSelector, mode, inventoryTitle);
        }

        public void ConfirmCancelation()
        {
            var wnd = new WndOptions(
                Messages.TitleCase(Name()),
                Messages.Get(this, "warning"),
                Messages.Get(this, "yes"),
                Messages.Get(this, "no"));

            wnd.selectAction = (index) =>
            {
                switch (index)
                {
                    case 0:
                        curUser.SpendAndNext(TIME_TO_READ);
                        identifiedByUse = false;
                        break;
                    case 1:
                        GameScene.SelectItem(itemSelector, mode, inventoryTitle);
                        break;
                }
            };

            wnd.skipBackPressed = true;

            GameScene.Show(wnd);
        }

        public abstract void OnItemSelected(Item item);

        public static bool identifiedByUse;

        protected static WndBag.IListener itemSelector = new InventoryScrollListener();

        private class InventoryScrollListener : WndBag.IListener
        {
            public void OnSelect(Item item)
            {
                var curItem = Item.curItem;
                //FIXME this safety check shouldn't be necessary
                //it would be better to eliminate the curItem static variable.
                if (!(curItem is InventoryScroll))
                    return;

                if (item != null)
                {
                    ((InventoryScroll)curItem).OnItemSelected(item);
                    ((InventoryScroll)curItem).ReadAnimation();

                    Sample.Instance.Play(Assets.Sounds.READ);
                }
                else if (InventoryScroll.identifiedByUse && !((Scroll)curItem).anonymous)
                {
                    ((InventoryScroll)curItem).ConfirmCancelation();
                }
                else if (!((Scroll)curItem).anonymous)
                {
                    curItem.Collect(curUser.belongings.backpack);
                }
            }
        }
    }
}