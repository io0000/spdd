using watabou.utils;
using watabou.noosa.particles;

namespace spdd.effects.particles
{
    public class SnowParticle : PixelParticle
    {
        public static Emitter.Factory Factory = new SnowParticleFactory();

        public class SnowParticleFactory : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<SnowParticle>().Reset(x, y);
            }
        }

        public SnowParticle()
        {
            speed.Set(0, Rnd.Float(5, 8));
            lifespan = 1.2f;
        }

        public void Reset(float x, float y)
        {
            Revive();

            this.x = x;
            this.y = y - speed.y * lifespan;

            left = lifespan;
        }

        public override void Update()
        {
            base.Update();
            var p = left / lifespan;
            am = (p < 0.5f ? p : 1 - p) * 1.5f;
        }
    }
}