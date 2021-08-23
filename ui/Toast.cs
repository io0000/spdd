using System;
using watabou.noosa;
using watabou.noosa.ui;
using spdd.scenes;

namespace spdd.ui
{
    // 타겟 위치 선택시 나오는 윈도우 
    public class Toast : Component
    {
        private const float MARGIN_HOR = 2.0f;
        private const float MARGIN_VER = 2.0f;

        protected NinePatch bg;
        protected SimpleButtonToast close;
        protected RenderedTextBlock text;

        public Toast(string text)
        {
            Text(text);

            width = this.text.Width() + close.Width() + bg.MarginHor() + MARGIN_HOR * 3;
            height = Math.Max(this.text.Height(), close.Height()) + bg.MarginVer() + MARGIN_VER * 2;
        }

        protected override void CreateChildren()
        {
            base.CreateChildren();

            bg = Chrome.Get(Chrome.Type.TOAST_TR);
            Add(bg);

            close = new SimpleButtonToast(this, Icons.CLOSE.Get());
            Add(close);

            text = PixelScene.RenderTextBlock(8);
            Add(text);
        }

        protected override void Layout()
        {
            base.Layout();

            bg.x = x;
            bg.y = y;
            bg.Size(width, height);

            close.SetPos(
                bg.x + bg.Width() - bg.MarginHor() / 2f - MARGIN_HOR - close.Width(),
                y + (height - close.Height()) / 2);
            PixelScene.Align(close);

            text.SetPos(close.Left() - MARGIN_HOR - text.Width(), y + (height - text.Height()) / 2);
            PixelScene.Align(text);
        }

        private void Text(string txt)
        {
            text.Text(txt);
        }

        public Action closeAction;

        protected void OnClose()
        {
            if (closeAction != null)
                closeAction();
        }

        public class SimpleButtonToast : SimpleButton
        {
            private Toast toast;
            public SimpleButtonToast(Toast toast, Image image)
                : base(image)
            {
                this.toast = toast;
            }

            protected override void OnClick()
            {
                toast.OnClose();
            }
        }
    }
}