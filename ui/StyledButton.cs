using System;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.ui;
using watabou.utils;
using spdd.scenes;

namespace spdd.ui
{
    //simple button which support a background chrome, text, and an icon.
    public class StyledButton : Button
    {
        protected NinePatch bg;
        protected RenderedTextBlock text;
        protected Image icon;

        public StyledButton(Chrome.Type type, string label)
            : this(type, label, 9)
        { }

        public StyledButton(Chrome.Type type, string label, int size)
        {
            bg = Chrome.Get(type);
            AddToBack(bg);

            text = PixelScene.RenderTextBlock(size);
            text.Text(label);
            Add(text);
        }

        protected override void Layout()
        {
            base.Layout();

            bg.x = x;
            bg.y = y;
            bg.Size(width, height);

            float componentWidth = 0;

            if (icon != null)
                componentWidth += icon.Width() + 2;

            if (text != null && !text.Text().Equals(""))
            {
                componentWidth += text.Width() + 2;

                text.SetPos(
                        x + (Width() + componentWidth) / 2f - text.Width() - 1,
                        y + (Height() - text.Height()) / 2f
                );
                PixelScene.Align(text);
            }

            if (icon != null)
            {
                icon.x = x + (Width() - componentWidth) / 2f + 1;
                icon.y = y + (Height() - icon.Height()) / 2f;
                PixelScene.Align(icon);
            }
        }

        protected override void OnPointerDown()
        {
            bg.Brightness(1.2f);
            Sample.Instance.Play(Assets.Sounds.CLICK);
        }

        protected override void OnPointerUp()
        {
            bg.ResetColor();
        }

        public virtual void Enable(bool value)
        {
            active = value;
            text.Alpha(value ? 1.0f : 0.3f);
        }

        public void Text(string value)
        {
            text.Text(value);
            Layout();
        }

        public string Text()
        {
            return text.Text();
        }

        public void TextColor(Color value)
        {
            text.Hardlight(value);
        }

        public void Icon(Image icon)
        {
            if (this.icon != null)
            {
                Remove(this.icon);
            }
            this.icon = icon;
            if (this.icon != null)
            {
                Add(this.icon);
                Layout();
            }
        }

        public Image Icon()
        {
            return icon;
        }

        public void Alpha(float value)
        {
            if (icon != null)
                icon.Alpha(value);
            if (bg != null)
                bg.Alpha(value);
            if (text != null)
                text.Alpha(value);
        }

        public float ReqWidth()
        {
            float reqWidth = 0;
            if (icon != null)
            {
                reqWidth += icon.Width() + 2;
            }
            if (text != null && !text.Text().Equals(""))
            {
                reqWidth += text.Width() + 2;
            }
            return reqWidth;
        }

        public float ReqHeight()
        {
            float reqHeight = 0;
            if (icon != null)
            {
                reqHeight = Math.Max(icon.Height() + 4, reqHeight);
            }
            if (text != null && !text.Text().Equals(""))
            {
                reqHeight = Math.Max(text.Height() + 4, reqHeight);
            }
            return reqHeight;
        }
    }
}