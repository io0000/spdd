using watabou.gltextures;
using watabou.glwrap;
using watabou.noosa;
using spdd.actors.buffs;
using spdd.messages;
using spdd.scenes;
using spdd.ui;

namespace spdd.windows
{
    public class WndInfoBuff : Window
    {
        private const float GAP = 2;

        private const int WIDTH = 120;

        private Texture icons;
        private TextureFilm film;

        public WndInfoBuff(Buff buff)
        {
            IconTitle titlebar = new IconTitle();

            icons = TextureCache.Get(Assets.Interfaces.BUFFS_LARGE);
            film = new TextureFilm(icons, 16, 16);

            Image buffIcon = new Image(icons);
            buffIcon.Frame(film.Get(buff.Icon()));
            buff.TintIcon(buffIcon);

            titlebar.Icon(buffIcon);
            titlebar.Label(Messages.TitleCase(buff.ToString()), Window.TITLE_COLOR);
            titlebar.SetRect(0, 0, WIDTH, 0);
            Add(titlebar);

            RenderedTextBlock txtInfo = PixelScene.RenderTextBlock(buff.Desc(), 6);
            txtInfo.MaxWidth(WIDTH);
            txtInfo.SetPos(titlebar.Left(), titlebar.Bottom() + 2 * GAP);
            Add(txtInfo);

            Resize(WIDTH, (int)txtInfo.Bottom() + 2);
        }
    }
}