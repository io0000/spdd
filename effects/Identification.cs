using System;
using watabou.noosa;
using watabou.noosa.particles;
using watabou.utils;
using watabou.glwrap;

namespace spdd.effects
{
    public class Identification : Group
    {
        private static readonly int[] DOTS = {
            -1, -3,
             0, -3,
            +1, -3,
            -1, -2,
            +1, -2,
            +1, -1,
             0,  0,
            +1,  0,
             0, +1,
             0, +3
        };

        public Identification(PointF p)
        {
            for (var i = 0; i < DOTS.Length; i += 2)
            {
                Add(new Speck(p.x, p.y, DOTS[i], DOTS[i + 1]));
                Add(new Speck(p.x, p.y, DOTS[i], DOTS[i + 1]));
            }
        }

        public override void Update()
        {
            base.Update();
            if (CountLiving() == 0)
                KillAndErase();
        }

        public override void Draw()
        {
            Blending.SetLightMode();
            base.Draw();
            Blending.SetNormalMode();
        }

        public class Speck : PixelParticle
        {
            private static Color COLOR = new Color(0x44, 0x88, 0xCC, 0xFF);
            private const int SIZE = 3;

            public Speck(float x0, float y0, int mx, int my)
            {
                SetColor(COLOR);

                var x1 = x0 + mx * SIZE;
                var y1 = y0 + my * SIZE;

                var p = new PointF().Polar(Rnd.Float(2 * PointF.PI), 8);
                x0 += p.x;
                y0 += p.y;

                var dx = x1 - x0;
                var dy = y1 - y0;

                x = x0;
                y = y0;
                speed.Set(dx, dy);
                acc.Set(-dx / 4, -dy / 4);

                left = lifespan = 2f;
            }

            public override void Update()
            {
                base.Update();

                am = 1 - Math.Abs(left / lifespan - 0.5f) * 2;
                am *= am;
                Size(am * SIZE);
            }
        }
    }
}