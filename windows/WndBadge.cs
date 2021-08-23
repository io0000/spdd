using System;
using watabou.noosa;
using spdd.effects;
using spdd.scenes;
using spdd.ui;

namespace spdd.windows
{
    public class WndBadge : Window
    {
        private const int WIDTH = 120;
        private const int MARGIN = 4;

        public WndBadge(Badges.Badge badge)
        {
            Image icon = BadgeBanner.Image(badge.GetImage());
            icon.scale.Set(2);
            Add(icon);

            RenderedTextBlock info = PixelScene.RenderTextBlock(badge.Desc(), 8);
            info.MaxWidth(WIDTH - MARGIN * 2);
            info.Align(RenderedTextBlock.CENTER_ALIGN);
            PixelScene.Align(info);
            Add(info);

            float w = Math.Max(icon.Width(), info.Width()) + MARGIN * 2;

            icon.x = (w - icon.Width()) / 2f;
            icon.y = MARGIN;
            PixelScene.Align(icon);

            info.SetPos((w - info.Width()) / 2, icon.y + icon.Height() + MARGIN);
            Resize((int)w, (int)(info.Bottom() + MARGIN));

            BadgeBanner.Highlight(icon, badge.GetImage());
        }
    }
}