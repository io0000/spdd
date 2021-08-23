using System;
using System.Collections.Generic;
using watabou.gltextures;
using watabou.input;
using watabou.noosa;
using watabou.utils;
using spdd.actors.hero;
using spdd.journal;
using spdd.messages;
using spdd.sprites;
using spdd.ui;
using spdd.windows;

namespace spdd.scenes
{
    public class HeroSelectScene : PixelScene
    {
        private Image background;
        private RenderedTextBlock prompt;

        //fading UI elements
        private List<StyledButton> heroBtns = new List<StyledButton>();
        private StyledButton startBtn;
        private IconButton infoButton;
        private IconButton challengeButton;
        private ExitButton btnExit;

        public override void Create()
        {
            base.Create();

            BadgesExtensions.LoadGlobal();
            Journal.LoadGlobal();

            background = new HeroSelectSceneImage(HeroClass.WARRIOR.SplashArt());
            background.scale.Set(Camera.main.height / background.height);

            background.x = (Camera.main.width - background.Width()) / 2f;
            background.y = (Camera.main.height - background.Height()) / 2f;
            background.visible = false;
            PixelScene.Align(background);
            Add(background);

            if (background.x > 0)
            {
                Color[] colors = new Color[] {
                    new Color(0x00, 0x00, 0x00, 0xFF),
                    new Color(0x00, 0x00, 0x00, 0x00)
                };
                Image fadeLeft = new Image(TextureCache.CreateGradient(colors));
                fadeLeft.x = background.x - 2;
                fadeLeft.scale.Set(4, background.Height());
                Add(fadeLeft);

                Image fadeRight = new Image(fadeLeft);
                fadeRight.x = background.x + background.Width() + 2;
                fadeRight.y = background.y + background.Height();
                fadeRight.angle = 180;
                Add(fadeRight);
            }

            prompt = PixelScene.RenderTextBlock(Messages.Get(typeof(WndStartGame), "title"), 12);
            prompt.Hardlight(Window.TITLE_COLOR);
            prompt.SetPos((Camera.main.width - prompt.Width()) / 2f, (Camera.main.height - HeroBtn.HEIGHT - prompt.Height() - 4));
            PixelScene.Align(prompt);
            Add(prompt);

            startBtn = new StartButton(Chrome.Type.GREY_BUTTON_TR, "");
            startBtn.Icon(Icons.ENTER.Get());
            startBtn.SetSize(80, 21);
            startBtn.SetPos((Camera.main.width - startBtn.Width()) / 2f, (Camera.main.height - HeroBtn.HEIGHT + 2 - startBtn.Height()));
            Add(startBtn);
            startBtn.visible = false;

            infoButton = new InfoButton(Icons.INFO.Get());
            infoButton.visible = false;
            infoButton.SetSize(21, 21);
            Add(infoButton);

            var classes = Enum.GetValues(typeof(HeroClass));

            int btnWidth = HeroBtn.MIN_WIDTH;
            int curX = (Camera.main.width - btnWidth * classes.Length) / 2;
            if (curX > 0)
            {
                btnWidth += Math.Min(curX / (classes.Length / 2), 15);
                curX = (Camera.main.width - btnWidth * classes.Length) / 2;
            }

            int heroBtnleft = curX;
            foreach (HeroClass cl in classes)
            {
                HeroBtn button = new HeroBtn(this, cl);
                button.SetRect(curX, Camera.main.height - HeroBtn.HEIGHT + 3, btnWidth, HeroBtn.HEIGHT);
                curX += btnWidth;
                Add(button);
                heroBtns.Add(button);
            }

            challengeButton = new ChallengeButton(
                    IconsExtensions.Get(SPDSettings.Challenges() > 0 ? Icons.CHALLENGE_ON : Icons.CHALLENGE_OFF));

            challengeButton.SetRect(heroBtnleft + 16, Camera.main.height - HeroBtn.HEIGHT - 16, 21, 21);
            challengeButton.visible = false;

            if (DeviceCompat.IsDebug() || BadgesExtensions.IsUnlocked(Badges.Badge.VICTORY))
            {
                Add(challengeButton);
            }
            else
            {
                Dungeon.challenges = 0;
                SPDSettings.Challenges(0);
            }

            btnExit = new ExitButton();
            btnExit.SetPos(Camera.main.width - btnExit.Width(), 0);
            Add(btnExit);
            btnExit.visible = !SPDSettings.Intro() || Rankings.Instance.wonNumber > 0;

            var fadeResetter = new FadeResetter(0, 0, Camera.main.width, Camera.main.height);
            fadeResetter.scene = this;

            Add(fadeResetter);
            ResetFade();

            if (GamesInProgress.selectedClass != null)
                SetSelectedHero(GamesInProgress.selectedClass.Value);

            FadeIn();
        }

        private class HeroSelectSceneImage : Image
        {
            public HeroSelectSceneImage(object tx)
                : base(tx)
            { }

