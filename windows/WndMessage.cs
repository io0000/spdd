using spdd.scenes;
using spdd.ui;

namespace spdd.windows
{
    public class WndMessage : Window
    {
        private const int WIDTH_P = 120;
        private const int WIDTH_L = 144;
        private const int MARGIN = 4;

        public WndMessage(string text)
        {
            RenderedTextBlock info = PixelScene.RenderTextBlock(text, 6);
            info.MaxWidth((PixelScene.Landscape() ? WIDTH_L : WIDTH_P) - MARGIN * 2);
            info.SetPos(MARGIN, MARGIN);
            Add(info);

            Resize(
                (int)info.Width() + MARGIN * 2,
                (int)info.Height() + MARGIN * 2);
        }
    }
}