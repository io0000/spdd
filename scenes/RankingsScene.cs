using System;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.ui;
using watabou.utils;
using spdd.actors.hero;
using spdd.effects;
using spdd.messages;
using spdd.sprites;
using spdd.ui;
using spdd.windows;

namespace spdd.scenes
{
    public class RankingsScene : PixelScene
    {
        private const float ROW_HEIGHT_MAX = 20;
        private const float ROW_HEIGHT_MIN = 12;

        private const float MAX_ROW_WIDTH = 160;

        private const float GAP = 4;

        private Archs archs;

        public override void Create()
        {
            base.Create();

            Music.Instance.Play(Assets.Music.THEME, true);

            uiCamera.visible = false;

            int w = Camera.main.width;
            int h = Camera.main.height;

            archs = new Archs();
            archs.SetSize(w, h);
            Add(archs);

            Rankings.Instance.Load();

            RenderedTextBlock title = PixelScene.RenderTextBlock(Messages.Get(this, "title"), 9);
            title.Hardlight(Window.TITLE_COLOR);
            title.SetPos(
                    (w - title.Width()) / 2f,
                    (20 - title.Height()) / 2f
            );
            Align(title);
            Add(title);

            if (Rankings.Instance.records.Count > 0)
            {
                //attempts to give each record as much space as possible, ideally as much space as portrait mode
                float rowHeight = GameMath.Gate(ROW_HEIGHT_MIN, (uiCamera.height - 26) / Rankings.Instance.records.Count, ROW_HEIGHT_MAX);

                float left = (w - Math.Min(MAX_ROW_WIDTH, w)) / 2 + GAP;
                float top = (h - rowHeight * Rankings.Instance.records.Count) / 2;

                int pos = 0;

                foreach (Rankings.Record rec in Rankings.Instance.records)
                {
                    var row = new Record(pos, pos == Rankings.Instance.lastRecord, rec);
                    float offset = 0;
                    if (rowHeight <= 14)
                        offset = (pos % 2 == 1) ? 5 : -5;

                    row.SetRect(left + offset, top + pos * rowHeight, w - left * 2, rowHeight);
                    Add(row);

                    ++pos;
                }

                if (Rankings.Instance.totalNumber >= Rankings.TABLE_SIZE)
                {
                    RenderedTextBlock label = PixelScene.RenderTextBlock(8);
                    label.Hardlight(new Color(0xCC, 0xCC, 0xCC, 0xFF));
                    label.SetHightlighting(true, Window.SHPX_COLOR);
                    label.Text(Messages.Get(this, "total") + " _" + Rankings.Instance.wonNumber + "_/" + Rankings.Instance.totalNumber);
                    Add(label);

                    label.SetPos(
                            (w - label.Width()) / 2,
                            h - label.Height() - 2 * GAP
                    );
                    Align(label);
                }
            }
            else
            {
                RenderedTextBlock noRec = PixelScene.RenderTextBlock(Messages.Get(this, "no_games"), 8);
                noRec.Hardlight(new Color(0xCC, 0xCC, 0xCC, 0xFF));
                noRec.SetPos(
                        (w - noRec.Width()) / 2,
                        (h - noRec.Height()) / 2
                );
                Align(noRec);
                Add(noRec);
            }

            ExitButton btnExit = new ExitButton();
            btnExit.SetPos(Camera.main.width - btnExit.Width(), 0);
            Add(btnExit);

            FadeIn();
        }

        public override void OnBackPressed()
        {
            ShatteredPixelDungeonDash.SwitchNoFade(typeof(TitleScene));
        }

        public class Record : Button
        {
            private const float GAP = 4;

            private static Color[] TEXT_WIN = { new Color(0xFF, 0xFF, 0x88, 0xFF), new Color(0xB2, 0xB2, 0x5F, 0xFF) };
            private static Color[] TEXT_LOSE = { new Color(0xDD, 0xDD, 0xDD, 0xFF), new Color(0x88, 0x88, 0x88, 0xFF) };
            private static Color FLARE_WIN = new Color(0x88, 0x88, 0x66, 0xFF);
            private static Color FLARE_LOSE = new Color(0x66, 0x66, 0x66, 0xFF);

            private Rankings.Record rec;

