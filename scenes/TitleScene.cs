using System;
using watabou.glwrap;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.effects;
using spdd.messages;
using spdd.services.news;
using spdd.services.updates;
using spdd.sprites;
using spdd.ui;
using spdd.windows;

namespace spdd.scenes
{
    public class TitleScene : PixelScene
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

            var title = BannerSprites.Get(BannerSprites.Type.PIXEL_DUNGEON);
            Add(title);

            float topRegion = Math.Max(title.height - 6, h * 0.45f);

            title.x = (w - title.Width()) / 2f;
            title.y = 2 + (topRegion - title.Height()) / 2f;

            Align(title);

            PlaceTorch(title.x + 22, title.y + 46);
            PlaceTorch(title.x + title.width - 22, title.y + 46);

            var signs = new SignsImage(BannerSprites.Get(BannerSprites.Type.PIXEL_DUNGEON_SIGNS));
            signs.x = title.x + (title.Width() - signs.Width()) / 2f;
            signs.y = title.y;
            Add(signs);

            Chrome.Type GREY_TR = Chrome.Type.GREY_BUTTON_TR;

            var btnPlay = new BtnPlay(GREY_TR, Messages.Get(this, "enter"));
            btnPlay.Icon(Icons.ENTER.Get());
            Add(btnPlay);

            StyledButton btnSupport = new SupportButton(GREY_TR, Messages.Get(this, "support"));
            Add(btnSupport);

            var btnRankings = new BtnRankings(GREY_TR, Messages.Get(this, "rankings"));
            btnRankings.Icon(Icons.RANKINGS.Get());
            Add(btnRankings);

            var btnBadges = new BtnBadges(GREY_TR, Messages.Get(this, "badges"));
            btnBadges.Icon(Icons.BADGES.Get());
            Add(btnBadges);

            StyledButton btnNews = new NewsButton(GREY_TR, Messages.Get(this, "news"));
            btnNews.Icon(Icons.NEWS.Get());
            Add(btnNews);

            StyledButton btnChanges = new ChangesButton(GREY_TR, Messages.Get(this, "changes"));
            btnChanges.Icon(Icons.CHANGES.Get());
            Add(btnChanges);

            StyledButton btnSettings = new SettingsButton(GREY_TR, Messages.Get(this, "settings"));
            Add(btnSettings);

            var btnAbout = new BtnAbout(GREY_TR, Messages.Get(this, "about"));
            btnAbout.Icon(Icons.SHPX.Get());
            Add(btnAbout);

            const int BTN_HEIGHT = 20;
            int GAP = (int)(h - topRegion - (Landscape() ? 3 : 4) * BTN_HEIGHT) / 3;
            GAP /= Landscape() ? 3 : 5;
            GAP = Math.Max(GAP, 2);

            if (Landscape())
            {
                btnPlay.SetRect(title.x - 50, topRegion + GAP, ((title.Width() + 100) / 2) - 1, BTN_HEIGHT);
                Align(btnPlay);
                btnSupport.SetRect(btnPlay.Right() + 2, btnPlay.Top(), btnPlay.Width(), BTN_HEIGHT);
                btnRankings.SetRect(btnPlay.Left(), btnPlay.Bottom() + GAP, (btnPlay.Width() * .67f) - 1, BTN_HEIGHT);
                btnBadges.SetRect(btnRankings.Left(), btnRankings.Bottom() + GAP, btnRankings.Width(), BTN_HEIGHT);
                btnNews.SetRect(btnRankings.Right() + 2, btnRankings.Top(), btnRankings.Width(), BTN_HEIGHT);
                btnChanges.SetRect(btnNews.Left(), btnNews.Bottom() + GAP, btnRankings.Width(), BTN_HEIGHT);
                btnSettings.SetRect(btnNews.Right() + 2, btnNews.Top(), btnRankings.Width(), BTN_HEIGHT);
                btnAbout.SetRect(btnSettings.Left(), btnSettings.Bottom() + GAP, btnRankings.Width(), BTN_HEIGHT);
            }
            else
            {
                btnPlay.SetRect(title.x, topRegion + GAP, title.Width(), BTN_HEIGHT);
                Align(btnPlay);
                btnSupport.SetRect(btnPlay.Left(), btnPlay.Bottom() + GAP, btnPlay.Width(), BTN_HEIGHT);
                btnRankings.SetRect(btnPlay.Left(), btnSupport.Bottom() + GAP, (btnPlay.Width() / 2) - 1, BTN_HEIGHT);
                btnBadges.SetRect(btnRankings.Right() + 2, btnRankings.Top(), btnRankings.Width(), BTN_HEIGHT);
                btnNews.SetRect(btnRankings.Left(), btnRankings.Bottom() + GAP, btnRankings.Width(), BTN_HEIGHT);
                btnChanges.SetRect(btnNews.Right() + 2, btnNews.Top(), btnNews.Width(), BTN_HEIGHT);
                btnSettings.SetRect(btnNews.Left(), btnNews.Bottom() + GAP, btnRankings.Width(), BTN_HEIGHT);
                btnAbout.SetRect(btnSettings.Right() + 2, btnSettings.Top(), btnSettings.Width(), BTN_HEIGHT);
            }

