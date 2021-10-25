using System;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.ui;
using watabou.utils;
using spdd.items;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;

namespace spdd.windows
{
    public class WndRanking : WndTabbed
    {
        private const int WIDTH = 115;
        private const int HEIGHT = 144;

        //private Thread _thread;
        private string error;

        private Image busy;

        public WndRanking(Rankings.Record rec)
        {
            Resize(WIDTH, HEIGHT);

            //if (thread != null){
            //    hide();
            //    return;
            //}

            //thread = new Thread() {
            //    @Override
            //    public void run() {
            //	    try {
            //		    Badges.loadGlobal();
            //		    Rankings.INSTANCE.loadGameData( rec );
            //	    } catch ( Exception e ) {
            //		    error = Messages.Get(WndRanking.class, "error");
            //	    }
            //    }
            //};

            busy = Icons.BUSY.Get();
            busy.origin.Set(busy.width / 2, busy.height / 2);
            busy.angularSpeed = 720;
            busy.x = (WIDTH - busy.width) / 2;
            busy.y = (HEIGHT - busy.height) / 2;
            Add(busy);

            //thread.start();
            try
            {
                BadgesExtensions.LoadGlobal();
                Rankings.Instance.LoadGameData(rec);
            }
            catch (Exception)
            {
                error = Messages.Get(typeof(WndRanking), "error");
            }
        }


