using watabou.noosa;
using watabou.glwrap;

namespace spdd.effects
{
    public class ShadowBox : NinePatch
    {
        public const float SIZE = 16;

        public ShadowBox()
            : base(Assets.Interfaces.SHADOW, 1)
        {
            //If this is the first time the texture is generated, set the filtering
            if (texture.id == -1)
                texture.Filter(Texture.LINEAR, Texture.LINEAR);

            scale.Set(SIZE, SIZE);
        }

        public override void Size(float width, float height)
        {
            base.Size(width / SIZE, height / SIZE);
        }

        public void BoxRect(float x, float y, float width, float height)
        {
            this.x = x - SIZE;
            this.y = y - SIZE;
            Size(width + SIZE * 2, height + SIZE * 2);
        }
    }
}