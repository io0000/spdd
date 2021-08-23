using watabou.gltextures;
using watabou.glwrap;
using watabou.utils;

namespace watabou.noosa
{
    public class Halo : Image
    {
        private static readonly object CACHE_KEY = typeof(Halo);

        public const int RADIUS = 128;

        public float radius = RADIUS;
        public float brightness = 1;

        public Halo()
        {
            if (!TextureCache.Contains(CACHE_KEY))
            {
                int w = RADIUS * 2 + 1;
                int h = RADIUS * 2 + 1;
                var pixmap = new watabou.glwrap.Pixmap(w, h);
                pixmap.SetColor(new Color(0xff, 0xff, 0xff, 0x08));
                for (int i = 0; i < RADIUS; i += 2)
                    pixmap.FillCircle(RADIUS, RADIUS, (RADIUS - i));

                TextureCache.Add(CACHE_KEY, new Texture(pixmap));
            }

            Texture(CACHE_KEY);
        }

        public Halo(float radius, Color color, float brightness)
            : this()
        {
            Hardlight(color);
            Alpha(this.brightness = brightness);
            Radius(radius);
        }

        public Halo Point(float x, float y)
        {
            this.x = x - (Width() / 2f);
            this.y = y - (Height() / 2f);
            return this;
        }

        public void Radius(float value)
        {
            scale.Set((this.radius = value) / RADIUS);
        }
    }
}