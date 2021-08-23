using System;
using watabou.input;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.particles;
using watabou.noosa.ui;
using watabou.utils;
using spdd.effects;
using spdd.items;
using spdd.scenes;
using spdd.sprites;
using spdd.windows;

namespace spdd.ui
{
    public class StatusPane : Component
    {
        private NinePatch bg;
        private Image avatar;
        private float warning;

        private int lastTier;

        private Image rawShielding;
        private Image shieldedHP;
        private Image hp;
        private Image exp;

        private BossHealthBar bossHP;

        private int lastLvl = -1;

        private BitmapText level;
        private BitmapText depth;

        private DangerIndicator danger;
        private BuffIndicator buffs;
        private Compass compass;

        private JournalButton btnJournal;
        private MenuButton btnMenu;

        private Toolbar.PickedUpItem pickedUp;

        private BitmapText version;

        protected override void CreateChildren()
        {
            bg = new NinePatch(Assets.Interfaces.STATUS, 0, 0, 128, 36, 85, 0, 45, 0);
            Add(bg);

            Add(new StatusPaneButton().SetRect(0, 1, 30, 30));

            btnJournal = new JournalButton();
            Add(btnJournal);

            btnMenu = new MenuButton();
            Add(btnMenu);

            avatar = HeroSprite.Avatar(Dungeon.hero.heroClass, lastTier);
            Add(avatar);

            compass = new Compass(Statistics.amuletObtained ? Dungeon.level.entrance : Dungeon.level.exit);
            Add(compass);

            rawShielding = new Image(Assets.Interfaces.SHLD_BAR);
            rawShielding.Alpha(0.5f);
            Add(rawShielding);

            shieldedHP = new Image(Assets.Interfaces.SHLD_BAR);
            Add(shieldedHP);

            hp = new Image(Assets.Interfaces.HP_BAR);
            Add(hp);

            exp = new Image(Assets.Interfaces.XP_BAR);
            Add(exp);

            bossHP = new BossHealthBar();
            Add(bossHP);

            level = new BitmapText(PixelScene.pixelFont);
            level.Hardlight(new Color(0xFF, 0xEB, 0xA4, 0xFF));
            Add(level);

            depth = new BitmapText(Dungeon.depth.ToString(), PixelScene.pixelFont);
            depth.Hardlight(new Color(0xCA, 0xCF, 0xC2, 0xFF));
            depth.Measure();
            Add(depth);

            danger = new DangerIndicator();
            Add(danger);

            buffs = new BuffIndicator(Dungeon.hero);
            Add(buffs);

            Add(pickedUp = new Toolbar.PickedUpItem());

            version = new BitmapText("v" + Game.version, PixelScene.pixelFont);
            version.Alpha(0.5f);
            Add(version);
        }

        private class StatusPaneButton : Button
        {
            protected override void OnClick()
            {
                Camera.main.PanTo(Dungeon.hero.sprite.Center(), 5f);
                GameScene.Show(new WndHero());
            }

            public override GameAction KeyAction()
            {
                return SPDAction.HERO_INFO;
            }
        }

        protected override void Layout()
        {
            height = 32;

            bg.Size(width, bg.height);

            avatar.x = bg.x + 15 - avatar.width / 2f;
            avatar.y = bg.y + 16 - avatar.height / 2f;
            PixelScene.Align(avatar);

            compass.x = avatar.x + avatar.width / 2f - compass.origin.x;
            compass.y = avatar.y + avatar.height / 2f - compass.origin.y;
            PixelScene.Align(compass);

            hp.x = shieldedHP.x = rawShielding.x = 30;
            hp.y = shieldedHP.y = rawShielding.y = 3;

            bossHP.SetPos(6 + (width - bossHP.Width()) / 2, 20);

            depth.x = width - 35.5f - depth.Width() / 2f;
            depth.y = 8f - depth.BaseLine() / 2f;
            PixelScene.Align(depth);

            danger.SetPos(width - danger.Width(), 20);

            buffs.SetPos(31, 9);

            btnJournal.SetPos(width - 42, 1);

            btnMenu.SetPos(width - btnMenu.Width(), 1);

            version.scale.Set(PixelScene.Align(0.5f));
            version.Measure();
            version.x = width - version.Width();
            version.y = btnMenu.Bottom() + (4 - version.BaseLine());
            PixelScene.Align(version);
        }

        private static Color[] warningColors = new Color[] {
            new Color(0x66, 0x00, 0x00, 0xFF),
            new Color(0xCC, 0x00, 0x00, 0xFF),
            new Color(0x66, 0x00, 0x00, 0xFF)
        };

