using System;
using watabou.noosa;
using watabou.noosa.ui;
using watabou.noosa.audio;
using spdd.scenes;

namespace spdd.ui
{
    public class IconButton : Button
    {
        public Image icon;

        public IconButton()
        { }

        public IconButton(Image icon)
        {
            Icon(icon);
        }

        protected override void Layout()
        {
            base.Layout();

            if (icon != null)
            {
                icon.x = x + (width - icon.Width()) / 2f;
                icon.y = y + (height - icon.Height()) / 2f;
                PixelScene.Align(icon);
            }
        }

        protected override void OnPointerDown()
        {
            if (icon != null)
                icon.Brightness(1.5f);

            Sample.Instance.Play(Assets.Sounds.CLICK);
        }

        protected override void OnPointerUp()
        {
            if (icon != null)
                icon.ResetColor();
        }

        public void Enable(bool value)
        {
            active = value;
            if (icon != null)
                icon.Alpha(value ? 1.0f : 0.3f);
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
    }

    public class ActionIconButton : IconButton
    {
        public Action action;

        public ActionIconButton()
        { }

        public ActionIconButton(Image icon)
            : base(icon)
        { }

        protected override void OnClick()
        {
            action();
        }
    }
}