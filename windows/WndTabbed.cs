using System.Collections.Generic;
using spdd.scenes;
using spdd.ui;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.ui;
using watabou.utils;

namespace spdd.windows
{
    public class WndTabbed : Window
    {
        protected List<Tab> tabs = new List<Tab>();
        protected Tab selected;

        public WndTabbed()
            : base(0, 0, Chrome.Get(Chrome.Type.TAB_SET))
        { }

        public Tab Add(Tab tab)
        {
            tab.SetPos(tabs.Count == 0 ? -chrome.MarginLeft() + 1 : tabs[tabs.Count - 1].Right(), height);
            tab.Select(false);
            base.Add(tab);

            tabs.Add(tab);

            return tab;
        }

        public void Select(int index)
        {
            Select(tabs[index]);
        }

        public virtual void Select(Tab tab)
        {
            if (tab != selected)
            {
                foreach (var t in tabs)
                {
                    if (t == selected)
                        t.Select(false);
                    else if (t == tab)
                        t.Select(true);
                }

                selected = tab;
            }
        }

        public override void Resize(int w, int h)
        {
            // -> super.Resize(...)
            this.width = w;
            this.height = h;

            chrome.Size(
                width + chrome.MarginHor(),
                height + chrome.MarginVer());

            camera.Resize((int)chrome.width, chrome.MarginTop() + height + TabHeight());
            camera.x = (int)(Game.width - camera.ScreenWidth()) / 2;
            camera.y = (int)(Game.height - camera.ScreenHeight()) / 2;
            camera.y += (int)(yOffset * camera.zoom);

            shadow.BoxRect(
                    camera.x / camera.zoom,
                    camera.y / camera.zoom,
                    chrome.Width(), chrome.height);
            // <- super.Resize(...)

            foreach (var tab in this.tabs)
                Remove(tab);

            var tabs = new List<Tab>(this.tabs);
            this.tabs.Clear();

            foreach (var tab in tabs)
                Add(tab);
        }

        public void LayoutTabs()
        {
            //subtract two as that horizontal space is transparent at the bottom
            int fullWidth = width + chrome.MarginHor() - 2;
            float numTabs = tabs.Count;
            float tabWidth = (fullWidth - (numTabs - 1)) / numTabs;

            float pos = -chrome.MarginLeft() + 1;
            foreach (var tab in tabs)
            {
                tab.SetSize(tabWidth, TabHeight());
                tab.SetPos(pos, height);
                pos = tab.Right() + 1;
                PixelScene.Align(tab);
            }
        }

        public virtual int TabHeight()
        {
            return 25;
        }

        public virtual void OnClick(Tab tab)
        {
            Select(tab);
        }

        public class Tab : Button
        {
            protected readonly WndTabbed owner;
            protected readonly int CUT = 5;

            internal bool selected;

            protected NinePatch bg;

            public Tab(WndTabbed owner)
            {
                this.owner = owner;
            }

            protected override void Layout()
            {
                base.Layout();

                if (bg != null)
                {
                    bg.x = x;
                    bg.y = y;

                    bg.Size(Width(), Height());
                }
            }

            public virtual void Select(bool value)
            {
                active = !(selected = value);

                if (bg != null)
                    Remove(bg);

                bg = Chrome.Get(selected ?
                    Chrome.Type.TAB_SELECTED :
                    Chrome.Type.TAB_UNSELECTED);
                AddToBack(bg);

                Layout();
            }

            protected override void OnClick()
            {
                Sample.Instance.Play(Assets.Sounds.CLICK, 0.7f, 0.7f, 1.2f);
                owner.OnClick(this);
            }
        }

        public class LabeledTab : Tab
        {
            protected RenderedTextBlock btLabel;

            public LabeledTab(WndTabbed owner, string label)
                : base(owner)
            {
                btLabel.Text(label);
            }

            protected override void CreateChildren()
            {
                base.CreateChildren();

                btLabel = PixelScene.RenderTextBlock(9);

                Add(btLabel);
            }

            protected override void Layout()
            {
                base.Layout();

                btLabel.SetPos(
                        x + (width - btLabel.Width()) / 2,
                        y + (height - btLabel.Height()) / 2 - (selected ? 1 : 3)
                );
                PixelScene.Align(btLabel);
            }

            public override void Select(bool value)
            {
                base.Select(value);
                btLabel.Alpha(selected ? 1.0f : 0.6f);
            }
        }

        public class IconTab : Tab
        {
            protected Image icon;
            private RectF defaultFrame;

            public IconTab(WndTabbed owner, Image icon)
                : base(owner)
            {
                this.icon.Copy(icon);
                this.defaultFrame = icon.Frame();
            }

            protected override void CreateChildren()
            {
                base.CreateChildren();

                icon = new Image();
                Add(icon);
            }

            protected override void Layout()
            {
                base.Layout();

                icon.Frame(defaultFrame);
                icon.x = x + (width - icon.width) / 2;
                icon.y = y + (height - icon.height) / 2 - 1;
                if (!selected)
                {
                    icon.y -= 2;
                    //if some of the icon is going into the window, cut it off
                    if (icon.y < y + CUT)
                    {
                        RectF frame = icon.Frame();
                        frame.top += (y + CUT - icon.y) / icon.texture.height;
                        icon.Frame(frame);
                        icon.y = y + CUT;
                    }
                }
                PixelScene.Align(icon);
            }

            public override void Select(bool value)
            {
                base.Select(value);
                icon.am = selected ? 1.0f : 0.6f;
            }
        }
    }
}