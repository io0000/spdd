using watabou.noosa.particles;
using watabou.utils;

namespace spdd.effects.particles
{
    public class CorrosionParticle : PixelParticle.Shrinking
    {
        public static Emitter.Factory Missile = new CorrosionParticleFactory1();
        public static Emitter.Factory Splash = new CorrosionParticleFactory2();

        public class CorrosionParticleFactory1 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<CorrosionParticle>().ResetMissile(x, y);
            }

            public override bool LightMode()
            {
                return false;
            }
        }

        public class CorrosionParticleFactory2 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<CorrosionParticle>().ResetSplash(x, y);
            }

            public override bool LightMode()
            {
                return false;
            }
        }

        public CorrosionParticle()
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

            // color: 0xAAAAAA -> 0xFF8800
            var c1 = new Color(0xFF, 0x88, 0x00, 0xFF); //0xFF8800
            var c2 = new Color(0xAA, 0xAA, 0xAA, 0xFF); //0xAAAAAA

            SetColor(ColorMath.Interpolate(c1, c2, am));
        }
    }
}