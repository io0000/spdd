using watabou.utils;
using watabou.noosa.particles;

namespace spdd.effects.particles
{
    public class BloodParticle : PixelParticle.Shrinking
    {
        public static Emitter.Factory Factory = new BloodParticleFactory1();
        public static Emitter.Factory Burst = new BloodParticleFactory2();

        private class BloodParticleFactory1 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<BloodParticle>().Reset(x, y);
            }
        }

        private class BloodParticleFactory2 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<BloodParticle>().ResetBurst(x, y);
            }

            public override bool LightMode()
            {
                return true;
            }
        }

        public BloodParticle()
        {
            SetColor(new Color(0xCC, 0x00, 0x00, 0xFF));
            lifespan = 0.8f;

            acc.Set(0, +40);
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

        public void ResetBurst(float x, float y)
        {
            Revive();

            this.x = x;
            this.y = y;

            speed.Polar(Rnd.Float(PointF.PI2), Rnd.Float(16, 32));
            size = 5;

            left = 0.5f;
        }

        public override void Update()
        {
            base.Update();
            var p = left / lifespan;
            am = p > 0.6f ? (1 - p) * 2.5f : 1;
        }
    }
}