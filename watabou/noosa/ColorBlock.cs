using watabou.gltextures;
using watabou.utils;

namespace watabou.noosa
{
    public class ColorBlock : Image, IResizable
    {
        public ColorBlock(float width, float height, Color color)
            : base(TextureCache.CreateSolid(color))
        {
            scale.Set(width, height);
            origin.Set(0, 0);
        }

        // Interface Resizable
        public void Size(float width, float height)
        {
            scale.Set(width, height);
        }

        public override float Width()
        {
            return scale.x;
        }

        public override float Height()
        {
            return scale.y;
        }
    }
}