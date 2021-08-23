using System;
using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;
using spdd.actors.hero;
using spdd.journal;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;

namespace spdd.windows
{
    public class WndStartGame : Window
    {
        private const int WIDTH = 120;
        private const int HEIGHT = 140;

        public WndStartGame(int slot)
        {
            BadgesExtensions.LoadGlobal();
            Journal.LoadGlobal();

            RenderedTextBlock title = PixelScene.RenderTextBlock(Messages.Get(this, "title"), 12);
            title.Hardlight(Window.TITLE_COLOR);
            title.SetPos((WIDTH - title.Width()) / 2f, 3);
            PixelScene.Align(title);
            Add(title);

            float heroBtnSpacing = (WIDTH - 4 * HeroBtn.WIDTH) / 5f;

            float curX = heroBtnSpacing;
            foreach (HeroClass cl in Enum.GetValues(typeof(HeroClass)))
            {
                HeroBtn button = new HeroBtn(cl);
                button.SetRect(curX, title.Height() + 7, HeroBtn.WIDTH, HeroBtn.HEIGHT);
                curX += HeroBtn.WIDTH + heroBtnSpacing;
                Add(button);
            }

            ColorBlock separator = new ColorBlock(1, 1, new Color(0x22, 0x22, 0x22, 0xFF));
            separator.Size(WIDTH, 1);
            separator.x = 0;
            separator.y = title.Bottom() + 6 + HeroBtn.HEIGHT;
            Add(separator);

            HeroPane ava = new HeroPane();
            ava.SetRect(20, separator.y + 2, WIDTH - 30, 80);
            Add(ava);

            var start = new StartButton(Messages.Get(this, "start"));
            start.slot = slot;
            start.visible = false;
            start.SetRect(0, HEIGHT - 20, WIDTH, 20);
            Add(start);

            if (DeviceCompat.IsDebug() || BadgesExtensions.IsUnlocked(Badges.Badge.VICTORY))
            {
                Icons icons = SPDSettings.Challenges() > 0 ? Icons.CHALLENGE_ON : Icons.CHALLENGE_OFF;
                Image image = IconsExtensions.Get(icons);

                IconButton challengeButton = new IconButton(image);
                challengeButton.SetRect(WIDTH - 20, HEIGHT - 20, 20, 20);
                challengeButton.visible = false;
                Add(challengeButton);
            }
            else
            {
                Dungeon.challenges = 0;
                SPDSettings.Challenges(0);
            }

            Resize(WIDTH, HEIGHT);
        }

        private class StartButton : ActionRedButton
        {
            internal int slot;

            public StartButton(string label)
                : base(label)
            { }

            protected override void OnClick()
            {
                if (GamesInProgress.selectedClass == null)
                    return;

                base.OnClick();

                GamesInProgress.curSlot = slot;
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

            public override void Update()
            {
                if (!visible && GamesInProgress.selectedClass != null)
                {
                    visible = true;
                }
                base.Update();
            }
        }

        private class ChallengeButton : IconButton
        {
            public ChallengeButton(Image icon)
                : base(icon)
            { }

            protected override void OnClick()
            {
                var wnd = new WndChallengesEx(SPDSettings.Challenges(), true);
                wnd.challengeButton = this;
                ShatteredPixelDungeonDash.Scene().AddToFront(wnd);
            }

            public override void Update()
            {
                if (!visible && GamesInProgress.selectedClass != null)
                    visible = true;

                base.Update();
            }

            private class WndChallengesEx : WndChallenges
            {
                internal ChallengeButton challengeButton;

                public WndChallengesEx(int checkedValue, bool editable)
                    : base(checkedValue, editable)
                { }

                public override void OnBackPressed()
                {
                    base.OnBackPressed();
                    if (parent != null)
                    {
                        var icon = SPDSettings.Challenges() > 0 ?
                                Icons.CHALLENGE_ON : Icons.CHALLENGE_OFF;

                        Image image = IconsExtensions.Get(icon);
                        challengeButton.Icon(image);
                    }
                }
            }
        }

        private class HeroBtn : Button
        {
            internal HeroClass cl;

            internal Image hero;

            internal const int WIDTH = 24;
            internal const int HEIGHT = 16;

            public HeroBtn(HeroClass cl)
            {
                this.cl = cl;

                Add(hero = new Image(cl.Spritesheet(), 0, 90, 12, 15));
            }

            protected override void Layout()
            {
                base.Layout();
                if (hero != null)
                {
                    hero.x = x + (width - hero.Width()) / 2f;
                    hero.y = y + (height - hero.Height()) / 2f;
                    PixelScene.Align(hero);
                }
            }

            public override void Update()
            {
                base.Update();
                if (cl != GamesInProgress.selectedClass)
                {
                    if (!cl.IsUnlocked())
                        hero.Brightness(0.3f);
                    else
                        hero.Brightness(0.6f);
                }
                else
                {
                    hero.Brightness(1f);
                }
            }

            protected override void OnClick()
            {
                base.OnClick();

                if (!cl.IsUnlocked())
                {
                    ShatteredPixelDungeonDash.Scene().AddToFront(new WndMessage(cl.UnlockMsg()));
                }
                else
                {
                    GamesInProgress.selectedClass = cl;
                }
            }
        }

        private class HeroPane : Component
        {
            internal HeroClass? cl;

