using spdd.actors.hero;
using spdd.items;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;

namespace spdd.windows
{
    public class WndChooseWay : Window
    {
        private const int WIDTH = 120;
        private const int BTN_HEIGHT = 18;
        private const float GAP = 2;

        public WndChooseWay(TomeOfMastery tome, HeroSubClass way1, HeroSubClass way2)
        {
            IconTitle titlebar = new IconTitle();
            titlebar.Icon(new ItemSprite(tome.Image(), null));
            titlebar.Label(tome.Name());
            titlebar.SetRect(0, 0, WIDTH, 0);
            Add(titlebar);

            RenderedTextBlock hl = PixelScene.RenderTextBlock(6);
            hl.Text(way1.Desc() + "\n\n" + way2.Desc() + "\n\n" + Messages.Get(this, "message"), WIDTH);
            hl.SetPos(titlebar.Left(), titlebar.Bottom() + GAP);
            Add(hl);

            var btnWay1 = new ActionRedButton(way1.Title().ToUpperInvariant());
            btnWay1.action = () =>
            {
                Hide();
                tome.Choose(way1);
            };
            btnWay1.SetRect(0, hl.Bottom() + GAP, (WIDTH - GAP) / 2, BTN_HEIGHT);
            Add(btnWay1);

            var btnWay2 = new ActionRedButton(way2.Title().ToUpperInvariant());
            btnWay2.action = () =>
            {
                Hide();
                tome.Choose(way2);
            };
            btnWay2.SetRect(btnWay1.Right() + GAP, btnWay1.Top(), btnWay1.Width(), BTN_HEIGHT);
            Add(btnWay2);

            var btnCancel = new ActionRedButton(Messages.Get(this, "cancel"));
            btnCancel.action = () => Hide();
            btnCancel.SetRect(0, btnWay2.Bottom() + GAP, WIDTH, BTN_HEIGHT);
            Add(btnCancel);

            Resize(WIDTH, (int)btnCancel.Bottom());
        }
    }
}