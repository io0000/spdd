using watabou.gltextures;
using watabou.glwrap;
using watabou.utils;

namespace watabou.noosa
{
    public class Image : Visual
    {
        public Texture texture;
        public RectF frame;

        public bool flipHorizontal;
        public bool flipVertical;

        public float[] vertices;
        //protected FloatBuffer verticesBuffer;
        //protected Vertexbuffer buffer;

        //protected bool dirty;

        public Image()
            : base(0.0f, 0.0f, 0.0f, 0.0f)
        {
            vertices = new float[16];
            //verticesBuffer = Quad.create();
        }

        public Image(Image src)
            : this()
        {
            Copy(src);
        }

        public Image(object tx)
            : this()
        {
            Texture(tx);
        }

        public Image(object tx, int left, int top, int width, int height)
            : this(tx)
        {
            Frame(texture.UvRect(left, top, left + width, top + height));
        }

        public void Texture(object tx)
        {
            texture = tx is Texture ? (Texture)tx : TextureCache.Get(tx);

            Frame(new RectF(0.0f, 0.0f, 1.0f, 1.0f));
        }

        public virtual void Frame(RectF frame)
        {
            this.frame = frame;

            width = frame.Width() * texture.width;
            height = frame.Height() * texture.height;

            UpdateFrame();
            UpdateVertices();
        }

        public void Frame(int left, int top, int width, int height)
        {
            Frame(texture.UvRect(left, top, left + width, top + height));
        }

        public RectF Frame()
        {
            return new RectF(frame);
        }

        public void Copy(Image other)
        {
            texture = other.texture;
            frame = new RectF(other.frame);

            width = other.width;
            height = other.height;

            scale = other.scale;

            UpdateFrame();
            UpdateVertices();
        }

        protected virtual void UpdateFrame()
        {
            if (flipHorizontal)
            {
                vertices[2] = frame.right;
                vertices[6] = frame.left;
                vertices[10] = frame.left;
                vertices[14] = frame.right;
            }
            else
            {
                vertices[2] = frame.left;
                vertices[6] = frame.right;
                vertices[10] = frame.right;
                vertices[14] = frame.left;
            }

            if (flipVertical)
            {
                vertices[3] = frame.bottom;
                vertices[7] = frame.bottom;
                vertices[11] = frame.top;
                vertices[15] = frame.top;
            }
            else
            {
                vertices[3] = frame.top;
                vertices[7] = frame.top;
                vertices[11] = frame.bottom;
                vertices[15] = frame.bottom;
            }

            //dirty = true;
        }

        protected void UpdateVertices()
        {
            vertices[0] = 0;
            vertices[1] = 0;

            vertices[4] = width;
            vertices[5] = 0;

            vertices[8] = width;
            vertices[9] = height;

            vertices[12] = 0;
            vertices[13] = height;

            //dirty = true;
        }

        public override void Draw()
        {
            if (texture == null) // || (!dirty && buffer == null))
                return;

            base.Draw();

            //if (dirty)
            //{
            //    verticesBuffer.position(0);
            //    verticesBuffer.put(vertices);
            //    if (buffer == null)
            //        buffer = new Vertexbuffer(verticesBuffer);
            //    else
            //        buffer.updateVertices(verticesBuffer);
            //    dirty = false;
            //}
            var script = Script();

            texture.Bind();

            script.Camera(GetCamera());

            script.uModel.ValueM4(matrix);
            script.Lighting(
                rm, gm, bm, am,
                ra, ga, ba, aa);

            script.DrawQuad(vertices);
        }

        protected virtual NoosaScript Script()
        {
            return NoosaScript.Get();
        }

        public override void Destroy()
        {
            base.Destroy();
            //if (buffer != null)
            //    buffer.delete();
        }
    }
}