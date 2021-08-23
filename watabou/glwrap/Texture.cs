using OpenTK.Graphics.OpenGL4;
using watabou.utils;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace watabou.glwrap
{
    public class Texture
    {
        //libgdx
        //public static final int GL_NEAREST = 0x2600; 9728
        //public static final int GL_LINEAR = 0x2601;  9729
        public const int NEAREST = (int)TextureMagFilter.Nearest;
        public const int LINEAR = (int)TextureMagFilter.Linear;

        public const int REPEAT = (int)TextureWrapMode.Repeat;
        public const int CLAMP = (int)TextureWrapMode.ClampToEdge;

        public int id = -1;
        private static int bound_id; //id of the currently bound texture

        public bool premultiplied;

        public int width;
        public int height;

        protected int fModeMin = NEAREST;
        protected int fModeMag = NEAREST;

        protected int wModeH = CLAMP;
        protected int wModeV = CLAMP;

        public Pixmap bitmap;
        //public Atlas atlas;

        public Texture()
        { }

        public Texture(Pixmap bitmap)
            : this(bitmap, NEAREST, CLAMP, false)
        { }

        public Texture(Pixmap bitmap, int filtering, int wrapping, bool premultiplied)
        {
            this.bitmap = bitmap;
            width = bitmap.GetWidth();
            height = bitmap.GetHeight();
            fModeMin = fModeMag = filtering;
            wModeH = wModeV = wrapping;
            this.premultiplied = premultiplied;
        }

        protected void Generate()
        {
            //id = Gdx.gl.glGenTexture();
            id = GL.GenTexture();

            if (bitmap != null)
                Bitmap(bitmap);

            Filter(fModeMin, fModeMag);
            Wrap(wModeH, wModeV);
        }

        public static void Activate(int index)
        {
            //Gdx.gl.glActiveTexture( Gdx.gl.GL_TEXTURE0 + index );
            GL.ActiveTexture(TextureUnit.Texture0 + index);
        }

        public void Bind()
        {
            if (id == -1)
                Generate();

            if (id != bound_id)
            {
                //Gdx.gl.glBindTexture(Gdx.gl.GL_TEXTURE_2D, id);
                GL.BindTexture(TextureTarget.Texture2D, id);
                bound_id = id;
            }
        }

        public static void Clear()
        {
            bound_id = 0;
        }

        public void Filter(int minMode, int magMode)
        {
            fModeMin = minMode;
            fModeMag = magMode;

            if (id == -1)
                return;

            Bind();

            GL.TextureParameter(id, TextureParameterName.TextureMinFilter, minMode);
            GL.TextureParameter(id, TextureParameterName.TextureMagFilter, magMode);
        }

        public void Wrap(int s, int t)
        {
            wModeH = s;
            wModeV = t;

            if (id == -1)
                return;

            Bind();

            GL.TextureParameter(id, TextureParameterName.TextureWrapS, s);
            GL.TextureParameter(id, TextureParameterName.TextureWrapT, t);
        }

        public void Delete()
        {
            if (bound_id == id)
                bound_id = 0;

            if (id != -1)
            {
                GL.DeleteTexture(id);
                id = -1;
                bitmap = null;
            }
        }

        public void Bitmap(Pixmap pixmap)
        {
            Bind();

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                pixmap.GetWidth(),
                pixmap.GetHeight(),
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                pixmap.GetPixels());

            premultiplied = true;

            this.bitmap = pixmap;

            width = bitmap.GetWidth();
            height = bitmap.GetHeight();
        }

        public void SubBitmap(Pixmap pixmap, int xoffset, int yoffset, int width, int height)
        {
            Bind();
            //Gdx.gl.glTexSubImage2D(page.texture.glTarget, 0, rectX, rectY, rectWidth, rectHeight, image.getGLFormat(),
            //    image.getGLType(), image.getPixels());
            GL.TexSubImage2D(
                TextureTarget.Texture2D,
                0,
                xoffset,
                yoffset,
                width,
                height,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                pixmap.GetPixels());
        }

        //public void pixels(int w, int h, byte[] pixels)
        //{
        //
        //    bind();
        //
        //    ByteBuffer imageBuffer = ByteBuffer.
        //        allocateDirect(w * h).
        //        order(ByteOrder.nativeOrder());
        //    imageBuffer.put(pixels);
        //    imageBuffer.position(0);
        //
        //    Gdx.gl.glPixelStorei(Gdx.gl.GL_UNPACK_ALIGNMENT, 1);
        //
        //    Gdx.gl.glTexImage2D(
        //        Gdx.gl.GL_TEXTURE_2D,
        //        0,
        //        Gdx.gl.GL_ALPHA,
        //        w,
        //        h,
        //        0,
        //        Gdx.gl.GL_ALPHA,
        //        Gdx.gl.GL_UNSIGNED_BYTE,
        //        imageBuffer);
        //}
        //
        //public static Texture create(Pixmap pix)
        //{
        //    Texture tex = new Texture();
        //    tex.bitmap(pix);
        //
        //    return tex;
        //}
        //
        //public static Texture create(int width, int height, int[] pixels)
        //{
        //    Texture tex = new Texture();
        //    tex.pixels(width, height, pixels);
        //
        //    return tex;
        //}
        //
        //public static Texture create(int width, int height, byte[] pixels)
        //{
        //    Texture tex = new Texture();
        //    tex.pixels(width, height, pixels);
        //
        //    return tex;
        //}

        public Color GetPixel(int x, int y)
        {
            return bitmap.GetPixel(x, y);
        }

        public void Reload()
        {
            id = -1;
            Generate();
        }

        public RectF UvRect(int left, int top, int right, int bottom)
        {
            return new RectF(
                (float)left / width,
                (float)top / height,
                (float)right / width,
                (float)bottom / height);
        }
    }
}