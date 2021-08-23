using watabou.gltextures;
using watabou.glwrap;
using watabou.utils;

namespace watabou.noosa
{
    public class BitmapText : Visual
    {
        protected string text;
        protected Font font;

        //protected float[] vertices = new float[16];
        //protected FloatBuffer quads;
        //protected Vertexbuffer buffer;
        protected float[] quads;

        protected int realLength;

        protected bool dirty = true;

        public BitmapText()
            : this("", null)
        { }

        public BitmapText(Font font)
            : this("", font)
        { }

        public BitmapText(string text, Font font)
            : base(0, 0, 0, 0)
        {
            this.text = text;
            this.font = font;
        }

        protected override void UpdateMatrix()
        {
            // "origin" field is ignored
            Matrix.SetIdentity(matrix);
            Matrix.Translate(matrix, x, y);
            Matrix.Scale(matrix, scale.x, scale.y);
            Matrix.Rotate(matrix, angle);
        }

        public override void Draw()
        {
            base.Draw();

            if (dirty)
            {
                UpdateVertices();
                //quads.limit(quads.position());
                //if (buffer == null)
                //    buffer = new Vertexbuffer(quads);
                //else
                //    buffer.updateVertices(quads);
            }

            var script = NoosaScript.Get();

            font.texture.Bind();

            script.Camera(GetCamera());

            script.uModel.ValueM4(matrix);
            script.Lighting(
                rm, gm, bm, am,
                ra, ga, ba, aa);

            script.DrawQuadSet(quads, realLength);
        }

        public override void Destroy()
        {
            base.Destroy();
            //if (buffer != null)
            //    buffer.delete();
        }

        public void UpdateVertices()
        {
            width = 0;
            height = 0;

            if (text == null)
                text = "";

            //quads = Quad.createSet( text.length() );
            quads = new float[text.Length * 16];
            realLength = 0;

            var length = text.Length;
            for (var i = 0; i < length; ++i)
            {
                //RectF rect = font.get( text.charAt( i ) );
                var rect = font.Get(text[i]);

                //??
                //if (rect == null)
                //    rect = null;

                var w = font.Width(rect);
                var h = font.Height(rect);

                int index = realLength * 16;

                quads[index + 0] = width;
                quads[index + 1] = 0;

                quads[index + 2] = rect.left;
                quads[index + 3] = rect.top;

                quads[index + 4] = width + w;
                quads[index + 5] = 0;

                quads[index + 6] = rect.right;
                quads[index + 7] = rect.top;

                quads[index + 8] = width + w;
                quads[index + 9] = h;

                quads[index + 10] = rect.right;
                quads[index + 11] = rect.bottom;

                quads[index + 12] = width;
                quads[index + 13] = h;

                quads[index + 14] = rect.left;
                quads[index + 15] = rect.bottom;

                ++realLength;

                width += w + font.tracking;
                if (h > height)
                    height = h;
            }

            if (length > 0)
                width -= font.tracking;

            dirty = false;
        }

        public void Measure()
        {
            width = 0;
            height = 0;

            if (text == null)
                text = "";

            var length = text.Length;
            for (var i = 0; i < length; ++i)
            {
                var rect = font.Get(text[i]);

                var w = font.Width(rect);
                var h = font.Height(rect);

                width += w + font.tracking;
                if (h > height)
                    height = h;
            }

            if (length > 0)
                width -= font.tracking;
        }

        public float BaseLine()
        {
            return font.baseLine * scale.y;
        }

        //public Font font()
        public Font GetFont()
        {
            return font;
        }

        //public synchronized void font( Font value )
        //public void SetFont(Font value)
        //{
        //    font = value;
        //}

        public string Text()
        {
            return text;
        }

        public void Text(string str)
        {
            text = str;
            dirty = true;
        }

        public class Font : TextureFilm
        {
            public const string LATIN_FULL = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\u007F";

            public Texture texture;

            public float tracking;
            public float baseLine;

            public float lineHeight;

            public Font(Texture tx)
                : base(tx)
            {
                texture = tx;
            }

            //public Font(Texture tx, int width, string chars)
            //    : this(tx, width, tx.Height, chars)
            //{
            //}

            public Font(Texture tx, int width, int height, string chars)
                : base(tx)
            {
                texture = tx;

                int length = chars.Length;

                float uw = (float)width / tx.width;
                float vh = (float)height / tx.height;

                float left = 0;
                float right = 0;
                float top = 0;
                float bottom = vh;

                for (int i = 0; i < length; ++i)
                {
                    right = left + uw;
                    RectF rect = new RectF(left, top, right, bottom);
                    left = right;

                    Add(chars[i], rect);

                    if (left >= 1)
                    {
                        left = 0;
                        top = bottom;//top = (int)bottom;
                        bottom += vh;
                    }
                }

                lineHeight = baseLine = height;
            }

            public void SplitBy(Pixmap bitmap, int height, Color color, string chars)
            {
                var length = chars.Length;

                int width = bitmap.GetWidth();
                var vHeight = (float)height / bitmap.GetHeight();

                int pos = 0;
                int line = 0;

                for (pos = 0; pos < width; ++pos)
                    for (int j = 0; j < height; ++j)
                        if (bitmap.GetPixel(pos, j).i32value != color.i32value)
                            goto spaceMeasuring;

            spaceMeasuring:
                Add(' ', new RectF(0, 0, (float)pos / width, vHeight - 0.01f));

                int separator = pos;

                for (int i = 0; i < length; ++i)
                {
                    char ch = chars[i];
                    if (ch == ' ')
                        continue;

                    bool found;

                    do
                    {
                        if (separator >= width)
                        {
                            line += height;
                            separator = 0;
                        }

                        found = false;
                        for (int j = line; j < line + height; ++j)
                        {
                            if (ColorNotMatch(bitmap, separator, j, color))
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                            ++separator;
                    }
                    while (!found);

                    int start = separator;

                    do
                    {
                        if (++separator >= width)
                        {
                            line += height;
                            separator = start = 0;
                            if (line + height >= bitmap.GetHeight())
                                break;
                        }
                        found = true;
                        for (int j = line; j < line + height; ++j)
                        {
                            if (ColorNotMatch(bitmap, separator, j, color))
                            {
                                found = false;
                                break;
                            }
                        }
                    }
                    while (!found);

                    Add(ch, new RectF((float)start / width, (float)line / bitmap.GetHeight(), (float)separator / width, (float)line / bitmap.GetHeight() + vHeight));
                    ++separator;
                }

                lineHeight = baseLine = Height(frames[chars[0]]);
            }

            private bool ColorNotMatch(Pixmap pixmap, int x, int y, Color color)
            {
                var pixel = pixmap.GetPixel(x, y);
                if (pixel.A == 0)
                {
                    return color.i32value != 0;
                }

                return pixel.i32value != color.i32value;
            }

            public static Font ColorMarked(Pixmap bmp, Color color, string chars)
            {
                Font font = new Font(TextureCache.Get(bmp));
                font.SplitBy(bmp, bmp.GetHeight(), color, chars);
                return font;
            }

            //public static Font ColorMarked(string font_texture, int height, Color color, string chars)
            //{
            //    var tex = TextureCache.Get(font_texture);
            //
            //    var font = new Font(tex);
            //    font.SplitBy(tex, height, color, chars);
            //    return font;
            //}

            public RectF Get(char ch)
            {
                if (frames.ContainsKey(ch))
                {
                    return base.Get(ch);
                }
                else
                {
                    return base.Get('?');
                }
            }
        }
    }
}