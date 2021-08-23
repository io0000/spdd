using System;
using spdd.windows;
using spdd.ui;
using spdd.sprites;
using spdd.scenes;
using spdd.items.weapon;
using spdd.items.armor;
using spdd.items.scrolls;
using spdd.messages;

namespace spdd.items.stones
{
    public class StoneOfAugmentation : InventoryStone
    {
        public StoneOfAugmentation()
        {
            mode = WndBag.Mode.ENCHANTABLE;
            image = ItemSpriteSheet.STONE_AUGMENTATION;
        }

        protected override void OnItemSelected(Item item)
        {
            GameScene.Show(new WndAugment(this, item));
        }

        public void Apply(Weapon weapon, Weapon.Augment augment)
        {
            weapon.augment = augment;
            UseAnimation();
            ScrollOfUpgrade.Upgrade(curUser);
        }

        public void Apply(Armor armor, Armor.Augment augment)
        {
            armor.augment = augment;
            UseAnimation();
            ScrollOfUpgrade.Upgrade(curUser);
        }

        public override int Value()
        {
            return 30 * quantity;
        }

        public class WndAugment : Window
        {
            private StoneOfAugmentation stone;
            private const int WIDTH = 120;
            private const int MARGIN = 2;
            private const int BUTTON_WIDTH = WIDTH - MARGIN * 2;
            private const int BUTTON_HEIGHT = 20;

            public WndAugment(StoneOfAugmentation stone, Item toAugment)
            {
                this.stone = stone;

                IconTitle titlebar = new IconTitle(toAugment);
                titlebar.SetRect(0, 0, WIDTH, 0);
                Add(titlebar);

                RenderedTextBlock tfMesage = PixelScene.RenderTextBlock(Messages.Get(this, "choice"), 8);
                tfMesage.MaxWidth(WIDTH - MARGIN * 2);
                tfMesage.SetPos(MARGIN, titlebar.Bottom() + MARGIN);
                Add(tfMesage);

                float pos = tfMesage.Top() + tfMesage.Height();

                if (toAugment is Weapon)
                {
                    foreach (Weapon.Augment aug in Enum.GetValues(typeof(Weapon.Augment)))
                    {
                        if (((Weapon)toAugment).augment != aug)
                        {
                            var btnSpeed = new ActionRedButton(Messages.Get(this, aug.ToString()));
                            btnSpeed.action = () =>
                            {
                                Hide();
                                stone.Apply((Weapon)toAugment, aug);
                            };

                            btnSpeed.SetRect(MARGIN, pos + MARGIN, BUTTON_WIDTH, BUTTON_HEIGHT);
                            Add(btnSpeed);

                            pos = btnSpeed.Bottom();
                        }
                    }
                }
                else if (toAugment is Armor)
                {
                    foreach (Armor.Augment aug in Enum.GetValues(typeof(Armor.Augment)))
                    {
                        if (((Armor)toAugment).augment != aug)
                        {
                            var btnSpeed = new ActionRedButton(Messages.Get(this, aug.ToString()));
                            btnSpeed.action = () =>
                            {
                                Hide();
                                stone.Apply((Armor)toAugment, aug);
                            };

                            btnSpeed.SetRect(MARGIN, pos + MARGIN, BUTTON_WIDTH, BUTTON_HEIGHT);
                            Add(btnSpeed);

                            pos = btnSpeed.Bottom();
                        }
                    }
                }

                var btnCancel = new ActionRedButton(Messages.Get(this, "cancel"));
                btnCancel.action = () =>
                {
                    Hide();
                    stone.Collect();
                };

                btnCancel.SetRect(MARGIN, pos + MARGIN, BUTTON_WIDTH, BUTTON_HEIGHT);
                Add(btnCancel);

                Resize(WIDTH, (int)btnCancel.Bottom() + MARGIN);
            }

            public override void OnBackPressed()
            {
                stone.Collect();
                base.OnBackPressed();
            }
        }
    }
}