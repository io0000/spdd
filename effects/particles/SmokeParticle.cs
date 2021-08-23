using watabou.noosa.particles;
using watabou.utils;

namespace spdd.effects.particles
{
    public class SmokeParticle : PixelParticle
    {
        public static Emitter.Factory Factory = new SmokeParticleFactory1();
        public static Emitter.Factory Spew = new SmokeParticleFactory2();

        public class SmokeParticleFactory1 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<SmokeParticle>().Reset(x, y);
            }
        }

        public class SmokeParticleFactory2 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<SmokeParticle>().ResetSpew(x, y);
            }
        }

        public SmokeParticle()
        {
            SetColor(new Color(0x22, 0x22, 0x22, 0xFF));

            acc.Set(0, -40);
        }

        public void Reset(float x, float y)
        {
            Revive();

            this.x = x;
            this.y = y;

            left = lifespan = Rnd.Float(0.6f, 1f);
            speed.Set(Rnd.Float(-4, +4), Rnd.Float(-8, +8));
        }

        public void ResetSpew(float x, float y)
        {
            Revive();

            this.x = x;
            this.y = y;

            acc.Set(-40, 40);

            left = lifespan = Rnd.Float(0.6f, 1f);
            speed.Polar(Rnd.Float(PointF.PI * 1.7f, PointF.PI * 1.8f), Rnd.Float(30, 60));
        }

        public override void Update()
        {
            base.Update();

            float p = left / lifespan;
            am = p > 0.8f ? 2 - 2 * p : p * 0.5f;
            Size(16 - p * 8);
        }
    }
}