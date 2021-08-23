using watabou.gltextures;
using watabou.utils;

namespace watabou.noosa
{
    public class PseudoPixel : Image
    {
        public PseudoPixel()
            : base(TextureCache.CreateSolid(new Color(0xff, 0xff, 0xff, 0xff)))
        { }

        //public PseudoPixel(float x, float y, int color)
        //{
        //
        //    this();
        //
        //    this.x = x;
        //    this.y = y;
        //    color(color);
        //}

        public void Size(float w, float h)
        {
            scale.Set(w, h);
        }

        public void Size(float value)
        {
            scale.Set(value);
        }
    }
}