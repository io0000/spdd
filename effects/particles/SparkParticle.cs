using watabou.noosa.particles;
using watabou.utils;

namespace spdd.effects.particles
{
    public class SparkParticle : PixelParticle
    {
        public static Emitter.Factory Factory = new SparkParticleFactory1();
        public static Emitter.Factory Static = new SparkParticleFactory2();

        public class SparkParticleFactory1 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<SparkParticle>().Reset(x, y);
            }

            public override bool LightMode()
            {
                return true;
            }
        }

        public class SparkParticleFactory2 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<SparkParticle>().ResetStatic(x, y);
            }

            public override bool LightMode()
            {
                return true;
            }
        }

        public SparkParticle()
        {
            Size(2);

            acc.Set(0, +50);
        }

        public void Reset(float x, float y)
        {
            Revive();

            this.x = x;
            this.y = y;

            left = lifespan = Rnd.Float(0.5f, 1.0f);

            speed.Polar(Rnd.Float(3.1415926f), Rnd.Float(20, 40));
        }

        public void ResetStatic(float x, float y)
        {
            Reset(x, y);

            left = lifespan = Rnd.Float(0.25f, 0.5f);

            acc.Set(0, 0);
            speed.Set(0, 0);
        }

        //public void setMaxSize(float value)
        //{
        //    size = value;
        //}

        public override void Update()
        {
            base.Update();
            Size(Rnd.Float(5 * left / lifespan));
        }
    }
}