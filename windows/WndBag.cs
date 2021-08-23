using System;
using watabou.gltextures;
using watabou.input;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.hero;
using spdd.actors.mobs.npcs;
using spdd.items;
using spdd.items.armor;
using spdd.items.artifacts;
using spdd.items.bags;
using spdd.items.food;
using spdd.items.potions;
using spdd.items.scrolls;
using spdd.items.wands;
using spdd.items.weapon;
using spdd.items.weapon.melee;
using spdd.items.weapon.missiles;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;

namespace spdd.windows
{
    public class WndBag : WndTabbed
    {
        //only one wnditem can appear at a time
        private static WndBag instance;

        //FIXME this is getting cumbersome, there should be a better way to manage this
        public enum Mode
        {
            ALL,
            UNIDENTIFED,
            UNCURSABLE,
            CURSABLE,
            UPGRADEABLE,
            QUICKSLOT,
            FOR_SALE,
            WEAPON,
            ARMOR,
            ENCHANTABLE,
            WAND,
            SEED,
            FOOD,
            POTION,
            SCROLL,
            UNIDED_POTION_OR_SCROLL,
            EQUIPMENT,
            TRANMSUTABLE,
            ALCHEMY,
            RECYCLABLE,
            NOT_EQUIPPED
        }

        protected const int COLS_P = 5;
        protected const int COLS_L = 5;

        protected const int SLOT_WIDTH_P = 28;
        protected const int SLOT_WIDTH_L = 28;

        protected const int SLOT_HEIGHT_P = 28;
        protected const int SLOT_HEIGHT_L = 28;

        protected const int SLOT_MARGIN = 1;

        protected const int TITLE_HEIGHT = 14;

        private IListener listener;
        private WndBag.Mode mode;
        private string title;

        private int nCols;
        private int nRows;

        private int slotWidth;
        private int slotHeight;

        protected int count;
        protected int col;
        protected int row;

        private static Mode lastMode;
        private static Bag lastBag;

        public WndBag(Bag bag, IListener listener, Mode mode, string title)
        {
            if (instance != null)
                instance.Hide();

            instance = this;

            this.listener = listener;
            this.mode = mode;
            this.title = title;

            lastMode = mode;
            lastBag = bag;

            slotWidth = PixelScene.Landscape() ? SLOT_WIDTH_L : SLOT_WIDTH_P;
            slotHeight = PixelScene.Landscape() ? SLOT_HEIGHT_L : SLOT_HEIGHT_P;

            nCols = PixelScene.Landscape() ? COLS_L : COLS_P;
            nRows = (int)Math.Ceiling(25 / (float)nCols); //we expect to lay out 25 slots in all cases

            int windowWidth = slotWidth * nCols + SLOT_MARGIN * (nCols - 1);
            int windowHeight = TITLE_HEIGHT + slotHeight * nRows + SLOT_MARGIN * (nRows - 1);

            if (PixelScene.Landscape())
            {
                while (slotHeight >= 24 && (windowHeight + 20 + chrome.MarginTop()) > PixelScene.uiCamera.height)
                {
                    --slotHeight;
                    windowHeight -= nRows;
                }
            }
            else
            {
                while (slotWidth >= 26 && (windowWidth + chrome.MarginHor()) > PixelScene.uiCamera.width)
                {
                    --slotWidth;
                    windowWidth -= nCols;
                }
            }

            PlaceTitle(bag, windowWidth);

            PlaceItems(bag);

            Resize(windowWidth, windowHeight);

            Belongings stuff = Dungeon.hero.belongings;
            Bag[] bags = {
                stuff.backpack,
                stuff.GetItem<VelvetPouch>(),
                stuff.GetItem<ScrollHolder>(),
                stuff.GetItem<PotionBandolier>(),
                stuff.GetItem<MagicalHolster>()
            };

            foreach (Bag b in bags)
            {
                if (b != null)
                {
                    BagTab tab = new BagTab(this, b);
                    Add(tab);
                    tab.Select(b == bag);
                }
            }

            LayoutTabs();
        }

        public static WndBag LastBag(IListener listener, Mode mode, string title)
        {
            if (mode == lastMode &&
                lastBag != null &&
                Dungeon.hero.belongings.backpack.Contains(lastBag))
            {
                return new WndBag(lastBag, listener, mode, title);
            }
            else
            {
                return new WndBag(Dungeon.hero.belongings.backpack, listener, mode, title);
            }
        }

        public static WndBag GetBag<T>(IListener listener, Mode mode, string title) where T : Bag
        {
            Bag bag = (Bag)Dungeon.hero.belongings.GetItem<T>();
            return bag != null ?
                    new WndBag(bag, listener, mode, title) :
                    LastBag(listener, mode, title);
        }

