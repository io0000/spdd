using System;
using watabou.input;
using watabou.noosa;
using watabou.utils;
using spdd.messages;
using spdd.scenes;
using spdd.ui;

namespace spdd.windows
{
    public class WndStory : Window
    {
        private const int WIDTH_P = 125;
        private const int WIDTH_L = 160;
        private const int MARGIN = 2;

        //private const float bgR = 0.77f;
        //private const float bgG = 0.73f;
        //private const float bgB = 0.62f;

        public const int ID_SEWERS = 0;
        public const int ID_PRISON = 1;
        public const int ID_CAVES = 2;
        public const int ID_CITY = 3;
        public const int ID_HALLS = 4;

        private static readonly SparseArray<string> CHAPTERS = new SparseArray<string>();

        static WndStory()
        {
            CHAPTERS.Add(ID_SEWERS, "sewers");
            CHAPTERS.Add(ID_PRISON, "prison");
            CHAPTERS.Add(ID_CAVES, "caves");
            CHAPTERS.Add(ID_CITY, "city");
            CHAPTERS.Add(ID_HALLS, "halls");
        }

        private IconTitle ttl;
        private RenderedTextBlock tf;

        private float delay;

        public WndStory(string text)
            : this(null, null, text)
        { }

        public WndStory(Image icon, string title, string text)
            : base(0, 0, Chrome.Get(Chrome.Type.SCROLL))
        {
            int width = PixelScene.Landscape() ? WIDTH_L - MARGIN * 2 : WIDTH_P - MARGIN * 2;

            float y = MARGIN;
            if (icon != null && !string.ReferenceEquals(title, null))
            {
                ttl = new IconTitle(icon, title);
                ttl.SetRect(MARGIN, y, width - 2 * MARGIN, 0);
                y = ttl.Bottom() + MARGIN;
                Add(ttl);
                ttl.tfLabel.Invert();
            }

            tf = PixelScene.RenderTextBlock(text, 6);
            tf.MaxWidth(width);
            tf.Invert();
            tf.SetPos(MARGIN, y);
            Add(tf);

            Add(new WndStoryPointerArea(this, chrome));

            Resize((int)(tf.Width() + MARGIN * 2), (int)Math.Min(tf.Bottom() + MARGIN, 180));
        }

        private class WndStoryPointerArea : PointerArea
        {
            private WndStory wndStory;

            public WndStoryPointerArea(WndStory wndStory, NinePatch chrome)
                : base(chrome)
            {
                this.wndStory = wndStory;
            }

            public override void OnClick(PointerEvent ev)
            {
                wndStory.Hide();
            }
        }

        public override void Update()
        {
            base.Update();

            if (delay > 0 && (delay -= Game.elapsed) <= 0)
            {
                shadow.visible = chrome.visible = tf.visible = true;
                if (ttl != null)
                {
                    ttl.visible = true;
                }
            }
        }

        public static void ShowChapter(int id)
        {
            if (Dungeon.chapters.Contains(id))
            {
                return;
            }

            string text = Messages.Get(typeof(WndStory), CHAPTERS[id]);
            if (!string.IsNullOrEmpty(text))
            {
                WndStory wnd = new WndStory(text);
                if ((wnd.delay = 0.6f) > 0)
                {
                    wnd.shadow.visible = wnd.chrome.visible = wnd.tf.visible = false;
                    if (wnd.ttl != null)
                    {
                        wnd.ttl.visible = false;
                    }
                }

                Game.Scene().Add(wnd);

                Dungeon.chapters.Add(id);
            }
        }
    }
}