            internal Image avatar;

            internal ActionIconButton heroItem;
            internal ActionIconButton heroLoadout;
            internal ActionIconButton heroMisc;
            internal ActionIconButton heroSubclass;

            internal RenderedTextBlock name;

            private const int BTN_SIZE = 20;

            protected override void CreateChildren()
            {
                base.CreateChildren();

                avatar = new Image(Assets.Sprites.AVATARS);
                avatar.scale.Set(2f);
                Add(avatar);

                heroItem = new ActionIconButton();
                heroItem.action = () =>
                {
                    if (cl == null)
                        return;

                    var type = cl.Value;
                    var wnd = new WndMessage(Messages.Get(type, type.ToString() + "_desc_item"));
                    ShatteredPixelDungeonDash.Scene().AddToFront(wnd);
                };
                heroItem.SetSize(BTN_SIZE, BTN_SIZE);
                Add(heroItem);

                heroLoadout = new ActionIconButton();
                heroLoadout.action = () =>
                {
                    if (cl == null)
                        return;

                    var type = cl.Value;
                    var wnd = new WndMessage(Messages.Get(type, type.ToString() + "_desc_loadout"));
                    ShatteredPixelDungeonDash.Scene().AddToFront(wnd);
                };
                heroLoadout.SetSize(BTN_SIZE, BTN_SIZE);
                Add(heroLoadout);

                heroMisc = new ActionIconButton();
                heroMisc.action = () =>
                {
                    if (cl == null)
                        return;

                    var type = cl.Value;
                    var wnd = new WndMessage(Messages.Get(type, type.ToString() + "_desc_misc"));
                    ShatteredPixelDungeonDash.Scene().AddToFront(wnd);
                };
                heroMisc.SetSize(BTN_SIZE, BTN_SIZE);
                Add(heroMisc);

                heroSubclass = new ActionIconButton(new ItemSprite(ItemSpriteSheet.MASTERY, null));
                heroSubclass.action = () =>
                {
                    if (cl == null)
                        return;

                    var type = cl.Value;

                    string msg = Messages.Get(type, type.ToString() + "_desc_subclasses");
                    foreach (HeroSubClass sub in type.SubClasses())
                    {
                        msg += "\n\n" + sub.Desc();
                    }

                    var wnd = new WndMessage(msg);
                    ShatteredPixelDungeonDash.Scene().AddToFront(wnd);
                };
                heroSubclass.SetSize(BTN_SIZE, BTN_SIZE);
                Add(heroSubclass);

                name = PixelScene.RenderTextBlock(12);
                Add(name);

                visible = false;
            }

            protected override void Layout()
            {
                base.Layout();

                avatar.x = x;
                avatar.y = y + (height - avatar.Height() - name.Height() - 4) / 2f;
                PixelScene.Align(avatar);

                name.SetPos(
                        x + (avatar.Width() - name.Width()) / 2f,
                        avatar.y + avatar.Height() + 3
                );
                PixelScene.Align(name);

                heroItem.SetPos(x + width - BTN_SIZE, y);
                heroLoadout.SetPos(x + width - BTN_SIZE, heroItem.Bottom());
                heroMisc.SetPos(x + width - BTN_SIZE, heroLoadout.Bottom());
                heroSubclass.SetPos(x + width - BTN_SIZE, heroMisc.Bottom());
            }

            public override void Update()
            {
                base.Update();
                if (GamesInProgress.selectedClass != cl)
                {
                    cl = GamesInProgress.selectedClass;
                    if (cl != null)
                    {
                        avatar.Frame((int)cl.Value * 24, 0, 24, 32);

                        name.Text(Messages.Capitalize(cl?.Title()));

                        switch (cl)
                        {
                            case HeroClass.WARRIOR:
                                heroItem.Icon(new ItemSprite(ItemSpriteSheet.SEAL, null));
                                heroLoadout.Icon(new ItemSprite(ItemSpriteSheet.WORN_SHORTSWORD, null));
                                heroMisc.Icon(new ItemSprite(ItemSpriteSheet.RATION, null));
                                break;
                            case HeroClass.MAGE:
                                heroItem.Icon(new ItemSprite(ItemSpriteSheet.MAGES_STAFF, null));
                                heroLoadout.Icon(new ItemSprite(ItemSpriteSheet.HOLDER, null));
                                heroMisc.Icon(new ItemSprite(ItemSpriteSheet.WAND_MAGIC_MISSILE, null));
                                break;
                            case HeroClass.ROGUE:
                                heroItem.Icon(new ItemSprite(ItemSpriteSheet.ARTIFACT_CLOAK, null));
                                heroLoadout.Icon(new ItemSprite(ItemSpriteSheet.DAGGER, null));
                                heroMisc.Icon(Icons.DEPTH.Get());
                                break;
                            case HeroClass.HUNTRESS:
                                heroItem.Icon(new ItemSprite(ItemSpriteSheet.SPIRIT_BOW, null));
                                heroLoadout.Icon(new ItemSprite(ItemSpriteSheet.GLOVES, null));
                                heroMisc.Icon(new Image(Assets.Environment.TILES_SEWERS, 112, 96, 16, 16));
                                break;
                        }

                        Layout();

                        visible = true;
                    }
                    else
                    {
                        visible = false;
                    }
                }
            }
        }
    }
}
