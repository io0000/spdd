using System;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using watabou.glwrap;

namespace spdd.effects
{
    public class Beam : Image
    {
        private const double A = 180 / Math.PI;

        private float duration;

        private float timeLeft;

        private Beam(PointF s, PointF e, Effects.Type asset, float duration)
            : base(Effects.Get(asset))
        {
            origin.Set(0, height / 2);

            x = s.x - origin.x;
            y = s.y - origin.y;

            float dx = e.x - s.x;
            float dy = e.y - s.y;
            angle = (float)(Math.Atan2(dy, dx) * A);
            scale.x = (float)Math.Sqrt(dx * dx + dy * dy) / width;

            Sample.Instance.Play(Assets.Sounds.RAY);

            timeLeft = this.duration = duration;
        }

        public class DeathRay : Beam
        {
            public DeathRay(PointF s, PointF e)
                : base(s, e, Effects.Type.DEATH_RAY, 0.5f)
            { }
        }

        public class LightRay : Beam
        {
            public LightRay(PointF s, PointF e)
                : base(s, e, Effects.Type.LIGHT_RAY, 1f)
            { }
        }

        public class HealthRay : Beam
        {
            public HealthRay(PointF s, PointF e)
                : base(s, e, Effects.Type.HEALTH_RAY, 0.75f)
            { }
        }

        public override void Update()
        {
            base.Update();

            float p = timeLeft / duration;
            Alpha(p);
            scale.Set(scale.x, p);

            if ((timeLeft -= Game.elapsed) <= 0)
            {
                KillAndErase();
            }
        }

        public override void Draw()
        {
            Blending.SetLightMode();
            base.Draw();
            Blending.SetNormalMode();
        }
    }
}