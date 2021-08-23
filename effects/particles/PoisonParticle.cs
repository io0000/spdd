using watabou.utils;
using watabou.noosa.particles;

namespace spdd.effects.particles
{
    public class PoisonParticle : PixelParticle
    {
        public static Emitter.Factory Factory = new PoisonParticleFactory1();

        public static Emitter.Factory Splash = new PoisonParticleFactory2();

        public class PoisonParticleFactory1 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<PoisonParticle>().ResetMissile(x, y);
            }

            public override bool LightMode()
            {
                return true;
            }
        }

        public class PoisonParticleFactory2 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<PoisonParticle>().ResetSplash(x, y);
            }

            public override bool LightMode()
            {
                return true;
            }
        }

        public PoisonParticle()
        {
            lifespan = 0.6f;

            acc.Set(0, +30);
        }

        public void ResetMissile(float x, float y)
        {
            Revive();

            this.x = x;
            this.y = y;

            left = lifespan;

            speed.Polar(-Rnd.Float(3.1415926f), Rnd.Float(6));
        }

        public void ResetSplash(float x, float y)
        {
            Revive();

            this.x = x;
            this.y = y;

            left = lifespan;

            speed.Polar(Rnd.Float(3.1415926f), Rnd.Float(10, 20));
        }

        public override void Update()
        {
            base.Update();
            // alpha: 1 -> 0; size: 1 -> 4
            Size(4 - (am = left / lifespan) * 3);

            // color: 0x8844FF -> 0x00FF00
            var color1 = new Color(0x00, 0xFF, 0x00, 0xFF);
            var color2 = new Color(0x88, 0x44, 0xFF, 0xFF);
            SetColor(ColorMath.Interpolate(color1, color2, am));
        }
    }
}