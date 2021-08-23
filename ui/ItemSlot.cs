using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;
using spdd.items;
using spdd.items.armor;
using spdd.items.rings;
using spdd.items.weapon;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;

namespace spdd.ui
{
    public class ItemSlot : Button
    {
        public static Color DEGRADED = new Color(0xFF, 0x44, 0x44, 0xFF);
        public static Color UPGRADED = new Color(0x44, 0xFF, 0x44, 0xFF);
        //public static Color FADED = new Color(0x99, 0x99, 0x99, 0xFF);
        public static Color WARNING = new Color(0xFF, 0x88, 0x00, 0xFF);
        public static Color ENHANCED = new Color(0x33, 0x99, 0xFF, 0xFF);

        private const float ENABLED = 1.0f;
        private const float DISABLED = 0.3f;

        public ItemSprite sprite;
        public Item item;
        public BitmapText status;
        public BitmapText extra;
        public Image itemIcon;
        public BitmapText level;

        private const string TXT_STRENGTH = ":%d";
        private const string TXT_TYPICAL_STR = "%d?";

        private const string TXT_LEVEL = "%+d";

        // Special "virtual items"
        public static Item CHEST = new Item { image = ItemSpriteSheet.CHEST };
        public static Item LOCKED_CHEST = new Item { image = ItemSpriteSheet.LOCKED_CHEST };
        public static Item CRYSTAL_CHEST = new Item { image = ItemSpriteSheet.CRYSTAL_CHEST };
        public static Item TOMB = new Item { image = ItemSpriteSheet.TOMB };
        public static Item SKELETON = new Item { image = ItemSpriteSheet.BONES };
        public static Item REMAINS = new Item { image = ItemSpriteSheet.REMAINS };

        public ItemSlot()
        {
            sprite.SetVisible(false);
            Enable(false);
        }

        public ItemSlot(Item item)
            : this()
        {
            Item(item);
        }

        protected override void CreateChildren()
        {
            base.CreateChildren();

            sprite = new ItemSprite();
            Add(sprite);

            status = new BitmapText(PixelScene.pixelFont);
            Add(status);

            extra = new BitmapText(PixelScene.pixelFont);
            Add(extra);

            level = new BitmapText(PixelScene.pixelFont);
            Add(level);
        }

        protected override void Layout()
        {
            base.Layout();

            sprite.x = x + (width - sprite.width) / 2f;
            sprite.y = y + (height - sprite.height) / 2f;
            PixelScene.Align(sprite);

            if (status != null)
            {
                status.Measure();
                if (status.width > width)
                {
                    status.scale.Set(PixelScene.Align(0.8f));
                }
                else
                {
                    status.scale.Set(1f);
                }
                status.x = x;
                status.y = y;
                PixelScene.Align(status);
            }

            if (extra != null)
            {
                extra.x = x + (width - extra.Width());
                extra.y = y;
                PixelScene.Align(extra);
            }

            if (itemIcon != null)
            {
                itemIcon.x = x + width - (ItemSpriteSheet.Icons.SIZE + itemIcon.Width()) / 2f;
                itemIcon.y = y + (ItemSpriteSheet.Icons.SIZE - itemIcon.height) / 2f;
                PixelScene.Align(itemIcon);
            }

            if (level != null)
            {
                level.x = x + (width - level.Width());
                level.y = y + (height - level.BaseLine() - 1);
                PixelScene.Align(level);
            }
        }

        public virtual void Item(Item item)
        {
            if (this.item == item)
            {
                if (item != null)
                {
                    sprite.Frame(item.Image());
                    sprite.Glow(item.Glowing());
                }
                UpdateText();
                return;
            }

            this.item = item;

            if (item == null)
            {
                Enable(false);
                sprite.SetVisible(false);

                UpdateText();
            }
            else
            {
                Enable(true);
                sprite.SetVisible(true);

                sprite.View(item);
                UpdateText();
            }
        }

        private void UpdateText()
        {
            if (itemIcon != null)
            {
                Remove(itemIcon);
                itemIcon = null;
            }

            if (item == null)
            {
                status.visible = extra.visible = level.visible = false;
                return;
            }
            else
            {
                status.visible = extra.visible = level.visible = true;
            }

            status.Text(item.Status());

            if (item.icon != -1 && (item.IsIdentified() || (item is Ring && ((Ring)item).IsKnown())))
            {
                extra.Text(null);

                itemIcon = new Image(Assets.Sprites.ITEM_ICONS);
                itemIcon.Frame(ItemSpriteSheet.Icons.film.Get(item.icon));
                Add(itemIcon);

            }
            else if (item is Weapon || item is Armor)
            {
                if (item.levelKnown)
                {
                    int str = item is Weapon ? ((Weapon)item).STRReq() : ((Armor)item).STRReq();
                    extra.Text(Messages.Format(TXT_STRENGTH, str));
                    if (str > Dungeon.hero.GetSTR())
                    {
                        extra.Hardlight(DEGRADED);
                    }
                    else
                    {
                        extra.ResetColor();
                    }
                }
                else
                {
                    int str = item is Weapon ? ((Weapon)item).STRReq(0) : ((Armor)item).STRReq(0);
                    extra.Text(Messages.Format(TXT_TYPICAL_STR, str));
                    extra.Hardlight(WARNING);
                }
                extra.Measure();
            }
            else
            {
                extra.Text(null);
            }

            int trueLvl = item.VisiblyUpgraded();
            int buffedLvl = item.BuffedVisiblyUpgraded();

            if (trueLvl != 0 || buffedLvl != 0)
            {
                level.Text(Messages.Format(TXT_LEVEL, buffedLvl));
                level.Measure();
                if (trueLvl == buffedLvl || buffedLvl <= 0)
                {
                    level.Hardlight(buffedLvl > 0 ? UPGRADED : DEGRADED);
                }
                else
                {
                    level.Hardlight(buffedLvl > trueLvl ? ENHANCED : WARNING);
                }
            }
            else
            {
                level.Text(null);
            }

            Layout();
        }

        public void Enable(bool value)
        {
            active = value;

            float alpha = value ? ENABLED : DISABLED;
            sprite.Alpha(alpha);
            status.Alpha(alpha);
            extra.Alpha(alpha);
            level.Alpha(alpha);
            if (itemIcon != null)
                itemIcon.Alpha(alpha);
        }

        public void ShowExtraInfo(bool show)
        {
            if (show)
            {
                Add(extra);
            }
            else
            {
                Remove(extra);
            }
        }
    }
}