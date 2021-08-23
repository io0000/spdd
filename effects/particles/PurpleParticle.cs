using watabou.noosa.particles;
using watabou.utils;

namespace spdd.effects.particles
{
    public class PurpleParticle : PixelParticle
    {
        //public static Emitter.Factory Missile = new PurpleParticleFactory1();
        public static Emitter.Factory Burst = new PurpleParticleFactory2();

        //public class PurpleParticleFactory1 : Emitter.Factory
        //{
        //    public override void Emit(Emitter emitter, int index, float x, float y)
        //    {
        //        emitter.Recycle<PurpleParticle>().Reset(x, y);
        //    }
        //}

        public class PurpleParticleFactory2 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<PurpleParticle>().ResetBurst(x, y);
            }

            public override bool LightMode()
            {
                return true;
            }
        }

        public PurpleParticle()
        {
            lifespan = 0.5f;
        }

        public void Reset(float x, float y)
        {
            Revive();

            this.x = x;
            this.y = y;

            speed.Set(Rnd.Float(-5, +5), Rnd.Float(-5, +5));

            left = lifespan;
        }

        public void ResetBurst(float x, float y)
        {
            Revive();

            this.x = x;
            this.y = y;

            speed.Polar(Rnd.Float(PointF.PI2), Rnd.Float(16, 32));

            left = lifespan;
        }

        public override void Update()
        {
            base.Update();

            // alpha: 1 -> 0; size: 1 -> 5
            Size(5 - (am = left / lifespan) * 4);
            // color: 0xFF0044 -> 0x220066
            var color1 = new Color(0x22, 0x00, 0x66, 0xFF);
            var color2 = new Color(0xFF, 0x00, 0x44, 0xFF);
            SetColor(ColorMath.Interpolate(color1, color2, am));
        }
    }
}