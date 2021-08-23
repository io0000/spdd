using watabou.utils;

namespace watabou.noosa.particles
{
    public class PixelParticle : PseudoPixel
    {
        protected float size;

        protected float lifespan;
        protected float left;

        public PixelParticle()
        {
            origin.Set(+0.5f);
        }

        public void Reset(float x, float y, Color color, float size, float lifespan)
        {
            Revive();

            this.x = x;
            this.y = y;

            SetColor(color);
            Size(this.size = size);

            this.left = this.lifespan = lifespan;
        }

        public override void Update()
        {
            base.Update();

            if ((left -= Game.elapsed) <= 0.0f)
                Kill();
        }

        public class Shrinking : PixelParticle
        {
            public override void Update()
            {
                base.Update();
                Size(size * left / lifespan);
            }
        }
    }
}