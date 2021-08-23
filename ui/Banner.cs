using watabou.utils;
using watabou.noosa;

namespace spdd.ui
{
    public class Banner : Image
    {
        private enum State
        {
            FADE_IN,
            STATIC,
            FADE_OUT
        }
        private State state;

        private float time;

        private Color color;
        private float fadeTime;
        private float showTime;

        public Banner(Image sample)
        {
            Copy(sample);
            Alpha(0);
        }

        public Banner(object tx)
            : base(tx)
        {
            Alpha(0);
        }

        public void Show(Color color, float fadeTime, float showTime)
        {
            this.color = color;
            this.fadeTime = fadeTime;
            this.showTime = showTime;

            state = State.FADE_IN;

            time = fadeTime;
        }

        public void Show(Color color, float fadeTime)
        {
            Show(color, fadeTime, float.MaxValue);
        }

        public override void Update()
        {
            base.Update();

            time -= Game.elapsed;
            if (time >= 0)
            {
                float p = time / fadeTime;

                switch (state)
                {
                    case State.FADE_IN:
                        Tint(color, p);
                        Alpha(1 - p);
                        break;
                    case State.STATIC:
                        break;
                    case State.FADE_OUT:
                        ResetColor();
                        Alpha(p);
                        break;
                }
            }
            else
            {
                switch (state)
                {
                    case State.FADE_IN:
                        time = showTime;
                        state = State.STATIC;
                        break;
                    case State.STATIC:
                        time = fadeTime;
                        state = State.FADE_OUT;
                        break;
                    case State.FADE_OUT:
                        KillAndErase();
                        break;
                }
            }
        }
    }
}