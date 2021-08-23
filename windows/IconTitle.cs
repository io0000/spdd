using System;
using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;
using spdd.items;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;

namespace spdd.windows
{
    public class IconTitle : Component
    {
        private const float FONT_SIZE = 9;

        private const float GAP = 2;

        public Image imIcon;
        public RenderedTextBlock tfLabel;
        protected HealthBar health;

        private float healthLvl = float.NaN;

        public IconTitle()
        { }

        public IconTitle(Item item)
        {
            ItemSprite icon = new ItemSprite();
            Icon(icon);
            Label(Messages.TitleCase(item.ToString()));
            icon.View(item);
        }

        public IconTitle(Heap heap)
        {
            ItemSprite icon = new ItemSprite();
            Icon(icon);
            Label(Messages.TitleCase(heap.ToString()));
            icon.View(heap);
        }

        public IconTitle(Image icon, string label)
        {
            Icon(icon);
            Label(label);
        }

        protected override void CreateChildren()
        {
            imIcon = new Image();
            Add(imIcon);

            tfLabel = PixelScene.RenderTextBlock((int)FONT_SIZE);
            tfLabel.Hardlight(Window.TITLE_COLOR);
            tfLabel.SetHightlighting(false);
            Add(tfLabel);

            health = new HealthBar();
            Add(health);
        }

        protected override void Layout()
        {
            health.visible = !float.IsNaN(healthLvl);

            imIcon.x = x + (Math.Max(0, 8 - imIcon.Width() / 2));
            imIcon.y = y + (Math.Max(0, 8 - imIcon.Height() / 2));
            PixelScene.Align(imIcon);

            int imWidth = (int)Math.Max(imIcon.Width(), 16);
            int imHeight = (int)Math.Max(imIcon.Height(), 16);

            tfLabel.MaxWidth((int)(width - (imWidth + GAP)));
            tfLabel.SetPos(x + imWidth + GAP,
                            imHeight > tfLabel.Height() ? y + (imHeight - tfLabel.Height()) / 2 : y);
            PixelScene.Align(tfLabel);

            if (health.visible)
            {
                health.SetRect(tfLabel.Left(), tfLabel.Bottom(), tfLabel.MaxWidth(), 0);
                height = Math.Max(imHeight, health.Bottom());
            }
            else
            {
                height = Math.Max(imHeight, tfLabel.Height());
            }
        }

        public void Icon(Image icon)
        {
            if (icon != null)
            {
                Remove(imIcon);
                Add(imIcon = icon);
            }
        }

        public void Label(string label)
        {
            tfLabel.Text(label);
        }

        public void Label(string label, Color color)
        {
            tfLabel.Text(label);
            tfLabel.Hardlight(color);
        }

        public void Color(Color color)
        {
            tfLabel.Hardlight(color);
        }

        public void Health(float value)
        {
            health.Level(healthLvl = value);
            Layout();
        }
    }
}