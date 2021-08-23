using System;
using watabou.noosa;
using watabou.utils;
using watabou.glwrap;
using spdd.sprites;

namespace spdd.effects
{
    public class ShieldHalo : Halo
    {
        private CharSprite target;
        private float phase;

        public ShieldHalo(CharSprite sprite)
            : base((float)Math.Sqrt(Math.Pow(sprite.Width() / 2f, 2) + Math.Pow(sprite.Height() / 2f, 2)),
                  new Color(0xBB, 0xAA, 0xCC, 0xFF),
                  1.0f)
        {
            am = -0.33f;
            aa = +0.33f;

            target = sprite;

            phase = 1.0f;
        }

        public override void Update()
        {
            base.Update();

            if (phase < 1.0f)
            {
                if ((phase -= Game.elapsed) <= 0.0f)
                {
                    KillAndErase();
                }
                else
                {
                    scale.Set((2.0f - phase) * radius / RADIUS);
                    am = phase * (-1.0f);
                    aa = phase * (+1.0f);
                }
            }

            visible = target.visible;
            if (visible)
            {
                PointF p = target.Center();
                Point(p.x, p.y);
            }
        }

        public override void Draw()
        {
            Blending.SetLightMode();
            base.Draw();
            Blending.SetNormalMode();
        }

        public void PutOut()
        {
            phase = 0.999f;
        }
    }
}