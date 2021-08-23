using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;

namespace spdd.effects
{
    public class BadgeBanner : Image
    {
        private enum State
        {
            FadeIn,
            Static,
            FadeOut
        }

        private State state;

        private const float DEFAULT_SCALE = 3;

        private const float FADE_IN_TIME = 0.2f;
        private const float STATIC_TIME = 1f;
        private const float FADE_OUT_TIME = 1.0f;

        private int index;
        private float time;

        private static TextureFilm atlas;

        private static BadgeBanner current;

        private BadgeBanner(int index)
            : base(Assets.Interfaces.BADGES)
        {
            if (atlas == null)
                atlas = new TextureFilm(texture, 16, 16);

            this.index = index;

            Frame(atlas.Get(index));
            origin.Set(width / 2, height / 2);

            Alpha(0);
            scale.Set(2 * DEFAULT_SCALE);

            state = State.FadeIn;
            time = FADE_IN_TIME;

            Sample.Instance.Play(Assets.Sounds.BADGE);
        }

        public override void Update()
        {
            base.Update();

            time -= Game.elapsed;
            if (time >= 0)
            {
                switch (state)
                {
                    case State.FadeIn:
                        var p = time / FADE_IN_TIME;
                        scale.Set((1 + p) * DEFAULT_SCALE);
                        Alpha(1 - p);
                        break;
                    case State.Static:
                        break;
                    case State.FadeOut:
                        Alpha(time / FADE_OUT_TIME);
                        break;
                }
            }
            else
            {
                switch (state)
                {
                    case State.FadeIn:
                        time = STATIC_TIME;
                        state = State.Static;
                        scale.Set(DEFAULT_SCALE);
                        Alpha(1);
                        Highlight(this, index);
                        break;
                    case State.Static:
                        time = FADE_OUT_TIME;
                        state = State.FadeOut;
                        break;
                    case State.FadeOut:
                        KillAndErase();
                        break;
                }
            }
        }

        public override void Kill()
        {
            if (current == this)
                current = null;

            base.Kill();
        }

        public static void Highlight(Image image, int index)
        {
            var p = new PointF();

            switch (index)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    p.Offset(7, 3);
                    break;
                case 4:
                case 5:
                case 6:
                case 7:
                    p.Offset(6, 5);
                    break;
                case 8:
                case 9:
                case 10:
                case 11:
                    p.Offset(6, 3);
                    break;
                case 12:
                case 13:
                case 14:
                case 15:
                    p.Offset(7, 4);
                    break;
                case 16:
                    p.Offset(6, 3);
                    break;
                case 17:
                    p.Offset(5, 4);
                    break;
                case 18:
                    p.Offset(7, 3);
                    break;
                case 20:
                    p.Offset(7, 3);
                    break;
                case 21:
                    p.Offset(7, 3);
                    break;
                case 22:
                    p.Offset(6, 4);
                    break;
                case 23:
                    p.Offset(4, 5);
                    break;
                case 24:
                    p.Offset(6, 4);
                    break;
                case 25:
                    p.Offset(6, 5);
                    break;
                case 26:
                    p.Offset(5, 5);
                    break;
                case 27:
                    p.Offset(6, 4);
                    break;
                case 28:
                    p.Offset(3, 5);
                    break;
                case 29:
                    p.Offset(5, 4);
                    break;
                case 30:
                    p.Offset(5, 4);
                    break;
                case 31:
                    p.Offset(5, 5);
                    break;
                case 32:
                case 33:
                    p.Offset(7, 4);
                    break;
                case 34:
                    p.Offset(6, 4);
                    break;
                case 35:
                    p.Offset(6, 4);
                    break;
                case 36:
                    p.Offset(6, 5);
                    break;
                case 37:
                    p.Offset(4, 4);
                    break;
                case 38:
                    p.Offset(5, 5);
                    break;
                case 39:
                    p.Offset(5, 4);
                    break;
                case 40:
                case 41:
                case 42:
                case 43:
                    p.Offset(5, 4);
                    break;
                case 44:
                case 45:
                case 46:
                case 47:
                    p.Offset(5, 5);
                    break;
                case 48:
                case 49:
                case 50:
                case 51:
                    p.Offset(7, 4);
                    break;
                case 52:
                case 53:
                case 54:
                case 55:
                    p.Offset(4, 4);
                    break;
                case 56:
                    p.Offset(3, 7);
                    break;
                case 57:
                    p.Offset(4, 5);
                    break;
                case 58:
                    p.Offset(6, 4);
                    break;
                case 59:
                    p.Offset(7, 4);
                    break;
                case 60:
                case 61:
                case 62:
                case 63:
                    p.Offset(4, 4);
                    break;
            }

            p.x *= image.scale.x;
            p.x *= image.scale.y;
            p.Offset(
                -image.origin.x * (image.scale.x - 1),
                -image.origin.y * (image.scale.y - 1));
            p.Offset(image.Point());

            var star = new Speck();
            star.Reset(0, p.x, p.y, Speck.DISCOVER);
            star.camera = image.GetCamera();
            image.parent.Add(star);
        }

        public static BadgeBanner Show(int image)
        {
            if (current != null)
                current.KillAndErase();

            return (current = new BadgeBanner(image));
        }

        public static Image Image(int index)
        {
            var image = new Image(Assets.Interfaces.BADGES);
            if (atlas == null)
                atlas = new TextureFilm(image.texture, 16, 16);

            image.Frame(atlas.Get(index));
            return image;
        }
    }
}