        public override void Update()
        {
            base.Update();

            float health = Dungeon.hero.HP;
            float shield = Dungeon.hero.Shielding();
            float max = Dungeon.hero.HT;

            if (!Dungeon.hero.IsAlive())
            {
                avatar.Tint(new Color(0x00, 0x00, 0x00, 0xFF), 0.5f);
            }
            else if ((health / max) < 0.3f)
            {
                warning += Game.elapsed * 5f * (0.4f - (health / max));
                warning %= 1f;
                avatar.Tint(ColorMath.Interpolate(warning, warningColors), 0.5f);
            }
            else
            {
                avatar.ResetColor();
            }

            hp.scale.x = Math.Max(0, (health - shield) / max);
            shieldedHP.scale.x = health / max;
            rawShielding.scale.x = shield / max;

            exp.scale.x = (width / exp.width) * Dungeon.hero.exp / Dungeon.hero.MaxExp();

            if (Dungeon.hero.lvl != lastLvl)
            {
                if (lastLvl != -1)
                {
                    Emitter emitter = (Emitter)Recycle<Emitter>();
                    emitter.Revive();
                    emitter.Pos(27, 27);
                    emitter.Burst(Speck.Factory(Speck.STAR), 12);
                }

                lastLvl = Dungeon.hero.lvl;
                level.Text(lastLvl.ToString());
                level.Measure();
                level.x = 27.5f - level.Width() / 2f;
                level.y = 28.0f - level.BaseLine() / 2f;
                PixelScene.Align(level);
            }

            int tier = Dungeon.hero.Tier();
            if (tier != lastTier)
            {
                lastTier = tier;
                avatar.Copy(HeroSprite.Avatar(Dungeon.hero.heroClass, tier));
            }
        }

        public void Pickup(Item item, int cell)
        {
            pickedUp.Reset(item,
                cell,
                btnJournal.journalIcon.x + btnJournal.journalIcon.Width() / 2f,
                btnJournal.journalIcon.y + btnJournal.journalIcon.Height() / 2f);
        }

        public void Flash()
        {
            btnJournal.flashing = true;
        }

        public void UpdateKeys()
        {
            btnJournal.UpdateKeyDisplay();
        }

        private class JournalButton : Button
        {
            internal Image bg;
            internal Image journalIcon;
            internal KeyDisplay keyIcon;

            internal bool flashing;

            public JournalButton()
            {
                width = bg.width + 13; //includes the depth display to the left
                height = bg.height + 4;
            }

            public override GameAction KeyAction()
            {
                return SPDAction.JOURNAL;
            }

            protected override void CreateChildren()
            {
                base.CreateChildren();

                bg = new Image(Assets.Interfaces.MENU, 2, 2, 13, 11);
                Add(bg);

                journalIcon = new Image(Assets.Interfaces.MENU, 31, 0, 11, 7);
                Add(journalIcon);

                keyIcon = new KeyDisplay();
                Add(keyIcon);
                UpdateKeyDisplay();
            }

            protected override void Layout()
            {
                base.Layout();

                bg.x = x + 13;
                bg.y = y + 2;

                journalIcon.x = bg.x + (bg.Width() - journalIcon.Width()) / 2f;
                journalIcon.y = bg.y + (bg.Height() - journalIcon.Height()) / 2f;
                PixelScene.Align(journalIcon);

                keyIcon.x = bg.x + 1;
                keyIcon.y = bg.y + 1;
                keyIcon.width = bg.width - 2;
                keyIcon.height = bg.height - 2;
                PixelScene.Align(keyIcon);
            }

            private float time;

            public override void Update()
            {
                base.Update();

                if (flashing)
                {
                    journalIcon.am = (float)Math.Abs(Math.Cos(3 * (time += Game.elapsed)));
                    keyIcon.am = journalIcon.am;
                    if (time >= 0.333f * Math.PI)
                    {
                        time = 0;
                    }
                }
            }

            public void UpdateKeyDisplay()
            {
                keyIcon.UpdateKeys();
                keyIcon.visible = keyIcon.KeyCount() > 0;
                journalIcon.visible = !keyIcon.visible;
                if (keyIcon.KeyCount() > 0)
                {
                    bg.Brightness(.8f - (Math.Min(6, keyIcon.KeyCount()) / 20f));
                }
                else
                {
                    bg.ResetColor();
                }
            }

            protected override void OnPointerDown()
            {
                bg.Brightness(1.5f);
                Sample.Instance.Play(Assets.Sounds.CLICK);
            }

            protected override void OnPointerUp()
            {
                if (keyIcon.KeyCount() > 0)
                {
                    bg.Brightness(.8f - (Math.Min(6, keyIcon.KeyCount()) / 20f));
                }
                else
                {
                    bg.ResetColor();
                }
            }

            protected override void OnClick()
            {
                flashing = false;
                time = 0;
                keyIcon.am = journalIcon.am = 1;
                GameScene.Show(new WndJournal());
            }
        }

        private class MenuButton : Button
        {
            private Image image;

            public MenuButton()
            {
                width = image.width + 4;
                height = image.height + 4;
            }

            protected override void CreateChildren()
            {
                base.CreateChildren();

                image = new Image(Assets.Interfaces.MENU, 17, 2, 12, 11);
                Add(image);
            }

            protected override void Layout()
            {
                base.Layout();

                image.x = x + 2;
                image.y = y + 2;
            }

            protected override void OnPointerDown()
            {
                image.Brightness(1.5f);
                Sample.Instance.Play(Assets.Sounds.CLICK);
            }

            protected override void OnPointerUp()
            {
                image.ResetColor();
            }

            protected override void OnClick()
            {
                GameScene.Show(new WndGame());
            }
        }
    }
}