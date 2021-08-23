using System;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.ui;
using spdd.actors.hero;
using spdd.actors.mobs.npcs;
using spdd.items;
using spdd.messages;
using spdd.scenes;
using spdd.ui;

namespace spdd.windows
{
    public class WndBlacksmith : Window
    {
        private const int BTN_SIZE = 36;
        private const float GAP = 2;
        private const float BTN_GAP = 10;
        private const int WIDTH = 116;

        public ItemButton btnPressed;

        public ItemButton btnItem1;
        public ItemButton btnItem2;
        public ActionRedButton btnReforge;

        public WndBlacksmith(Blacksmith troll, Hero hero)
        {
            itemSelector = new BlacksmithItemSelector(this);

            IconTitle titlebar = new IconTitle();
            titlebar.Icon(troll.GetSprite());
            titlebar.Label(Messages.TitleCase(troll.Name()));
            titlebar.SetRect(0, 0, WIDTH, 0);
            Add(titlebar);

            RenderedTextBlock message = PixelScene.RenderTextBlock(Messages.Get(this, "prompt"), 6);
            message.MaxWidth(WIDTH);
            message.SetPos(0, titlebar.Bottom() + GAP);
            Add(message);

            btnItem1 = new ItemButton();
            btnItem1.action = () =>
            {
                btnPressed = btnItem1;
                GameScene.SelectItem(itemSelector, WndBag.Mode.UPGRADEABLE, Messages.Get(typeof(WndBlacksmith), "select"));
            };
            btnItem1.SetRect((WIDTH - BTN_GAP) / 2 - BTN_SIZE, message.Top() + message.Height() + BTN_GAP, BTN_SIZE, BTN_SIZE);
            Add(btnItem1);

            btnItem2 = new ItemButton();
            btnItem2.action = () =>
            {
                btnPressed = btnItem2;
                GameScene.SelectItem(itemSelector, WndBag.Mode.UPGRADEABLE, Messages.Get(typeof(WndBlacksmith), "select"));
            };
            btnItem2.SetRect(btnItem1.Right() + BTN_GAP, btnItem1.Top(), BTN_SIZE, BTN_SIZE);
            Add(btnItem2);

            btnReforge = new ActionRedButton(Messages.Get(this, "reforge"));
            btnReforge.action = () =>
            {
                Blacksmith.Upgrade(btnItem1.item, btnItem2.item);
                Hide();
            };

            btnReforge.Enable(false);
            btnReforge.SetRect(0, btnItem1.Bottom() + BTN_GAP, WIDTH, 20);
            Add(btnReforge);

            Resize(WIDTH, (int)btnReforge.Bottom());
        }

        protected BlacksmithItemSelector itemSelector;

        public class BlacksmithItemSelector : WndBag.IListener
        {
            private WndBlacksmith wnd;

            public BlacksmithItemSelector(WndBlacksmith wndBlacksmith)
            {
                wnd = wndBlacksmith;
            }

            public void OnSelect(Item item)
            {
                if (item != null && wnd.btnPressed.parent != null)
                {
                    wnd.btnPressed.Item(item);

                    if (wnd.btnItem1.item == null || wnd.btnItem2.item == null)
                        return;

                    var result = Blacksmith.Verify(wnd.btnItem1.item, wnd.btnItem2.item);
                    if (result != null)
                    {
                        GameScene.Show(new WndMessage(result));
                        wnd.btnReforge.Enable(false);
                    }
                    else
                    {
                        wnd.btnReforge.Enable(true);
                    }
                }
            }
        }

        public class ItemButton : Component
        {
            protected NinePatch bg;
            protected Slot slot;

            public Item item;

            protected override void CreateChildren()
            {
                base.CreateChildren();

                bg = Chrome.Get(Chrome.Type.RED_BUTTON);
                Add(bg);

                slot = new Slot(this);
                slot.Enable(true);
                Add(slot);
            }

            public Action action;

            public void OnClick()
            {
                if (action != null)
                    action();
            }

            protected override void Layout()
            {
                base.Layout();

                bg.x = x;
                bg.y = y;

                bg.Size(Width(), Height());

                slot.SetRect(x + 2, y + 2, Width() - 4, Height() - 4);
            }

            public void Item(Item item)
            {
                slot.Item(this.item = item);
            }

            public class Slot : ItemSlot
            {
                private ItemButton button;

                public Slot(ItemButton itemButton)
                {
                    button = itemButton;
                }

                protected override void OnPointerDown()
                {
                    button.bg.Brightness(1.2f);
                    Sample.Instance.Play(Assets.Sounds.CLICK);
                }

                protected override void OnPointerUp()
                {
                    button.bg.ResetColor();
                }

                protected override void OnClick()
                {
                    button.OnClick();
                }
            } 
        }
    }
}