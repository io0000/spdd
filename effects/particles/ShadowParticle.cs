using watabou.utils;
using watabou.noosa.particles;

namespace spdd.effects.particles
{
    public class ShadowParticle : PixelParticle.Shrinking
    {
        public static Emitter.Factory Missile = new ShadowParticleFactory1();

        public static Emitter.Factory Curse = new ShadowParticleFactory2();

        public static Emitter.Factory Up = new ShadowParticleFactory3();

        public class ShadowParticleFactory1 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<ShadowParticle>().Reset(x, y);
            }
        }

        public class ShadowParticleFactory2 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<ShadowParticle>().ResetCurse(x, y);
            }
        }

        public class ShadowParticleFactory3 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<ShadowParticle>().ResetUp(x, y);
            }
        }

        public void Reset(float x, float y)
        {
            Revive();

            this.x = x;
            this.y = y;

            speed.Set(Rnd.Float(-5, +5), Rnd.Float(-5, +5));

            size = 6;
            left = lifespan = 0.5f;
        }

        public void ResetCurse(float x, float y)
        {
            Revive();

            size = 8;
            left = lifespan = 0.5f;

            speed.Polar(Rnd.Float(PointF.PI2), Rnd.Float(16, 32));
            this.x = x - speed.x * lifespan;
            this.y = y - speed.y * lifespan;
        }

        public void ResetUp(float x, float y)
        {
            Revive();

            speed.Set(Rnd.Float(-8, +8), Rnd.Float(-32, -48));
            this.x = x;
            this.y = y;

            size = 6;
            left = lifespan = 1f;
        }

        public override void Update()
        {
            base.Update();

            var p = left / lifespan;
            // alpha: 0 -> 1 -> 0; size: 6 -> 0; color: 0x000000 -> 0x440044
            var color1 = new Color(0x00, 0x00, 0x00, 0xFF);
            var color2 = new Color(0x44, 0x00, 0x44, 0xFF);
            SetColor(ColorMath.Interpolate(color1, color2, p));
            am = p < 0.5f ? p * p * 4 : (1 - p) * 2;
        }
    }
}