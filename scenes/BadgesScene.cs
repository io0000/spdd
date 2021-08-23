using System;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.ui;
using watabou.utils;
using spdd.effects;
using spdd.messages;
using spdd.ui;
using spdd.windows;

namespace spdd.scenes
{
    public class BadgesScene : PixelScene
    {
        public override void Create()
        {
            base.Create();

            Music.Instance.Play(Assets.Music.THEME, true);

            uiCamera.visible = false;

            var w = Camera.main.width;
            var h = Camera.main.height;

            var archs = new Archs();
            archs.SetSize(w, h);
            Add(archs);

            float left = 5;
            float top = 20;

            RenderedTextBlock title = PixelScene.RenderTextBlock(Messages.Get(this, "title"), 9);
            title.Hardlight(Window.TITLE_COLOR);
            title.SetPos((w - title.Width()) / 2f,(top - title.Height()) / 2f);
            Align(title);
            Add(title);

            BadgesExtensions.LoadGlobal();

            var badges = BadgesExtensions.Filtered(true);

            int blankBadges = 36;
            blankBadges -= badges.Count;
            if (badges.Contains(Badges.Badge.ALL_ITEMS_IDENTIFIED))
                blankBadges -= 6;
            if (badges.Contains(Badges.Badge.YASD))
                blankBadges -= 5;
            blankBadges = Math.Max(0, blankBadges);

            //guarantees a max of 5 rows in landscape, and 8 in portrait, assuming a max of 40 buttons
            int nCols = Landscape() ? 7 : 4;
            if (badges.Count + blankBadges > 32 && !Landscape())
                ++nCols;

            int nRows = 1 + (blankBadges + badges.Count) / nCols;

            float badgeWidth = (w - 2 * left) / nCols;
            float badgeHeight = (h - 2 * top) / nRows;

            for (int i = 0; i < badges.Count + blankBadges; ++i)
            {
                int row = i / nCols;
                int col = i % nCols;
                //Badges.Badge? b = i < badges.Count ? badges[i] : null;
                Badges.Badge? b = null;
                if (i < badges.Count)
                    b = badges[i];

                BadgeButton button = new BadgeButton(b);
                button.SetPos(
                        left + col * badgeWidth + (badgeWidth - button.Width()) / 2,
                        top + row * badgeHeight + (badgeHeight - button.Height()) / 2);
                Align(button);
                Add(button);
            }

            ExitButton btnExit = new ExitButton();
            btnExit.SetPos(Camera.main.width - btnExit.Width(), 0);
            Add(btnExit);

            FadeIn();
        }

        public override void Destroy()
        {
            BadgesExtensions.SaveGlobal();

            base.Destroy();
        }

        public override void OnBackPressed()
        {
            ShatteredPixelDungeonDash.SwitchNoFade(typeof(TitleScene));
        }

        private class BadgeButton : Button
        {
            private Badges.Badge? badge;

            private Image icon;

            public BadgeButton(Badges.Badge? badge)
            {
                this.badge = badge;
                active = (badge != null);

                icon = active ? BadgeBanner.Image(badge.Value.GetImage()) : new Image(Assets.Interfaces.LOCKED);
                Add(icon);

                SetSize(icon.Width(), icon.Height());
            }

            protected override void Layout()
            {
                base.Layout();

                icon.x = x + (width - icon.Width()) / 2;
                icon.y = y + (height - icon.Height()) / 2;
            }

            public override void Update()
            {
                base.Update();

                if (Rnd.Float() < Game.elapsed * 0.1)
                {
                    BadgeBanner.Highlight(icon, badge.Value.GetImage());
                }
            }

            protected override void OnClick()
            {
                Sample.Instance.Play(Assets.Sounds.CLICK, 0.7f, 0.7f, 1.2f);
                Game.Scene().Add(new WndBadge(badge.Value));
            }
        }
    }
}