        public override void Update()
        {
            base.Update();

            //if (thread != null && !thread.isAlive() && busy != null) {
            //    if (error == null) {
            //        remove( busy );
            //        busy = null;
            //        if (Dungeon.hero != null) {
            //            createControls();
            //        } else {
            //            hide();
            //        }
            //    } else {
            //        hide();
            //        Game.scene().add( new WndError( error ) );
            //    }
            //}            

            if (busy != null)
            {
                if (error == null)
                {
                    Remove(busy);
                    busy = null;
                    if (Dungeon.hero != null)
                    {
                        CreateControls();
                    }
                    else
                    {
                        Hide();
                    }
                }
                else
                {
                    Hide();
                    Game.Scene().Add(new WndError(error));
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            //thread = null;
        }

        private void CreateControls()
        {
            string[] labels = {
                Messages.Get(this, "stats"),
                Messages.Get(this, "items"),
                Messages.Get(this, "badges")
            };

            Group[] pages = {
                new StatsTab(),
                new ItemsTab(this),
                new BadgesTab(this)
            };

            for (int i = 0; i < pages.Length; ++i)
            {
                Add(pages[i]);

                Tab tab = new RankingTab(this, labels[i], pages[i]);
                Add(tab);
            }

            LayoutTabs();

            Select(0);
        }

        private class RankingTab : LabeledTab
        {
            private Group page;

            public RankingTab(WndTabbed owner, string label, Group page)
                : base(owner, label)
            {
                this.page = page;
            }

            public override void Select(bool value)
            {
                base.Select(value);

                if (page != null)
                    page.visible = page.active = selected;
            }
        }

        private class StatsTab : Group
        {
            private int GAP = 5;

            public StatsTab()
            {
                if (Dungeon.challenges > 0)
                    --GAP;

                string heroClass = Dungeon.hero.ClassName();

                IconTitle title = new IconTitle();
                title.Icon(HeroSprite.Avatar(Dungeon.hero.heroClass, Dungeon.hero.Tier()));
                title.Label(Messages.Get(this, "title", Dungeon.hero.lvl, heroClass).ToUpperInvariant());
                title.Color(Window.SHPX_COLOR);
                title.SetRect(0, 0, WIDTH, 0);
                Add(title);

                float pos = title.Bottom() + GAP;

                if (Dungeon.challenges > 0)
                {
                    var btnChallenges = new ActionRedButton(Messages.Get(this, "challenges"));
                    btnChallenges.action = () => Game.Scene().Add(new WndChallenges(Dungeon.challenges, false));

                    float btnW = btnChallenges.ReqWidth() + 2;
                    btnChallenges.SetRect((WIDTH - btnW) / 2, pos, btnW, btnChallenges.ReqHeight() + 2);
                    Add(btnChallenges);

                    pos = btnChallenges.Bottom();
                }

                pos += GAP;

                pos = StatSlot(this, Messages.Get(this, "str"), Dungeon.hero.GetSTR().ToString(), pos);
                pos = StatSlot(this, Messages.Get(this, "health"), Dungeon.hero.HT.ToString(), pos);

                pos += GAP;

                pos = StatSlot(this, Messages.Get(this, "duration"), ((int)Statistics.duration).ToString(), pos);

                pos += GAP;

                pos = StatSlot(this, Messages.Get(this, "depth"), Statistics.deepestFloor.ToString(), pos);
                pos = StatSlot(this, Messages.Get(this, "enemies"), Statistics.enemiesSlain.ToString(), pos);
                pos = StatSlot(this, Messages.Get(this, "gold"), Statistics.goldCollected.ToString(), pos);

                pos += GAP;

                pos = StatSlot(this, Messages.Get(this, "food"), Statistics.foodEaten.ToString(), pos);
                pos = StatSlot(this, Messages.Get(this, "alchemy"), Statistics.potionsCooked.ToString(), pos);
                pos = StatSlot(this, Messages.Get(this, "ankhs"), Statistics.ankhsUsed.ToString(), pos);
            }

            private float StatSlot(Group parent, string label, string value, float pos)
            {
                RenderedTextBlock txt = PixelScene.RenderTextBlock(label, 7);
                txt.SetPos(0, pos);
                parent.Add(txt);

                txt = PixelScene.RenderTextBlock(value, 7);
                txt.SetPos(WIDTH * 0.7f, pos);
                PixelScene.Align(txt);
                parent.Add(txt);

                return pos + GAP + txt.Height();
            }
        }

        private class ItemsTab : Group
        {
            WndRanking wndRanking;
            private float pos;

            public ItemsTab(WndRanking wndRanking)
            {
                this.wndRanking = wndRanking;

                var stuff = Dungeon.hero.belongings;
                if (stuff.weapon != null)
                    AddItem(stuff.weapon);

                if (stuff.armor != null)
                    AddItem(stuff.armor);

                if (stuff.artifact != null)
                    AddItem(stuff.artifact);

                if (stuff.misc != null)
                    AddItem(stuff.misc);

                if (stuff.ring != null)
                    AddItem(stuff.ring);

                pos = 0;
                for (int i = 0; i < 4; ++i)
                {
                    if (Dungeon.quickslot.GetItem(i) != null)
                    {
                        QuickSlotButton slot = new QuickSlotButton(Dungeon.quickslot.GetItem(i));

                        slot.SetRect(pos, 120, 28, 23);

                        Add(slot);
                    }
                    else
                    {
                        ColorBlock bg = new ColorBlock(28, 23, new Color(0x53, 0x56, 0x4D, 0x99));
                        bg.x = pos;
                        bg.y = 120;
                        Add(bg);
                    }
                    pos += 29;
                }
            }

            private void AddItem(Item item)
            {
                var slot = new ItemButton(item);
                slot.SetRect(0, pos, wndRanking.width, ItemButton.HEIGHT);
                Add(slot);

                pos += slot.Height() + 1;
            }
        }

        private class BadgesTab : Group
        {
            public BadgesTab(WndRanking wndRanking)
            {
                camera = wndRanking.camera;

                ScrollPane list = new BadgesList(false);
                Add(list);

                list.SetSize(WIDTH, HEIGHT);
            }
        }

        private class ItemButton : Button
        {
            public const int HEIGHT = 23;

            private readonly Item item;

            private ItemSlot slot;
            private ColorBlock bg;
            private RenderedTextBlock name;

            public ItemButton(Item item)
            {
                this.item = item;

                slot.Item(item);
                if (item.cursed && item.cursedKnown)
                {
                    bg.ra = +0.2f;
                    bg.ga = -0.1f;
                }
                else if (!item.IsIdentified())
                {
                    bg.ra = 0.1f;
                    bg.ba = 0.1f;
                }
            }

            protected override void CreateChildren()
            {
                bg = new ColorBlock(28, HEIGHT, new Color(0x53, 0x56, 0x4D, 0x99));
                Add(bg);

                slot = new ItemSlot();
                Add(slot);

                name = PixelScene.RenderTextBlock(7);
                Add(name);

                base.CreateChildren();
            }

            protected override void Layout()
            {
                bg.x = x;
                bg.y = y;

                slot.SetRect(x, y, 28, HEIGHT);
                PixelScene.Align(slot);

                name.MaxWidth((int)(width - slot.Width() - 2));
                name.Text(Messages.TitleCase(item.Name()));
                name.SetPos(
                        slot.Right() + 2,
                        y + (height - name.Height()) / 2
                );
                PixelScene.Align(name);

                base.Layout();
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
                Game.Scene().Add(new WndInfoItem(item));
            }
        }

        private class QuickSlotButton : ItemSlot
        {
            public const int HEIGHT = 23;

            //private Item item;  warning CS0108: 'WndRanking.QuickSlotButton.item'Àº(´Â) »ó¼ÓµÈ 'ItemSlot.item' ¸â¹ö¸¦ ¼û±é´Ï´Ù.
            private ColorBlock bg;

            public QuickSlotButton(Item item)
                : base(item)
            {
                //this.item = item;
            }

            protected override void CreateChildren()
            {
                bg = new ColorBlock(28, HEIGHT, new Color(0x53, 0x56, 0x4D, 0x99));
                Add(bg);

                base.CreateChildren();
            }

            protected override void Layout()
            {
                bg.x = x;
                bg.y = y;

                base.Layout();
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
                Game.Scene().Add(new WndInfoItem(item));
            }
        }
    }
}