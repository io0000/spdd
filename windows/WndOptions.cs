using System;
using spdd.scenes;
using spdd.ui;

namespace spdd.windows
{
    public class WndOptions : Window
    {
        private const int WIDTH_P = 120;
        private const int WIDTH_L = 144;

        private const int MARGIN = 2;
        private const int BUTTON_HEIGHT = 20;

        public Action<int> selectAction;
        public bool skipBackPressed;

        public WndOptions(string title, string message, params string[] options)
        {
            int width = PixelScene.Landscape() ? WIDTH_L : WIDTH_P;

            float pos = MARGIN;
            if (!string.IsNullOrEmpty(title))
            {
                RenderedTextBlock tfTitle = PixelScene.RenderTextBlock(title, 9);
                tfTitle.Hardlight(TITLE_COLOR);
                tfTitle.SetPos(MARGIN, pos);
                tfTitle.MaxWidth(width - MARGIN * 2);
                Add(tfTitle);

                pos = tfTitle.Bottom() + 3 * MARGIN;
            }

            RenderedTextBlock tfMesage = PixelScene.RenderTextBlock(6);
            tfMesage.Text(message, width - MARGIN * 2);
            tfMesage.SetPos(MARGIN, pos);
            Add(tfMesage);

            pos = tfMesage.Bottom() + 2 * MARGIN;

            for (int i = 0; i < options.Length; ++i)
            {
                int index = i;

                var btn = new ActionRedButton(options[i]);
                btn.action = () =>
                {
                    Hide();
                    OnSelect(index);
                };

                btn.Enable(Enabled(i));
                btn.SetRect(MARGIN, pos, width - MARGIN * 2, BUTTON_HEIGHT);
                Add(btn);

                pos += BUTTON_HEIGHT + MARGIN;
            }

            Resize(width, (int)pos);
        }

        protected virtual bool Enabled(int index)
        {
            return true;
        }

        protected virtual void OnSelect(int index)
        {
            if (selectAction != null)
                selectAction(index);
        }

        public override void OnBackPressed()
        {
            if (skipBackPressed)
                return;

            base.OnBackPressed();
        }
    }
}