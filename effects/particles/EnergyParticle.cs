using watabou.utils;
using watabou.noosa.particles;

namespace spdd.effects.particles
{
    public class EnergyParticle : PixelParticle
    {
        public static Emitter.Factory Factory = new EnergyParticleFactory();

        public class EnergyParticleFactory : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<EnergyParticle>().Reset(x, y);
            }

            public override bool LightMode()
            {
                return true;
            }
        }

        public EnergyParticle()
        {
            lifespan = 1f;
            SetColor(new Color(0xFF, 0xFF, 0xAA, 0xFF));

            speed.Polar(Rnd.Float(PointF.PI2), Rnd.Float(24, 32));
        }

        public void Reset(float x, float y)
        {
            Revive();

            left = lifespan;

            this.x = x - speed.x * lifespan;
            this.y = y - speed.y * lifespan;
        }

        public override void Update()
        {
            base.Update();

            var p = left / lifespan;
            am = p < 0.5f ? p * p * 4 : (1 - p) * 2;
            Size(Rnd.Float(5 * left / lifespan));
        }
    }
}