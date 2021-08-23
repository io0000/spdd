using watabou.utils;
using watabou.noosa.particles;

namespace spdd.effects.particles
{
    public class WebParticle : PixelParticle
    {
        public static Emitter.Factory Factory = new WebParticleFactory();

        public class WebParticleFactory : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                for (var i = 0; i < 3; ++i)
                    emitter.Recycle<WebParticle>().Reset(x, y);
            }
        }

        public WebParticle()
        {
            SetColor(new Color(0xCC, 0xCC, 0xCC, 0xFF));
            lifespan = 2f;
        }

        public void Reset(float x, float y)
        {
            Revive();

            this.x = x;
            this.y = y;

            left = lifespan;
            angle = Rnd.Float(360);
        }

        public override void Update()
        {
            base.Update();

            var p = left / lifespan;
            am = p < 0.5f ? p : 1 - p;
            scale.y = 12 + p * 6;
        }
    }
}