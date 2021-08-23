using spdd.scenes;
using spdd.ui;
using spdd.items;

namespace spdd.windows
{
    public class WndInfoItem : Window
    {
        private const float GAP = 2;

        private const int WIDTH_MIN = 120;
        private const int WIDTH_MAX = 220;

        public WndInfoItem(Heap heap)
        {
            FillFields(heap);
        }

        public WndInfoItem(Item item)
        {
            FillFields(item);
        }

        private void FillFields(Heap heap)
        {
            IconTitle titlebar = new IconTitle(heap);
            titlebar.Color(TITLE_COLOR);

            RenderedTextBlock txtInfo = PixelScene.RenderTextBlock(heap.Info(), 6);

            LayoutFields(titlebar, txtInfo);
        }

        private void FillFields(Item item)
        {
            var color = TITLE_COLOR;
            if (item.levelKnown && item.GetLevel() > 0)
                color = ItemSlot.UPGRADED;
            else if (item.levelKnown && item.GetLevel() < 0)
                color = ItemSlot.DEGRADED;

            IconTitle titlebar = new IconTitle(item);
            titlebar.Color(color);

            RenderedTextBlock txtInfo = PixelScene.RenderTextBlock(item.Info(), 6);

            LayoutFields(titlebar, txtInfo);
        }

        private void LayoutFields(IconTitle title, RenderedTextBlock info)
        {
            int width = WIDTH_MIN;

            info.MaxWidth(width);

            //window can go out of the screen on landscape, so widen it as appropriate
            while (PixelScene.Landscape() && 
                info.Height() > 100 && 
                width < WIDTH_MAX)
            {
                width += 20;
                info.MaxWidth(width);
            }

            title.SetRect(0, 0, width, 0);
            Add(title);

            info.SetPos(title.Left(), title.Bottom() + GAP);
            Add(info);

            Resize(width, (int)(info.Bottom() + 2));
        }
    }
}