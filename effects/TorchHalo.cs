using watabou.glwrap;
using watabou.noosa;
using watabou.utils;
using spdd.sprites;

namespace spdd.effects
{
    public class TorchHalo : Halo
    {
        private readonly CharSprite target;

        private float phase;

        public TorchHalo(CharSprite sprite)
            : base(20, new Color(0xFF, 0xDD, 0xCC, 0xFF), 0.2f)
        {
            target = sprite;
            am = 0;
        }

        public override void Update()
        {
            base.Update();

            if (phase < 0)
            {
                if ((phase += Game.elapsed) >= 0)
                {
                    KillAndErase();
                }
                else
                {
                    scale.Set((2 + phase) * radius / RADIUS);
                    am = -phase * brightness;
                }
            }
            else if (phase < 1)
            {
                if ((phase += Game.elapsed) >= 1)
                    phase = 1;

                scale.Set(phase * radius / RADIUS);
                am = phase * brightness;
            }

            Point(target.x + target.width / 2, target.y + target.height / 2);
        }

        public override void Draw()
        {
            Blending.SetLightMode();
            base.Draw();
            Blending.SetNormalMode();
        }

        public void PutOut()
        {
            phase = -1;
        }
    }
}