            BitmapText version = new BitmapText("v" + Game.version, pixelFont);
            version.Measure();
            version.Hardlight(new Color(0x88, 0x88, 0x88, 0xFF));
            version.x = w - version.Width() - 4;
            version.y = h - version.Height() - 2;
            Add(version);

            FadeIn();
        }

        private class SignsImage : Image
        {
            private float time;

            public SignsImage(Image src)
                : base(src)
            { }

            public override void Update()
            {
                base.Update();
                am = Math.Max(0f, (float)Math.Sin(time += Game.elapsed));
                if (time >= 1.5f * Math.PI)
                    time = 0;
            }

            public override void Draw()
            {
                Blending.SetLightMode();
                base.Draw();
                Blending.SetNormalMode();
            }
        }  // SignsImage

        private class BtnPlay : StyledButton
        {
            public BtnPlay(Chrome.Type type, string label)
                : base(type, label)
            { }

            protected override void OnClick()
            {
                if (GamesInProgress.CheckAll().Count == 0)
                {
                    GamesInProgress.selectedClass = null;
                    GamesInProgress.curSlot = 1;
                    ShatteredPixelDungeonDash.SwitchScene(typeof(HeroSelectScene));
                }
                else
                {
                    ShatteredPixelDungeonDash.SwitchNoFade(typeof(StartScene));
                }
            }

            protected override bool OnLongClick()
            {
                //making it easier to start runs quickly while debugging
                if (DeviceCompat.IsDebug())
                {
                    GamesInProgress.selectedClass = null;
                    GamesInProgress.curSlot = 1;
                    ShatteredPixelDungeonDash.SwitchScene(typeof(HeroSelectScene));
                    return true;
                }
                return base.OnLongClick();
            }
        } // BtnPlay

        private class BtnRankings : StyledButton
        {
            public BtnRankings(Chrome.Type type, string label)
                : base(type, label)
            { }

            protected override void OnClick()
            {
                ShatteredPixelDungeonDash.SwitchNoFade(typeof(RankingsScene));
            }
        }  // BtnRankings

        private class BtnBadges : StyledButton
        {
            public BtnBadges(Chrome.Type type, string label)
                : base(type, label)
            { }

            protected override void OnClick()
            {
                ShatteredPixelDungeonDash.SwitchNoFade(typeof(BadgesScene));
            }
        }  // BtnBadges

        private class BtnAbout : StyledButton
        {
            public BtnAbout(Chrome.Type type, string label)
                : base(type, label)
            { }

            protected override void OnClick()
            {
                ShatteredPixelDungeonDash.SwitchNoFade(typeof(AboutScene));
            }
        }  // BtnAbout

        private void PlaceTorch(float x, float y)
        {
            var fb = new Fireball();
            fb.SetPos(x, y);
            Add(fb);
        }

        private class NewsButton : StyledButton
        {
            public NewsButton(Chrome.Type type, string label)
                : base(type, label)
            {
                if (SPDSettings.News())
                    News.CheckForNews();
            }

            int unreadCount = -1;

            public override void Update()
            {
                base.Update();

                if (unreadCount == -1 && News.ArticlesAvailable())
                {
                    long lastRead = SPDSettings.NewsLastRead();
                    if (lastRead == 0)
                    {
                        if (News.Articles().Count > 0)
                        {
                            SPDSettings.NewsLastRead(News.Articles()[0].date.Ticks);
                        }
                    }
                    else
                    {
                        unreadCount = News.UnreadArticles(new DateTime(SPDSettings.NewsLastRead()));
                        if (unreadCount > 0)
                        {
                            unreadCount = Math.Min(unreadCount, 9);
                            Text(Text() + "(" + unreadCount + ")");
                        }
                    }
                }

                if (unreadCount > 0)
                {
                    var c1 = new Color(0xFF, 0xFF, 0xFF, 0xFF);
                    TextColor(ColorMath.Interpolate(c1, Window.SHPX_COLOR, 0.5f + (float)Math.Sin(Game.timeTotal * 5) / 2f));
                }
            }

            protected override void OnClick()
            {
                base.OnClick();
                ShatteredPixelDungeonDash.SwitchNoFade(typeof(NewsScene));
            }
        } // NewsButton

        private class ChangesButton : StyledButton
        {
            public ChangesButton(Chrome.Type type, string label)
                : base(type, label)
            {
                if (SPDSettings.Updates())
                    Updates.CheckForUpdate();
            }

            bool updateShown;

            public override void Update()
            {
                base.Update();

                if (!updateShown && (Updates.UpdateAvailable() || Updates.IsInstallable()))
                {
                    updateShown = true;
                    if (Updates.IsInstallable())
                        Text(Messages.Get(typeof(TitleScene), "install"));
                    else
                        Text(Messages.Get(typeof(TitleScene), "update"));
                }

                if (updateShown)
                {
                    var c1 = new Color(0xFF, 0xFF, 0xFF, 0xFF);
                    TextColor(ColorMath.Interpolate(c1, Window.SHPX_COLOR, 0.5f + (float)Math.Sin(Game.timeTotal * 5) / 2f));
                }
            }

            protected override void OnClick()
            {
                if (Updates.IsInstallable())
                {
                    Updates.LaunchInstall();

                }
                else if (Updates.UpdateAvailable())
                {
                    AvailableUpdateData update = Updates.UpdateData();

                    var wnd = new WndOptions(
                            update.versionName == null ? Messages.Get(this, "title") : Messages.Get(this, "versioned_title", update.versionName),
                            update.desc == null ? Messages.Get(this, "desc") : update.desc,
                            Messages.Get(this, "update"),
                            Messages.Get(this, "changes")
                    );
                    wnd.selectAction = (index) =>
                    {
                        if (index == 0)
                        {
                            Updates.LaunchUpdate(Updates.UpdateData());
                        }
                        else if (index == 1)
                        {
                            //ChangesScene.changesSelected = 0;
                            //ShatteredPixelDungeon.SwitchNoFade(typeof(ChangesScene));
                        }
                    };

                    ShatteredPixelDungeonDash.Scene().AddToFront(wnd);
                }
                else
                {
                    //ChangesScene.changesSelected = 0;
                    //ShatteredPixelDungeon.SwitchNoFade(typeof(ChangesScene));
                }
            }
        } // ChangesButton

        private class SettingsButton : StyledButton
        {
            public SettingsButton(Chrome.Type type, string label)
                : base(type, label)
            {
                if (Messages.Lang().GetStatus() == Languages.Status.INCOMPLETE)
                {
                    Icon(Icons.LANGS.Get());
                    icon.Hardlight(1.5f, 0, 0);
                }
                else
                {
                    Icon(Icons.PREFS.Get());
                }
            }

            public override void Update()
            {
                base.Update();

                if (Messages.Lang().GetStatus() == Languages.Status.INCOMPLETE)
                {
                    var c1 = new Color(0xFF, 0xFF, 0xFF, 0xFF);
                    TextColor(ColorMath.Interpolate(c1, CharSprite.NEGATIVE, 0.5f + (float)Math.Sin(Game.timeTotal * 5) / 2f));
                }
            }

            protected override void OnClick()
            {
                if (Messages.Lang().GetStatus() == Languages.Status.INCOMPLETE)
                {
                    WndSettings.last_index = 4;
                }
                ShatteredPixelDungeonDash.Scene().Add(new WndSettings());
            }
        } // SettingsButton

        private class SupportButton : StyledButton
        {
            public SupportButton(Chrome.Type type, string label)
                    : base(type, label)
            {
                Icon(Icons.GOLD.Get());
                TextColor(Window.TITLE_COLOR);
            }

            protected override void OnClick()
            {
                ShatteredPixelDungeonDash.SwitchNoFade(typeof(SupporterScene));
            }
        } // SupportButton
    }
}