            public override void Update()
            {
                if (rm > 1f)
                {
                    rm -= Game.elapsed;
                    gm = bm = rm;
                }
                else
                {
                    rm = gm = bm = 1;
                }
            }
        }

        private class StartButton : StyledButton
        {
            public StartButton(Chrome.Type type, string label)
                : base(type, label)
            { }

            protected override void OnClick()
            {
                base.OnClick();

                if (GamesInProgress.selectedClass == null)
                    return;

                Dungeon.hero = null;
                ActionIndicator.action = null;
                InterlevelScene.mode = InterlevelScene.Mode.DESCEND;

                if (SPDSettings.Intro())
                {
                    SPDSettings.Intro(false);
                    Game.SwitchScene(typeof(IntroScene));
                }
                else
                {
                    Game.SwitchScene(typeof(InterlevelScene));
                }
            }
        }

        private class InfoButton : IconButton
        {
            public InfoButton(Image icon)
                : base(icon)
            { }

            protected override void OnClick()
            {
                base.OnClick();
                ShatteredPixelDungeonDash.Scene().AddToFront(new WndHeroInfo(GamesInProgress.selectedClass.Value));
            }
        }

        private class FadeResetter : PointerArea
        {
            internal HeroSelectScene scene;
            public FadeResetter(float x, float y, float width, float height)
                : base(x, y, width, height)
            { }

            public override bool OnSignal(PointerEvent ev)
            {
                scene.ResetFade();
                return false;
            }
        }

        private class ChallengeButton : IconButton
        {
            public ChallengeButton(Image image)
                : base(image)
            { }

            protected override void OnClick()
            {
                var wnd = new WndChallenges2(SPDSettings.Challenges(), true);
                wnd.btn = this;

                ShatteredPixelDungeonDash.Scene().AddToFront(wnd);
            }

            public override void Update()
            {
                if (!visible && GamesInProgress.selectedClass != null)
                    visible = true;

                base.Update();
            }

            private class WndChallenges2 : WndChallenges
            {
                internal ChallengeButton btn;
                public WndChallenges2(int isChecked, bool editable)
                    : base(isChecked, editable)
                { }

                public override void OnBackPressed()
                {
                    base.OnBackPressed();
                    btn.Icon(IconsExtensions.Get(SPDSettings.Challenges() > 0 ? Icons.CHALLENGE_ON : Icons.CHALLENGE_OFF));
                }
            }
        }

        private void SetSelectedHero(HeroClass cl)
        {
            GamesInProgress.selectedClass = cl;

            background.Texture(cl.SplashArt());
            background.visible = true;
            background.Hardlight(1.5f, 1.5f, 1.5f);

            prompt.visible = false;
            startBtn.visible = true;
            startBtn.Text(Messages.TitleCase(cl.Title()));
            startBtn.TextColor(Window.TITLE_COLOR);
            startBtn.SetSize(startBtn.ReqWidth() + 8, 21);
            startBtn.SetPos((Camera.main.width - startBtn.Width()) / 2f, startBtn.Top());
            PixelScene.Align(startBtn);

            infoButton.visible = true;
            infoButton.SetPos(startBtn.Right(), startBtn.Top());

            challengeButton.visible = true;
            challengeButton.SetPos(startBtn.Left() - challengeButton.Width(), startBtn.Top());
        }

        private float uiAlpha;

        public override void Update()
        {
            base.Update();
            btnExit.visible = !SPDSettings.Intro() || Rankings.Instance.wonNumber > 0;
            //do not fade when a window is open
            foreach (var v in members)
            {
                if (v is Window)
                    ResetFade();
            }
            if (GamesInProgress.selectedClass != null)
            {
                if (uiAlpha > 0f)
                    uiAlpha -= Game.elapsed / 4f;

                float alpha = GameMath.Gate(0f, uiAlpha, 1f);
                foreach (StyledButton b in heroBtns)
                    b.Alpha(alpha);

                startBtn.Alpha(alpha);
                btnExit.Icon().Alpha(alpha);
                challengeButton.Icon().Alpha(alpha);
                infoButton.Icon().Alpha(alpha);
            }
        }

        private void ResetFade()
        {
            //starts fading after 4 seconds, fades over 4 seconds.
            uiAlpha = 2f;
        }

