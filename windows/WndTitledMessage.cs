using watabou.noosa;
using watabou.noosa.ui;
using spdd.scenes;
using spdd.ui;

namespace spdd.windows
{
    public class WndTitledMessage : Window
    {
        protected internal const int WIDTH_MIN = 120;
        protected internal const int WIDTH_MAX = 220;
        protected internal const int GAP = 2;

        public WndTitledMessage(Image icon, string title, string message)
            : this(new IconTitle(icon, title), message)
        { }

        public WndTitledMessage(Component titlebar, string message) 
        {
            int width = WIDTH_MIN;

            titlebar.SetRect(0, 0, width, 0);
            Add(titlebar);

            RenderedTextBlock text = PixelScene.RenderTextBlock(6);
            text.Text(message, width);
            text.SetPos(titlebar.Left(), titlebar.Bottom() + 2 * GAP);
            Add(text);

            while (PixelScene.Landscape() &&
                text.Bottom() > (PixelScene.MIN_HEIGHT_L - 10) &&
                width < WIDTH_MAX)
            {
                width += 20;
                text.MaxWidth(width);
            }

            Resize(width, (int)text.Bottom() + 2);
        }
    }
}