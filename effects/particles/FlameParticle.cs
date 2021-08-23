using watabou.utils;
using watabou.noosa.particles;

namespace spdd.effects.particles
{
    public class FlameParticle : PixelParticle.Shrinking
    {
        public static Emitter.Factory Factory = new FlameParticleFactory();

        public class FlameParticleFactory : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<FlameParticle>().Reset(x, y);
            }

            public override bool LightMode()
            {
                return true;
            }
        }

        public FlameParticle()
        {
            SetColor(new Color(0xEE, 0x77, 0x22, 0xFF));
            lifespan = 0.6f;

            acc.Set(0, -80);
        }

        public void Reset(float x, float y)
        {
            Revive();

            this.x = x;
            this.y = y;

            left = lifespan;

            size = 4;
            speed.Set(0);
        }

        public override void Update()
        {
            base.Update();
            var p = left / lifespan;
            am = p > 0.8f ? (1 - p) * 5 : 1;
        }
    }
}