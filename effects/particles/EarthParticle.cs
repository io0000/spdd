using watabou.noosa.particles;
using watabou.utils;

namespace spdd.effects.particles
{
    public class EarthParticle : PixelParticle
    {
        public static Emitter.Factory Factory = new EarthParticleFactory1();
        public static Emitter.Factory Falling = new EarthParticleFactory2();

        public class EarthParticleFactory1 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<EarthParticle>().Reset(x, y);
            }
        }

        public class EarthParticleFactory2 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<EarthParticle>().ResetFalling(x, y);
            }
        }

        public EarthParticle()
        {
            var color1 = new Color(0x44, 0x44, 0x44, 0xFF);
            var color2 = new Color(0x77, 0x77, 0x66, 0xFF);
            SetColor(ColorMath.Random(color1, color2));
            angle = Rnd.Float(-30, 30);
        }

        public void Reset(float x, float y)
        {
            Revive();

            this.x = x;
            this.y = y;

            left = lifespan = 0.5f;
            size = 16;
        }

        public void ResetFalling(float x, float y)
        {
            Reset(x, y);

            left = lifespan = 1f;
            size = 8;

            acc.y = 30;
            speed.y = -5;
            angularSpeed = Rnd.Float(-90, 90);
        }

        public override void Update()
        {
            base.Update();

            var p = left / lifespan;
            Size((p < 0.5f ? p : 1 - p) * size);
        }
    }
}