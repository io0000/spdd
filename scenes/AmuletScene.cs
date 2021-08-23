using watabou.utils;
using watabou.noosa;
using spdd.effects;
using spdd.ui;
using spdd.messages;
using spdd.items;

namespace spdd.scenes
{
    public class AmuletScene : PixelScene
    {
        private const int WIDTH = 120;
        private const int BTN_HEIGHT = 18;
        private const float SMALL_GAP = 2;
        private const float LARGE_GAP = 8;

        public static bool noText;

        private Image amulet;

        public override void Create()
        {
            base.Create();

            RenderedTextBlock text = null;
            if (!noText)
            {
                text = RenderTextBlock(Messages.Get(this, "text"), 8);
                text.MaxWidth(WIDTH);
                Add(text);
            }

            amulet = new Image(Assets.Sprites.AMULET);
            Add(amulet);

            var btnExit = new ActionRedButton(Messages.Get(this, "exit"));
            btnExit.action = () =>
            {
                Dungeon.Win(typeof(Amulet));
                Dungeon.DeleteGame(GamesInProgress.curSlot, true);
                Game.SwitchScene(typeof(RankingsScene));
            };

            btnExit.SetSize(WIDTH, BTN_HEIGHT);
            Add(btnExit);

            var btnStay = new ActionRedButton(Messages.Get(this, "stay"));
            btnStay.action = () =>
            {
                OnBackPressed();
            };

            btnStay.SetSize(WIDTH, BTN_HEIGHT);
            Add(btnStay);

            float height;
            if (noText)
            {
                height = amulet.height + LARGE_GAP + btnExit.Height() + SMALL_GAP + btnStay.Height();

                amulet.x = (Camera.main.width - amulet.width) / 2;
                amulet.y = (Camera.main.height - height) / 2;
                Align(amulet);

                btnExit.SetPos((Camera.main.width - btnExit.Width()) / 2, amulet.y + amulet.height + LARGE_GAP);
                btnStay.SetPos(btnExit.Left(), btnExit.Bottom() + SMALL_GAP);
            }
            else
            {
                height = amulet.height + LARGE_GAP + text.Height() + LARGE_GAP + btnExit.Height() + SMALL_GAP + btnStay.Height();

                amulet.x = (Camera.main.width - amulet.width) / 2;
                amulet.y = (Camera.main.height - height) / 2;
                Align(amulet);

                text.SetPos((Camera.main.width - text.Width()) / 2, amulet.y + amulet.height + LARGE_GAP);
                Align(text);

                btnExit.SetPos((Camera.main.width - btnExit.Width()) / 2, text.Top() + text.Height() + LARGE_GAP);
                btnStay.SetPos(btnExit.Left(), btnExit.Bottom() + SMALL_GAP);
            }

            new Flare(8, 48).Color(new Color(0xFF, 0xDD, 0xBB, 0xFF), true).Show(amulet, 0).angularSpeed = +30;

            FadeIn();
        }

        public override void OnBackPressed()
        {
            InterlevelScene.mode = InterlevelScene.Mode.CONTINUE;
            Game.SwitchScene(typeof(InterlevelScene));
        }

        private float timer;

        public override void Update()
        {
            base.Update();

            if ((timer -= Game.elapsed) < 0)
            {
                timer = Rnd.Float(0.5f, 5f);

                var star = Recycle<Speck>();
                star.Reset(0, amulet.x + 10.5f, amulet.y + 5.5f, Speck.DISCOVER);
                Add(star);
            }
        }
    }
}