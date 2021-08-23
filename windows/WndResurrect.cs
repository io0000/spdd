using watabou.noosa;
using spdd.actors.hero;
using spdd.items;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;

namespace spdd.windows
{
    public class WndResurrect : Window
    {
        private const int WIDTH = 120;
        private const int BTN_HEIGHT = 20;
        private const float GAP = 2;

        public static WndResurrect instance;
        public static object causeOfDeath;

        public WndResurrect(Ankh ankh, object causeOfDeath) 
            : base()
        {
            instance = this;
            WndResurrect.causeOfDeath = causeOfDeath;

            IconTitle titlebar = new IconTitle();
            titlebar.Icon(new ItemSprite(ankh.Image(), null));
            titlebar.Label(Messages.TitleCase(ankh.Name()));
            titlebar.SetRect(0, 0, WIDTH, 0);
            Add(titlebar);

            RenderedTextBlock message = PixelScene.RenderTextBlock(Messages.Get(this, "message"), 6);
            message.MaxWidth(WIDTH);
            message.SetPos(0, titlebar.Bottom() + GAP);
            Add(message);

            var btnYes = new ActionRedButton(Messages.Get(this, "yes"));
            btnYes.action = () =>
            {
                Hide();

                ++Statistics.ankhsUsed;

                InterlevelScene.mode = InterlevelScene.Mode.RESURRECT;
                Game.SwitchScene(typeof(InterlevelScene));
            };
            btnYes.SetRect(0, message.Top() + message.Height() + GAP, WIDTH, BTN_HEIGHT);
            Add(btnYes);

            var btnNo = new ActionRedButton(Messages.Get(this, "no"));
            btnNo.action = () =>
            {
                Hide();

                Hero.ReallyDie(WndResurrect.causeOfDeath);
                Rankings.Instance.Submit(false, WndResurrect.causeOfDeath.GetType());
            };
            btnNo.SetRect(0, btnYes.Bottom() + GAP, WIDTH, BTN_HEIGHT);
            Add(btnNo);

            Resize(WIDTH, (int)btnNo.Bottom());
        }

        public override void Destroy()
        {
            base.Destroy();
            instance = null;
        }

        public override void OnBackPressed()
        { }
    }
}