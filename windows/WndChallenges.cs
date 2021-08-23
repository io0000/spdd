using System.Collections.Generic;
using watabou.noosa;
using spdd.messages;
using spdd.scenes;
using spdd.ui;

namespace spdd.windows
{
    public class WndChallenges : Window
    {
        private const int WIDTH = 120;
        private const int TTL_HEIGHT = 18;
        private const int BTN_HEIGHT = 18;
        private const int GAP = 1;

        private bool editable;
        private List<CheckBox> boxes;

        // checked : keyword
        public WndChallenges(int checkedValue, bool editable)
        {
            this.editable = editable;

            RenderedTextBlock title = PixelScene.RenderTextBlock(Messages.Get(this, "title"), 12);
            title.Hardlight(TITLE_COLOR);
            title.SetPos(
                    (WIDTH - title.Width()) / 2,
                    (TTL_HEIGHT - title.Height()) / 2
            );
            PixelScene.Align(title);
            Add(title);

            boxes = new List<CheckBox>();

            float pos = TTL_HEIGHT;
            for (int i = 0; i < Challenges.NAME_IDS.Length; ++i)
            {
                string challenge = Challenges.NAME_IDS[i];

                CheckBox cb = new CheckBox(Messages.Get(typeof(Challenges), challenge));
                cb.Checked((checkedValue & Challenges.MASKS[i]) != 0);
                cb.active = editable;

                if (i > 0)
                {
                    pos += GAP;
                }
                cb.SetRect(0, pos, WIDTH - 16, BTN_HEIGHT);

                Add(cb);
                boxes.Add(cb);

                var info = new WndChallengesIconButton(Icons.INFO.Get());
                info.challenge = challenge;
                info.SetRect(cb.Right(), pos, 16, BTN_HEIGHT);
                Add(info);

                pos = cb.Bottom();
            }

            Resize(WIDTH, (int)pos);
        }

        private class WndChallengesIconButton : IconButton
        {
            internal string challenge;

            public WndChallengesIconButton(Image icon)
                : base(icon)
            { }

            protected override void OnClick()
            {
                base.OnClick();
                var wnd = new WndMessage(Messages.Get(typeof(Challenges), challenge + "_desc"));
                ShatteredPixelDungeonDash.Scene().Add(wnd);
            }
        }

        public override void OnBackPressed()
        {
            if (editable)
            {
                int value = 0;
                for (int i = 0; i < boxes.Count; ++i)
                {
                    if (boxes[i].Checked())
                        value |= Challenges.MASKS[i];
                }

                SPDSettings.Challenges(value);
            }

            base.OnBackPressed();
        }
    }
}