using watabou.utils;
using watabou.noosa.particles;

namespace spdd.effects.particles
{
    public class ShaftParticle : PixelParticle
    {
        public static Emitter.Factory Factory = new ShaftParticleFactory();

        public class ShaftParticleFactory : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<ShaftParticle>().Reset(x, y);
            }

            public override bool LightMode()
            {
                return true;
            }
        }

        public ShaftParticle()
        {
            lifespan = 1.2f;
            speed.Set(0, -6);
        }

        private float offs;

        public void Reset(float x, float y)
        {
            Revive();

            base.x = x;
            base.y = y;

            offs = -Rnd.Float(lifespan);
            left = lifespan - offs;
        }

        public override void Update()
        {
            base.Update();

            var p = left / lifespan;
            am = p < 0.5f ? p : 1 - p;
            scale.x = (1 - p) * 4;
            scale.y = 16 + (1 - p) * 16;
        }
    }
}