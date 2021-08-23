using watabou.utils;

namespace watabou.noosa
{
    public class SkinnedBlock : Image
    {
        protected float scaleX;
        protected float scaleY;

        protected float offsetX;
        protected float offsetY;

        public bool autoAdjust;

        public SkinnedBlock(float width, float height, object tx)
            : base(tx)
        {
            texture.Wrap(glwrap.Texture.REPEAT, glwrap.Texture.REPEAT);

            Size(width, height);
        }

        public override void Frame(RectF frame)
        {
            scaleX = 1.0f;
            scaleY = 1.0f;

            offsetX = 0.0f;
            offsetY = 0.0f;

            base.Frame(new RectF(0.0f, 0.0f, 1.0f, 1.0f));
        }

        protected override void UpdateFrame()
        {
            if (autoAdjust)
            {
                while (offsetX > texture.width)
                    offsetX -= texture.width;

                while (offsetX < -texture.width)
                    offsetX += texture.width;

                while (offsetY > texture.height)
                    offsetY -= texture.height;

                while (offsetY < -texture.height)
                    offsetY += texture.height;
            }

            float tw = 1.0f / texture.width;
            float th = 1.0f / texture.height;

            float u0 = offsetX * tw;
            float v0 = offsetY * th;
            float u1 = u0 + width * tw / scaleX;
            float v1 = v0 + height * th / scaleY;

            vertices[2] = u0;
            vertices[3] = v0;

            vertices[6] = u1;
            vertices[7] = v0;

            vertices[10] = u1;
            vertices[11] = v1;

            vertices[14] = u0;
            vertices[15] = v1;

            //dirty = true;
        }

        public void OffsetTo(float x, float y)
        {
            offsetX = x;
            offsetY = y;
            UpdateFrame();
        }

        public void Offset(float x, float y)
        {
            offsetX += x;
            offsetY += y;
            UpdateFrame();
        }

        public float OffsetX()
        {
            return offsetX;
        }

        public float OffsetY()
        {
            return offsetY;
        }

        public void Scale(float x, float y)
        {
            scaleX = x;
            scaleY = y;
            UpdateFrame();
        }

        public void Size(float w, float h)
        {
            this.width = w;
            this.height = h;
            UpdateFrame();
            UpdateVertices();
        }
    }
}