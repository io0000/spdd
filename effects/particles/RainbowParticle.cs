using watabou.utils;
using watabou.noosa.particles;

namespace spdd.effects.particles
{
    public class RainbowParticle : PixelParticle
    {
        public static Emitter.Factory Burst = new RainbowPaticleFactory();

        public class RainbowPaticleFactory : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<RainbowParticle>().ResetBurst(x, y);
            }

            public override bool LightMode()
            {
                return true;
            }
        }

        public RainbowParticle()
        {
            // color( Random.Int( 0x1000000 ) );
            byte r = (byte)Rnd.Int(0xFF);
            byte g = (byte)Rnd.Int(0xFF);
            byte b = (byte)Rnd.Int(0xFF);
            var c = new Color(r, g, b, 0xFF);
            SetColor(c);

            lifespan = 0.5f;
        }

        //public void Reset(float x, float y)
        //{
        //    Revive();
        //
        //    this.x = x;
        //    this.y = y;
        //
        //    Speed.Set(Random.Float(-5, +5), Random.Float(-5, +5));
        //
        //    left = lifespan;
        //}

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
        }
    }
}