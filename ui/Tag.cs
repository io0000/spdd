using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;

namespace spdd.ui
{
    public class Tag : Button
    {
        private readonly float r;
        private readonly float g;
        private readonly float b;
        public NinePatch bg;

        public float lightness;

        public Tag(Color color)
        {
            r = color.R / 255f;
            g = color.G / 255f;
            b = color.B / 255f;
        }

        protected override void CreateChildren()
        {
            base.CreateChildren();

            bg = Chrome.Get(Chrome.Type.TAG);
            bg.Hardlight(r, g, b);
            Add(bg);
        }

        protected override void Layout()
        {
            base.Layout();

            bg.x = x;
            bg.y = y;
            bg.Size(width, height);
        }

        public void Flash()
        {
            lightness = 1f;
        }

        public void Flip(bool value)
        {
            bg.FlipHorizontal(value);
        }

        public override void Update()
        {
            base.Update();

            if (visible && lightness > 0.5)
            {
                if ((lightness -= Game.elapsed) > 0.5)
                {
                    bg.ra = bg.ga = bg.ba = 2 * lightness - 1;
                    bg.rm = 2 * r * (1 - lightness);
                    bg.gm = 2 * g * (1 - lightness);
                    bg.bm = 2 * b * (1 - lightness);
                }
                else
                {
                    bg.Hardlight(r, g, b);
                }
            }
        }
    }
}