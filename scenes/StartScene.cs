using System;
using System.Collections.Generic;
using watabou.noosa;
using watabou.noosa.ui;
using spdd.actors.hero;
using spdd.journal;
using spdd.messages;
using spdd.ui;
using spdd.windows;

namespace spdd.scenes
{
    public class StartScene : PixelScene
    {
        private const int SLOT_WIDTH = 120;
        private const int SLOT_HEIGHT = 30;

        public override void Create()
        {
            base.Create();

            BadgesExtensions.LoadGlobal();
            Journal.LoadGlobal();

            uiCamera.visible = false;

            var w = Camera.main.width;
            var h = Camera.main.height;

            var archs = new Archs();
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

            List<GamesInProgress.Info> games = GamesInProgress.CheckAll();

            int slotGap = Landscape() ? 5 : 10;
            int slotCount = Math.Min(GamesInProgress.MAX_SLOTS, games.Count + 1);
            int slotsHeight = slotCount * SLOT_HEIGHT + (slotCount - 1) * slotGap;

            float yPos = (h - slotsHeight) / 2f;
            if (Landscape())
                yPos += 8;

            foreach (GamesInProgress.Info game in games)
            {
                SaveSlotButton existingGame = new SaveSlotButton();
                existingGame.Set(game.slot);
                existingGame.SetRect((w - SLOT_WIDTH) / 2f, yPos, SLOT_WIDTH, SLOT_HEIGHT);
                yPos += SLOT_HEIGHT + slotGap;
                Align(existingGame);
                Add(existingGame);
            }

            if (games.Count < GamesInProgress.MAX_SLOTS)
            {
                SaveSlotButton newGame = new SaveSlotButton();
                newGame.Set(GamesInProgress.FirstEmpty());
                newGame.SetRect((w - SLOT_WIDTH) / 2f, yPos, SLOT_WIDTH, SLOT_HEIGHT);
                yPos += SLOT_HEIGHT + slotGap;
                Align(newGame);
                Add(newGame);
            }

            GamesInProgress.curSlot = 0;

            FadeIn();
        }

        public override void OnBackPressed()
        {
            ShatteredPixelDungeonDash.SwitchNoFade(typeof(TitleScene));
        }

        public class SaveSlotButton : Button
        {
            private NinePatch bg;

            private Image hero;
            private RenderedTextBlock name;

            private Image steps;
            private BitmapText depth;
            private Image classIcon;
            private BitmapText level;

            private int slot;
            private bool newGame;

            protected override void CreateChildren()
            {
                base.CreateChildren();

                bg = Chrome.Get(Chrome.Type.GEM);
                Add(bg);

                name = PixelScene.RenderTextBlock(9);
                Add(name);
            }

            public void Set(int slot)
            {
                this.slot = slot;
                GamesInProgress.Info info = GamesInProgress.Check(slot);
                newGame = info == null;
                if (newGame)
                {
                    name.Text(Messages.Get(typeof(StartScene), "new"));

                    if (hero != null)
                    {
                        Remove(hero);
                        hero = null;
                        Remove(steps);
                        steps = null;
                        Remove(depth);
                        depth = null;
                        Remove(classIcon);
                        classIcon = null;
                        Remove(level);
                        level = null;
                    }
                }
                else
                {
                    if (info.subClass != HeroSubClass.NONE)
                    {
                        name.Text(Messages.TitleCase(info.subClass.Title()));
                    }
                    else
                    {
                        name.Text(Messages.TitleCase(info.heroClass.Title()));
                    }

                    if (hero == null)
                    {
                        hero = new Image(info.heroClass.Spritesheet(), 0, 15 * info.armorTier, 12, 15);
                        Add(hero);

                        steps = new Image(Icons.DEPTH.Get());
                        Add(steps);
                        depth = new BitmapText(PixelScene.pixelFont);
                        Add(depth);

                        classIcon = new Image(IconsExtensions.Get(info.heroClass));
                        Add(classIcon);
                        level = new BitmapText(PixelScene.pixelFont);
                        Add(level);
                    }
                    else
                    {
                        hero.Copy(new Image(info.heroClass.Spritesheet(), 0, 15 * info.armorTier, 12, 15));

                        classIcon.Copy(IconsExtensions.Get(info.heroClass));
                    }

                    depth.Text(info.depth.ToString());
                    depth.Measure();

                    level.Text(info.level.ToString());
                    level.Measure();

                    if (info.challenges > 0)
                    {
                        name.Hardlight(Window.TITLE_COLOR);
                        depth.Hardlight(Window.TITLE_COLOR);
                        level.Hardlight(Window.TITLE_COLOR);
                    }
                    else
                    {
                        name.ResetColor();
                        depth.ResetColor();
                        level.ResetColor();
                    }
                }

                Layout();
            }

            protected override void Layout()
            {
                base.Layout();

                bg.x = x;
                bg.y = y;
                bg.Size(width, height);

                if (hero != null)
                {
                    hero.x = x + 8;
                    hero.y = y + (height - hero.Height()) / 2f;
                    Align(hero);

                    name.SetPos(
                            hero.x + hero.Width() + 6,
                            y + (height - name.Height()) / 2f
                    );
                    Align(name);

                    classIcon.x = x + width - 24 + (16 - classIcon.Width()) / 2f;
                    classIcon.y = y + (height - classIcon.Height()) / 2f;
                    Align(classIcon);

                    level.x = classIcon.x + (classIcon.Width() - level.Width()) / 2f;
                    level.y = classIcon.y + (classIcon.Height() - level.Height()) / 2f + 1;
                    Align(level);

                    steps.x = x + width - 40 + (16 - steps.Width()) / 2f;
                    steps.y = y + (height - steps.Height()) / 2f;
                    Align(steps);

                    depth.x = steps.x + (steps.Width() - depth.Width()) / 2f;
                    depth.y = steps.y + (steps.Height() - depth.Height()) / 2f + 1;
                    Align(depth);
                }
                else
                {
                    name.SetPos(
                            x + (width - name.Width()) / 2f,
                            y + (height - name.Height()) / 2f
                    );
                    Align(name);
                }
            }

            protected override void OnClick()
            {
                if (newGame)
                {
                    GamesInProgress.selectedClass = null;
                    GamesInProgress.curSlot = slot;
                    ShatteredPixelDungeonDash.SwitchScene(typeof(HeroSelectScene));
                }
                else
                {
                    ShatteredPixelDungeonDash.Scene().Add(new WndGameInProgress(slot));
                }
            }
        }
    }
}