        public override void OnBackPressed()
        {
            if (!SPDSettings.Intro() && Rankings.Instance.wonNumber == 0)
            {
                ShatteredPixelDungeonDash.SwitchScene(typeof(TitleScene));
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public class HeroBtn : StyledButton
        {
            private HeroSelectScene scene;
            private HeroClass cl;

            internal const int MIN_WIDTH = 20;
            internal const int HEIGHT = 24;

            public HeroBtn(HeroSelectScene scene, HeroClass cl)
                : base(Chrome.Type.GREY_BUTTON_TR, "")
            {
                this.scene = scene;
                this.cl = cl;

                Icon(new Image(cl.Spritesheet(), 0, 90, 12, 15));
            }

            public override void Update()
            {
                base.Update();
                if (cl != GamesInProgress.selectedClass)
                {
                    if (!cl.IsUnlocked())
                    {
                        icon.Brightness(0.1f);
                    }
                    else
                    {
                        icon.Brightness(0.6f);
                    }
                }
                else
                {
                    icon.Brightness(1f);
                }
            }

            protected override void OnClick()
            {
                base.OnClick();

                if (!cl.IsUnlocked())
                {
                    ShatteredPixelDungeonDash.Scene().AddToFront(new WndMessage(cl.UnlockMsg()));
                }
                else if (GamesInProgress.selectedClass == cl)
                {
                    ShatteredPixelDungeonDash.Scene().Add(new WndHeroInfo(cl));
                }
                else
                {
                    scene.SetSelectedHero(cl);
                }
            }
        }

        private class WndHeroInfo : WndTabbed
        {
            private RenderedTextBlock info;

            private const int WIDTH = 120;
            private const int MARGIN = 4;
            private const int INFO_WIDTH = WIDTH - MARGIN * 2;

            public WndHeroInfo(HeroClass cl)
            {
                HeroSelectSceneIconTab tab;
                Image[] tabIcons;
                switch (cl)
                {
                    case HeroClass.WARRIOR:
                    default:
                        tabIcons = new Image[]{
                                new ItemSprite(ItemSpriteSheet.SEAL, null),
                                new ItemSprite(ItemSpriteSheet.WORN_SHORTSWORD, null),
                                new ItemSprite(ItemSpriteSheet.RATION, null)
                        };
                        break;
                    case HeroClass.MAGE:
                        tabIcons = new Image[]{
                                new ItemSprite(ItemSpriteSheet.MAGES_STAFF, null),
                                new ItemSprite(ItemSpriteSheet.HOLDER, null),
                                new ItemSprite(ItemSpriteSheet.WAND_MAGIC_MISSILE, null)
                        };
                        break;
                    case HeroClass.ROGUE:
                        tabIcons = new Image[]{
                                new ItemSprite(ItemSpriteSheet.ARTIFACT_CLOAK, null),
                                new ItemSprite(ItemSpriteSheet.DAGGER, null),
                                IconsExtensions.Get(Icons.DEPTH)
                        };
                        break;
                    case HeroClass.HUNTRESS:
                        tabIcons = new Image[]{
                                new ItemSprite(ItemSpriteSheet.SPIRIT_BOW, null),
                                new ItemSprite(ItemSpriteSheet.GLOVES, null),
                                new Image(Assets.Environment.TILES_SEWERS, 112, 96, 16, 16)
                        };
                        break;
                }

                tab = new HeroSelectSceneIconTab(this, tabIcons[0]);
                tab.action = (value) =>
                {
                    // base.select(value); ->  호출된 상태
                    if (value)
                    {
                        info.Text(Messages.Get(cl, cl.ToString() + "_desc_item"), INFO_WIDTH);
                    }
                };
                Add(tab);

                tab = new HeroSelectSceneIconTab(this, tabIcons[1]);
                tab.action = (value) =>
                {
                    // base.select(value); ->  호출된 상태
                    if (value)
                    {
                        info.Text(Messages.Get(cl, cl.ToString() + "_desc_loadout"), INFO_WIDTH);
                    }
                };
                Add(tab);

                tab = new HeroSelectSceneIconTab(this, tabIcons[2]);
                tab.action = (value) =>
                {
                    // base.select(value); ->  호출된 상태
                    if (value)
                    {
                        info.Text(Messages.Get(cl, cl.ToString() + "_desc_misc"), INFO_WIDTH);
                    }
                };
                Add(tab);

                tab = new HeroSelectSceneIconTab(this, new ItemSprite(ItemSpriteSheet.MASTERY, null));
                tab.action = (value) =>
                {
                    // base.select(value); ->  호출된 상태
                    if (value)
                    {
                        string msg = Messages.Get(cl, cl.ToString() + "_desc_subclasses");
                        foreach (HeroSubClass sub in cl.SubClasses())
                            msg += "\n\n" + sub.Desc();

                        info.Text(msg, INFO_WIDTH);
                    }
                };
                Add(tab);

                info = PixelScene.RenderTextBlock(6);
                info.SetPos(MARGIN, MARGIN);
                Add(info);

                Select(0);
            }

            private class HeroSelectSceneIconTab : IconTab
            {
                public HeroSelectSceneIconTab(WndTabbed owner, Image icon)
                    : base(owner, icon)
                { }

                internal Action<bool> action;

                public override void Select(bool value)
                {
                    base.Select(value);
                    if (action != null)
                        action(value);
                }
            }

            public override void Select(Tab tab)
            {
                base.Select(tab);
                Resize(WIDTH, (int)info.Bottom() + MARGIN);
                LayoutTabs();
            }
        }
    }
}
