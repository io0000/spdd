using System.Collections.Generic;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.ui;
using spdd.effects;
using spdd.scenes;
using spdd.windows;

namespace spdd.ui
{
    public class BadgesList : ScrollPane
    {
        private List<ListItem> items = new List<ListItem>();

        public BadgesList(bool global)
            : base(new Component())
        {
            foreach (var badge in BadgesExtensions.Filtered(global))
            {
                if (badge.GetImage() == -1)
                    continue;

                ListItem item = new ListItem(badge);
                Content().Add(item);
                items.Add(item);
            }
        }

        protected override void Layout()
        {
            float pos = 0;

            var size = items.Count;
            for (var i = 0; i < size; ++i)
            {
                items[i].SetRect(0, pos, width, ListItem.HEIGHT);
                pos += ListItem.HEIGHT;
            }

            content.SetSize(Width(), pos);

            base.Layout();
        }

        public override void OnClick(float x, float y)
        {
            int size = items.Count;

            for (int i = 0; i < size; ++i)
            {
                if (items[i].OnClick(x, y))
                    break;
            }
        }

        private class ListItem : Component
        {
            public const float HEIGHT = 20;

            private readonly Badges.Badge badge;

            private Image icon;
            private RenderedTextBlock label;

            public ListItem(Badges.Badge badge)
            {
                this.badge = badge;
                icon.Copy(BadgeBanner.Image(badge.GetImage()));
                label.Text(badge.Desc());
            }

            protected override void CreateChildren()
            {
                icon = new Image();
                Add(icon);

                label = PixelScene.RenderTextBlock(6);
                Add(label);
            }

            protected override void Layout()
            {
                icon.x = x;
                icon.y = y + (height - icon.height) / 2;
                PixelScene.Align(icon);

                label.SetPos(
                        icon.x + icon.width + 2,
                        y + (height - label.Height()) / 2
                );
                PixelScene.Align(label);
            }

            public bool OnClick(float x, float y)
            {
                if (Inside(x, y))
                {
                    Sample.Instance.Play(Assets.Sounds.CLICK, 0.7f, 0.7f, 1.2f);
                    Game.Scene().Add(new WndBadge(badge));
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}