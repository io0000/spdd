using System;
using System.Collections.Generic;
using watabou.gltextures;
using watabou.glwrap;
using watabou.noosa;
using watabou.noosa.ui;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.scenes;
using spdd.ui;
using spdd.sprites;
using spdd.messages;

namespace spdd.windows
{
    public class WndHero : WndTabbed
    {
        private const int WIDTH = 115;
        private const int HEIGHT = 100;

        private StatsTab stats;
        private BuffsTab buffs;

        private Texture icons;
        private TextureFilm film;

        public WndHero()
        {
            Resize(WIDTH, HEIGHT);

            icons = TextureCache.Get(Assets.Interfaces.BUFFS_LARGE);
            film = new TextureFilm(icons, 16, 16);

            stats = new StatsTab();
            Add(stats);

            buffs = new BuffsTab(this);
            Add(buffs);
            buffs.SetRect(0, 0, WIDTH, HEIGHT);
            buffs.SetupList();

            var tab1 = new WndHeroLabledTab(this, Messages.Get(this, "stats"));
            tab1.action = (value) => stats.visible = stats.active = tab1.selected;
            Add(tab1);

            var tab2 = new WndHeroLabledTab(this, Messages.Get(this, "buffs"));
            tab2.action = (value) => buffs.visible = buffs.active = tab2.selected;
            Add(tab2);

            LayoutTabs();

            Select(0);
        }

        private class WndHeroLabledTab : LabeledTab
        {
            public WndHeroLabledTab(WndTabbed owner, string label)
                : base(owner, label)
            { }

            public Action<bool> action;

            public override void Select(bool value)
            {
                base.Select(value);
                action(value);
            }
        }

        private class StatsTab : Group
        {
            private const int GAP = 6;

            private float pos;

            public StatsTab()
            {
                Hero hero = Dungeon.hero;

                IconTitle title = new IconTitle();
                title.Icon(HeroSprite.Avatar(hero.heroClass, hero.Tier()));
                if (hero.Name().Equals(hero.ClassName()))
                    title.Label(Messages.Get(this, "title", hero.lvl, hero.ClassName()).ToUpperInvariant());
                else
                    title.Label((hero.Name() + "\n" + Messages.Get(this, "title", hero.lvl, hero.ClassName())).ToUpperInvariant());

                title.Color(Window.SHPX_COLOR);
                title.SetRect(0, 0, WIDTH, 0);
                Add(title);

                pos = title.Bottom() + 2 * GAP;

                StatSlot(Messages.Get(this, "str"), hero.GetSTR());
                if (hero.Shielding() > 0)
                    StatSlot(Messages.Get(this, "health"), hero.HP + "+" + hero.Shielding() + "/" + hero.HT);
                else
                    StatSlot(Messages.Get(this, "health"), (hero.HP) + "/" + hero.HT);

                StatSlot(Messages.Get(this, "exp"), hero.exp + "/" + hero.MaxExp());

                pos += GAP;

                StatSlot(Messages.Get(this, "gold"), Statistics.goldCollected);
                StatSlot(Messages.Get(this, "depth"), Statistics.deepestFloor);

                pos += GAP;
            }

            private void StatSlot(string label, string value)
            {
                RenderedTextBlock txt = PixelScene.RenderTextBlock(label, 8);
                txt.SetPos(0, pos);
                Add(txt);

                txt = PixelScene.RenderTextBlock(value, 8);
                txt.SetPos(WIDTH * 0.6f, pos);
                PixelScene.Align(txt);
                Add(txt);

                pos += GAP + txt.Height();
            }

            private void StatSlot(string label, int value)
            {
                StatSlot(label, value.ToString());
            }

            public virtual float Height()
            {
                return pos;
            }
        }  // StatsTab

        private class BuffsTab : Component
        {
            private WndHero wndHero;
            private const int GAP = 2;

            private float pos;
            private ScrollPane buffList;
            private List<BuffSlot> slots = new List<BuffSlot>();

            public BuffsTab(WndHero wndHero)
            {
                this.wndHero = wndHero;

                buffList = new BuffList(new Component());
                ((BuffList)buffList).tab = this;
                Add(buffList);
            }

            private class BuffList : ScrollPane
            {
                internal BuffsTab tab;

                public BuffList(Component content)
                    : base(content)
                { }

                public override void OnClick(float x, float y)
                {
                    foreach (var slot in tab.slots)
                    {
                        if (slot.OnClick(x, y))
                            break;
                    }
                }
            }

            protected override void Layout()
            {
                base.Layout();
                buffList.SetRect(0, 0, width, height);
            }

            public void SetupList()
            {
                Component content = buffList.Content();
                foreach (Buff buff in Dungeon.hero.Buffs())
                {
                    if (buff.Icon() != BuffIndicator.NONE)
                    {
                        BuffSlot slot = new BuffSlot(buff, wndHero);
                        slot.SetRect(0, pos, WIDTH, slot.icon.Height());
                        content.Add(slot);
                        slots.Add(slot);
                        pos += GAP + slot.Height();
                    }
                }
                content.SetSize(buffList.Width(), pos);
                buffList.SetSize(buffList.Width(), buffList.Height());
            }

            private class BuffSlot : Component
            {
                private WndHero wndHero;
                internal Buff buff;

                internal Image icon;
                internal RenderedTextBlock txt;

                public BuffSlot(Buff buff, WndHero wndHero)
                {
                    this.wndHero = wndHero;
                    this.buff = buff;
                    int index = buff.Icon();

                    icon = new Image(wndHero.icons);
                    icon.Frame(wndHero.film.Get(index));
                    buff.TintIcon(icon);
                    icon.y = this.y;
                    Add(icon);

                    txt = PixelScene.RenderTextBlock(buff.ToString(), 8);
                    txt.SetPos(
                            icon.width + GAP,
                            this.y + (icon.height - txt.Height()) / 2
                    );
                    PixelScene.Align(txt);
                    Add(txt);
                }

                protected override void Layout()
                {
                    base.Layout();
                    icon.y = this.y;
                    txt.SetPos(
                            icon.width + GAP,
                            this.y + (icon.height - txt.Height()) / 2
                    );
                }

                public bool OnClick(float x, float y)
                {
                    if (Inside(x, y))
                    {
                        GameScene.Show(new WndInfoBuff(buff));
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }  // BuffSlot
        }  // BuffsTab
    }
}