using watabou.utils;
using watabou.noosa.particles;

namespace spdd.effects.particles
{
    public class BlastParticle : PixelParticle.Shrinking
    {
        public static Emitter.Factory Factory = new BlastParticleFactory();

        private class BlastParticleFactory : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<BlastParticle>().Reset(x, y);
            }

            public override bool LightMode()
            {
                return true;
            }
        }

        public BlastParticle()
        {
            SetColor(new Color(0xEE, 0x77, 0x22, 0xFF));
            acc.Set(0, +50);
        }

        public void Reset(float x, float y)
        {
            Revive();

            this.x = x;
            this.y = y;

            left = lifespan = Rnd.Float();

            size = 8;
            speed.Polar(-Rnd.Float(3.1415926f), Rnd.Float(32, 64));
        }

        public override void Update()
        {
            base.Update();
            am = left > 0.8f ? (1 - left) * 5 : 1;
        }
    }
}