        protected void PlaceTitle(Bag bag, int width)
        {
            ItemSprite gold = new ItemSprite(ItemSpriteSheet.GOLD, null);
            gold.x = width - gold.Width() - 1;
            gold.y = (TITLE_HEIGHT - gold.Height()) / 2f - 1;
            PixelScene.Align(gold);
            Add(gold);

            BitmapText amt = new BitmapText(Dungeon.gold.ToString(), PixelScene.pixelFont);
            amt.Hardlight(TITLE_COLOR);
            amt.Measure();
            amt.x = width - gold.Width() - amt.Width() - 2;
            amt.y = (TITLE_HEIGHT - amt.BaseLine()) / 2f - 1;
            PixelScene.Align(amt);
            Add(amt);

            RenderedTextBlock txtTitle = PixelScene.RenderTextBlock(
                    title != null ? Messages.TitleCase(title) : Messages.TitleCase(bag.Name()), 8);
            txtTitle.Hardlight(TITLE_COLOR);
            txtTitle.MaxWidth((int)amt.x - 2);
            txtTitle.SetPos(
                    1,
                    (TITLE_HEIGHT - txtTitle.Height()) / 2f - 1
            );
            PixelScene.Align(txtTitle);
            Add(txtTitle);
        }

        protected void PlaceItems(Bag container)
        {
            // Equipped items
            Belongings stuff = Dungeon.hero.belongings;

            PlaceItem(stuff.weapon != null ? (Item)stuff.weapon : new Placeholder(ItemSpriteSheet.WEAPON_HOLDER));
            PlaceItem(stuff.armor != null ? (Item)stuff.armor : new Placeholder(ItemSpriteSheet.ARMOR_HOLDER));
            PlaceItem(stuff.artifact != null ? (Item)stuff.artifact : new Placeholder(ItemSpriteSheet.ARTIFACT_HOLDER));
            PlaceItem(stuff.misc != null ? (Item)stuff.misc : new Placeholder(ItemSpriteSheet.SOMETHING));
            PlaceItem(stuff.ring != null ? (Item)stuff.ring : new Placeholder(ItemSpriteSheet.RING_HOLDER));

            //the container itself if it's not the root backpack
            if (container != Dungeon.hero.belongings.backpack)
            {
                PlaceItem(container);
                --count; //don't count this one, as it's not actually inside of itself
            }

            // Items in the bag, except other containers (they have tags at the bottom)
            foreach (Item item in container.items.ToArray())
            {
                if (!(item is Bag))
                {
                    PlaceItem(item);
                }
                else
                {
                    ++count;
                }
            }

            // Free Space
            while ((count - 5) < container.Capacity())
            {
                PlaceItem(null);
            }
        }

        protected void PlaceItem(Item item)
        {
            ++count;

            int x = col * (slotWidth + SLOT_MARGIN);
            int y = TITLE_HEIGHT + row * (slotHeight + SLOT_MARGIN);

            Add(new ItemButton(this, item).SetPos(x, y));

            if (++col >= nCols)
            {
                col = 0;
                ++row;
            }
        }

        public override bool OnSignal(KeyEvent ev)
        {
            if (ev.pressed && KeyBindings.GetActionForKey(ev) == SPDAction.INVENTORY)
            {
                Hide();
                return true;
            }
            else
            {
                return base.OnSignal(ev);
            }
        }

        public override void OnBackPressed()
        {
            if (listener != null)
                listener.OnSelect(null);

            base.OnBackPressed();
        }

        public override void OnClick(Tab tab)
        {
            Hide();
            Game.Scene().AddToFront(new WndBag(((BagTab)tab).bag, listener, mode, title));
        }

        public override void Hide()
        {
            base.Hide();
            if (instance == this)
            {
                instance = null;
            }
        }

        public override int TabHeight()
        {
            return 20;
        }

        private Image Icon(Bag bag)
        {
            if (bag is VelvetPouch)
            {
                return Icons.SEED_POUCH.Get();
            }
            else if (bag is ScrollHolder)
            {
                return Icons.SCROLL_HOLDER.Get();
            }
            else if (bag is MagicalHolster)
            {
                return Icons.WAND_HOLSTER.Get();
            }
            else if (bag is PotionBandolier)
            {
                return Icons.POTION_BANDOLIER.Get();
            }
            else
            {
                return Icons.BACKPACK.Get();
            }
        }

        private class BagTab : IconTab
        {
            internal Bag bag;

            public BagTab(WndTabbed owner, Bag bag)
                : base(owner, ((WndBag)owner).Icon(bag))
            {
                this.bag = bag;
            }
        }

        [SPDStatic]
        public class Placeholder : Item
        {
            public Placeholder(int image)
            {
                this.image = image;
            }

            public override string Name()
            {
                return null;
            }

            public override bool IsIdentified()
            {
                return true;
            }

            public override bool IsEquipped(Hero hero)
            {
                return true;
            }
        }

        private class ItemButton : ItemSlot
        {
            private static Color NORMAL = new Color(0x53, 0x56, 0x4D, 0x99); // 0x99 53 56 4D
            private static Color EQUIPPED = new Color(0x91, 0x93, 0x8C, 0x99);  // 0x99 91 93 8C;

            private WndBag owner;
            //private Item item; ItemSlot aleady has item
            private ColorBlock bg;