            protected ItemSprite shield;
            private Flare flare;
            private BitmapText position;
            private RenderedTextBlock desc;
            private Image steps;
            private BitmapText depth;
            private Image classIcon;
            private BitmapText level;

            public Record(int pos, bool latest, Rankings.Record rec)
            {
                this.rec = rec;

                if (latest)
                {
                    flare = new Flare(6, 24);
                    flare.angularSpeed = 90;
                    flare.SetColor(rec.win ? FLARE_WIN : FLARE_LOSE);
                    AddToBack(flare);
                }

                if (pos != Rankings.TABLE_SIZE - 1)
                {
                    var posStr = pos + 1;
                    position.Text(posStr.ToString());
                }
                else
                {
                    position.Text(" ");
                }
                position.Measure();

                desc.Text(Messages.TitleCase(rec.Desc()));

                int odd = pos % 2;

                if (rec.win)
                {
                    shield.View(ItemSpriteSheet.AMULET, null);
                    position.Hardlight(TEXT_WIN[odd]);
                    desc.Hardlight(TEXT_WIN[odd]);
                    depth.Hardlight(TEXT_WIN[odd]);
                    level.Hardlight(TEXT_WIN[odd]);
                }
                else
                {
                    position.Hardlight(TEXT_LOSE[odd]);
                    desc.Hardlight(TEXT_LOSE[odd]);
                    depth.Hardlight(TEXT_LOSE[odd]);
                    level.Hardlight(TEXT_LOSE[odd]);

                    if (rec.depth != 0)
                    {
                        depth.Text(rec.depth.ToString());
                        depth.Measure();
                        steps.Copy(Icons.DEPTH.Get());

                        Add(steps);
                        Add(depth);
                    }
                }

                if (rec.herolevel != 0)
                {
                    level.Text(rec.herolevel.ToString());
                    level.Measure();
                    Add(level);
                }

                classIcon.Copy(IconsExtensions.Get(rec.heroClass));
                if (rec.heroClass == HeroClass.ROGUE)
                {
                    //cloak of shadows needs to be brightened a bit
                    classIcon.Brightness(2f);
                }
            }

            protected override void CreateChildren()
            {
                base.CreateChildren();

                shield = new ItemSprite(ItemSpriteSheet.TOMB, null);
                Add(shield);

                position = new BitmapText(PixelScene.pixelFont);
                Add(position);

                desc = RenderTextBlock(7);
                Add(desc);

                depth = new BitmapText(PixelScene.pixelFont);

                steps = new Image();

                classIcon = new Image();
                Add(classIcon);

                level = new BitmapText(PixelScene.pixelFont);
            }

            protected override void Layout()
            {
                base.Layout();

                shield.x = x;
                shield.y = y + (height - shield.height) / 2f;
                Align(shield);

                position.x = shield.x + (shield.width - position.Width()) / 2f;
                position.y = shield.y + (shield.height - position.Height()) / 2f + 1;
                Align(position);

                if (flare != null)
                {
                    flare.Point(shield.Center());
                }

                classIcon.x = x + width - 16 + (16 - classIcon.Width()) / 2f;
                classIcon.y = shield.y + (16 - classIcon.Height()) / 2f;
                Align(classIcon);

                level.x = classIcon.x + (classIcon.width - level.Width()) / 2f;
                level.y = classIcon.y + (classIcon.height - level.Height()) / 2f + 1;
                Align(level);

                steps.x = x + width - 32 + (16 - steps.Width()) / 2f;
                steps.y = shield.y + (16 - steps.Height()) / 2f;
                Align(steps);

                depth.x = steps.x + (steps.width - depth.Width()) / 2f;
                depth.y = steps.y + (steps.height - depth.Height()) / 2f + 1;
                Align(depth);

                desc.MaxWidth((int)(steps.x - (shield.x + shield.width + GAP)));
                desc.SetPos(shield.x + shield.width + GAP, shield.y + (shield.height - desc.Height()) / 2f + 1);
                Align(desc);
            }

            protected override void OnClick()
            {
                if (rec.gameData != null)
                {
                    parent.Add(new WndRanking(rec));
                }
                else
                {
                    parent.Add(new WndError(Messages.Get(typeof(RankingsScene), "no_info")));
                }
            }
        } // Record
    }
}