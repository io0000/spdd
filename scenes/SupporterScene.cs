using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;
using spdd.messages;
using spdd.ui;

namespace spdd.scenes
{
    public class SupporterScene : PixelScene
    {
        private const int BTN_HEIGHT = 22;
        private const int GAP = 2;

        public override void Create()
        {
            base.Create();

            uiCamera.visible = false;

            int w = Camera.main.width;
            int h = Camera.main.height;

            int elementWidth = PixelScene.Landscape() ? 202 : 120;

            Archs archs = new Archs();
            archs.SetSize(w, h);
            Add(archs);

            ExitButton btnExit = new ExitButton();
            btnExit.SetPos(w - btnExit.Width(), 0);
            Add(btnExit);

            RenderedTextBlock title = PixelScene.RenderTextBlock(Messages.Get(this, "title"), 9);
            title.Hardlight(Window.TITLE_COLOR);
            title.SetPos(
                    (w - title.Width()) / 2f,
                    (20 - title.Height()) / 2f
            );
            Align(title);
            Add(title);

            SupporterMessage msg = new SupporterMessage();
            msg.SetSize(elementWidth, 0);
            Add(msg);

            var link = new LinkButton(Chrome.Type.GREY_BUTTON_TR, Messages.Get(this, "supporter_link"));
            link.Icon(Icons.GOLD.Get());
            link.TextColor(Window.TITLE_COLOR);
            link.SetSize(elementWidth, BTN_HEIGHT);
            Add(link);

            float elementHeight = msg.Height() + BTN_HEIGHT + GAP;

            float top = 16 + (h - 16 - elementHeight) / 2f;
            float left = (w - elementWidth) / 2f;

            msg.SetPos(left, top);
            Align(msg);

            link.SetPos(left, msg.Bottom() + GAP);
            Align(link);
        }

        private class LinkButton : StyledButton
        {
            public LinkButton(Chrome.Type type, string label)
                : base(type, label, 9)
            { }

            protected override void OnClick()
            {
                base.OnClick();
                string link = "https://www.patreon.com/ShatteredPixel";
                //tracking codes, so that the website knows where this pageview came from
                link += "?utm_source=shatteredpd";
                link += "&utm_medium=supporter_page";
                link += "&utm_campaign=ingame_link";
                DeviceCompat.OpenURI(link);
            }
        }

        public override void OnBackPressed()
        {
            ShatteredPixelDungeonDash.SwitchNoFade(typeof(TitleScene));
        }

        private class SupporterMessage : Component
        {
            NinePatch bg;
            RenderedTextBlock text;
            Image icon;

            protected override void CreateChildren()
            {
                bg = Chrome.Get(Chrome.Type.GREY_BUTTON_TR);
                Add(bg);

                string message = Messages.Get(typeof(SupporterScene), "intro");
                message += "\n\n" + Messages.Get(typeof(SupporterScene), "patreon_msg");
                if (Messages.Lang() != Languages.ENGLISH)
                {
                    message += "\n" + Messages.Get(typeof(SupporterScene), "patreon_english");
                }
                message += "\n\n- Evan";

                text = PixelScene.RenderTextBlock(message, 6);
                Add(text);

                icon = Icons.SHPX.Get();
                Add(icon);
            }

            protected override void Layout()
            {
                bg.x = x;
                bg.y = y;

                text.MaxWidth((int)width - bg.MarginHor());
                text.SetPos(x + bg.MarginLeft(), y + bg.MarginTop());

                icon.y = text.Bottom() - icon.Height() + 4;
                icon.x = x + 25;

                height = (text.Bottom() + 2) - y;

                height += bg.MarginBottom();

                bg.Size(width, height);
            }
        }
    }
}