            public ItemButton(WndBag owner, Item item)
                : base(item)
            {
                this.owner = owner;
                this.item = item;

                ItemHelper(item);   // Item(Item item) 함수 주석참고

                if (item is Gold || item is Bag)
                    bg.visible = false;

                width = owner.slotWidth;
                height = owner.slotHeight;
            }

            protected override void CreateChildren()
            {
                bg = new ColorBlock(1, 1, NORMAL);
                Add(bg);

                base.CreateChildren();
            }

            protected override void Layout()
            {
                bg.Size(width, height);
                bg.x = x;
                bg.y = y;

                base.Layout();
            }

            // 문제: ItemButton은 ItemSlot에서 상속받음
            //       ItemButton(WndBag owner, Item item) 에서 Item(Item item)가 호출됨
            //       Item(Item item)은 virtual 함수라서 문제 생김
            //       https://stackoverflow.com/questions/119506/virtual-member-call-in-a-constructor/119543#119543
            // 우회책: 2개 함수로 분리, owner null검사, ItemButton(WndBag owner, Item item)에서 ItemHelper호출
            public override void Item(Item item)
            {
                base.Item(item);

                if (owner == null)
                    return;

                ItemHelper(item);
            }

            public void ItemHelper(Item item)
            {
                if (item != null)
                {
                    bg.Texture(TextureCache.CreateSolid(item.IsEquipped(Dungeon.hero) ? EQUIPPED : NORMAL));

                    if (item.cursed && item.cursedKnown)
                    {
                        bg.ra = +0.3f;
                        bg.ga = -0.15f;
                    }
                    else if (!item.IsIdentified())
                    {
                        if ((item is EquipableItem || item is Wand) && item.cursedKnown)
                        {
                            bg.ba = 0.3f;
                        }
                        else
                        {
                            bg.ra = 0.3f;
                            bg.ba = 0.3f;
                        }
                    }

                    if (item.Name() == null)
                    {
                        Enable(false);
                    }
                    else
                    {
                        var mode = owner.mode;

                        Enable(
                            mode == Mode.FOR_SALE && Shopkeeper.WillBuyItem(item) ||
                            mode == Mode.UPGRADEABLE && item.IsUpgradable() ||
                            mode == Mode.UNIDENTIFED && !item.IsIdentified() ||
                            mode == Mode.UNCURSABLE && ScrollOfRemoveCurse.Uncursable(item) ||
                            mode == Mode.CURSABLE && ((item is EquipableItem && !(item is MissileWeapon)) || item is Wand) ||
                            mode == Mode.QUICKSLOT && (item.defaultAction != null) ||
                            mode == Mode.WEAPON && (item is MeleeWeapon) ||
                            mode == Mode.ARMOR && (item is Armor) ||
                            mode == Mode.ENCHANTABLE && (item is MeleeWeapon || item is SpiritBow || item is Armor) ||
                            mode == Mode.WAND && (item is Wand) ||
                            mode == Mode.SEED && SandalsOfNature.CanUseSeed(item) ||
                            mode == Mode.FOOD && (item is Food) ||
                            mode == Mode.POTION && (item is Potion) ||
                            mode == Mode.SCROLL && (item is Scroll) ||
                            mode == Mode.UNIDED_POTION_OR_SCROLL && (!item.IsIdentified() && (item is Scroll || item is Potion)) ||
                            mode == Mode.EQUIPMENT && (item is EquipableItem || item is Wand) ||
                            mode == Mode.ALCHEMY && Recipe.UsableInRecipe(item) ||
                            mode == Mode.TRANMSUTABLE && ScrollOfTransmutation.CanTransmute(item) ||
                            mode == Mode.NOT_EQUIPPED && !item.IsEquipped(Dungeon.hero) ||
                            mode == Mode.RECYCLABLE && items.spells.Recycle.IsRecyclable(item) ||
                            mode == Mode.ALL
                        );
                    }
                }
                else
                {
                    bg.SetColor(NORMAL);
                }
            }

            protected override void OnPointerDown()
            {
                bg.Brightness(1.5f);
                Sample.Instance.Play(Assets.Sounds.CLICK, 0.7f, 0.7f, 1.2f);
            }

            protected override void OnPointerUp()
            {
                bg.Brightness(1.0f);
            }

            protected override void OnClick()
            {
                if (lastBag != item && !lastBag.Contains(item) && !item.IsEquipped(Dungeon.hero))
                {
                    owner.Hide();
                }
                else if (owner.listener != null)
                {
                    owner.Hide();
                    owner.listener.OnSelect(item);
                }
                else
                {
                    Game.Scene().AddToFront(new WndUseItem(owner, item));
                }
            }

            protected override bool OnLongClick()
            {
                if (owner.listener == null && item.defaultAction != null)
                {
                    owner.Hide();
                    Dungeon.quickslot.SetSlot(0, item);
                    QuickSlotButton.Refresh();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public interface IListener
        {
            void OnSelect(Item item);
        }
    }
}