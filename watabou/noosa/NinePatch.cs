using watabou.gltextures;
using watabou.glwrap;
using watabou.utils;

namespace watabou.noosa
{
    public class NinePatch : Visual
    {
        public Texture texture;

        //protected float[] vertices;
        //protected FloatBuffer quads;
        //protected Vertexbuffer buffer;
        protected float[] verticesBuffer;

        protected RectF outterF;
        protected RectF innerF;

        protected int marginLeft;
        protected int marginRight;
        protected int marginTop;
        protected int marginBottom;

        protected float nWidth;
        protected float nHeight;

        protected bool flipHorizontal;
        protected bool flipVertical;

        protected bool dirty;

        public NinePatch(object tx, int margin)
            : this(tx, margin, margin, margin, margin)
        { }

        public NinePatch(object tx, int left, int top, int right, int bottom)
            : this(tx, 0, 0, 0, 0, left, top, right, bottom)
        { }

        public NinePatch(object tx, int x, int y, int w, int h, int margin)
            : this(tx, x, y, w, h, margin, margin, margin, margin)
        { }

        public NinePatch(object tx, int x, int y, int w, int h, int left, int top, int right, int bottom)
            : base(0, 0, 0, 0)
        {
            texture = TextureCache.Get(tx);
            w = w == 0 ? texture.width : w;
            h = h == 0 ? texture.height : h;

            nWidth = width = w;
            nHeight = height = h;

            //vertices = new float[16];
            // ByteBuffer.AllocateDirect(size * ByteCount).Order(ByteOrder.NativeOrder()).AsFloatBuffer();
            //verticesBuffer = Quad.CreateSet(9);
            verticesBuffer = new float[9 * 16];

            marginLeft = left;
            marginRight = right;
            marginTop = top;
            marginBottom = bottom;

            outterF = texture.UvRect(x, y, x + w, y + h);
            innerF = texture.UvRect(x + left, y + top, x + w - right, y + h - bottom);

            UpdateVertices();
        }

        protected void UpdateVertices()
        {
            //quads.position( 0 );

            var right = width - marginRight;
            var bottom = height - marginBottom;

            float outleft = flipHorizontal ? outterF.right : outterF.left;
            float outright = flipHorizontal ? outterF.left : outterF.right;
            float outtop = flipVertical ? outterF.bottom : outterF.top;
            float outbottom = flipVertical ? outterF.top : outterF.bottom;

            float inleft = flipHorizontal ? innerF.right : innerF.left;
            float inright = flipHorizontal ? innerF.left : innerF.right;
            float intop = flipVertical ? innerF.bottom : innerF.top;
            float inbottom = flipVertical ? innerF.top : innerF.bottom;

            int index = 0;
            Quad.Fill(verticesBuffer, index,
                0, marginLeft, 0, marginTop, outleft, inleft, outtop, intop);
            index += 16;

            Quad.Fill(verticesBuffer, index,
                marginLeft, right, 0, marginTop, inleft, inright, outtop, intop);
            index += 16;

            Quad.Fill(verticesBuffer, index,
                right, width, 0, marginTop, inright, outright, outtop, intop);
            index += 16;

            Quad.Fill(verticesBuffer, index,
                0, marginLeft, marginTop, bottom, outleft, inleft, intop, inbottom);
            index += 16;

            Quad.Fill(verticesBuffer, index,
                marginLeft, right, marginTop, bottom, inleft, inright, intop, inbottom);
            index += 16;

            Quad.Fill(verticesBuffer, index,
                right, width, marginTop, bottom, inright, outright, intop, inbottom);
            index += 16;

            Quad.Fill(verticesBuffer, index,
                0, marginLeft, bottom, height, outleft, inleft, inbottom, outbottom);
            index += 16;

            Quad.Fill(verticesBuffer, index,
                marginLeft, right, bottom, height, inleft, inright, inbottom, outbottom);
            index += 16;

            Quad.Fill(verticesBuffer, index,
                right, width, bottom, height, inright, outright, inbottom, outbottom);
            index += 16;

            dirty = true;
        }

        public int MarginLeft()
        {
            return marginLeft;
        }

        public int MarginRight()
        {
            return marginRight;
        }

        public int MarginTop()
        {
            return marginTop;
        }

        public int MarginBottom()
        {
            return marginBottom;
        }

        public int MarginHor()
        {
            return marginLeft + marginRight;
        }

        public int MarginVer()
        {
            return marginTop + marginBottom;
        }

        //public float InnerWidth()
        //{
        //    return width - marginLeft - marginRight;
        //}
        //
        //public float InnerHeight()
        //{
        //    return height - marginTop - marginBottom;
        //}
        //
        //public float InnerRight()
        //{
        //    return width - marginRight;
        //}
        //
        //public float InnerBottom()
        //{
        //    return height - marginBottom;
        //}

        public void FlipHorizontal(bool value)
        {
            flipHorizontal = value;
            UpdateVertices();
        }

        public void FlipVertical(bool value)
        {
            flipVertical = value;
            UpdateVertices();
        }

        public virtual void Size(float width, float height)
        {
            this.width = width;
            this.height = height;
            UpdateVertices();
        }

        public override void Draw()
        {
            base.Draw();

            if (dirty)
            {
                //if (buffer == null)
                //    buffer = new Vertexbuffer(quads);
                //else
                //    buffer.updateVertices(quads);
                dirty = false;
            }

            NoosaScript script = NoosaScript.Get();

            texture.Bind();

            script.Camera(GetCamera());

            script.uModel.ValueM4(matrix);
            script.Lighting(
                rm, gm, bm, am,
                ra, ga, ba, aa);

            script.DrawQuadSet(verticesBuffer, 9);
        }

        public override void Destroy()
        {
            base.Destroy();
            //if (buffer != null)
            //    buffer.delete();
        }
    }
}