using System.IO;
using StbImageSharp;
using watabou.utils;

// TODO reload -> pixel데이터를 메모리에 가지고 있는다

//ex1)
// Pixmap pixmap = new Pixmap(2*RADIUS+1, 2*RADIUS+1, Pixmap.Format.RGBA8888);
// pixmap.setColor( 0xFFFFFF08 );
// for (int i = 0; i < RADIUS; i+=2) {
//    pixmap.fillCircle(RADIUS, RADIUS, (RADIUS - i));
//
//int w = RADIUS * 2 + 1;
//int h = RADIUS * 2 + 1;
//var color0 = new pdsharp.utils.Color(0xff, 0xff, 0xff, 0xff);
//var color1 = new pdsharp.utils.Color(0xff, 0xff, 0xff, 0x88);
//var canvas = new Canvas(w, h);
//canvas.FillCircle(RADIUS, RADIUS, RADIUS, color1.i32value);
//canvas.FillCircle(RADIUS, RADIUS, (int)(RADIUS * 0.75f), color0.i32value);
//var texture = new Texture(w, h, canvas.Pixels);

// ex2)
// Pixmap pixmap =new Pixmap( 1, 1, Pixmap.Format.RGBA8888 );
// pixmap.setColor( (color << 8) | (color >>> 24) );
// pixmap.fill();
//
//int[] pixels = new int[1];
//pixels[0] = color.i32value;
//var tx = new Texture(1, 1, pixels);

// ex3)
// Pixmap bmp = new Pixmap( Gdx.files.internal(assetName) );
//
//byte[] buffer = File.ReadAllBytes(bitmapName);
//ImageResult result = ImageResult.FromMemory(buffer, ColorComponents.RedGreenBlueAlpha);
//Bind();
//GL.TexImage2D(TextureTarget.Texture2D,
//    0,
//    PixelInternalFormat.Rgba,
//    result.Width,
//    result.Height,
//    0,
//    PixelFormat.Rgba,
//    PixelType.UnsignedByte,
//    result.Data);


// ex4)
// Pixmap pixmap = new Pixmap( colors.length, 1, Pixmap.Format.RGBA8888);
// for (int i=0; i < colors.length; ++i) {
//   pixmap.drawPixel( i, 0, (colors[i] << 8) | (colors[i] >>> 24) );
//
// var tx = new Texture(colors.Length, 1, colors);

namespace watabou.glwrap
{
    public class Pixmap
    {
        public enum Blending
        {
            None, SourceOver
        }

        private int width;
        private int height;
        private byte[] pixels;
        private Blending blending = Blending.SourceOver;
        private Color color = new Color(0, 0, 0, 0);

        public Pixmap(int width, int height)
        {
            this.width = width;
            this.height = height;

            pixels = new byte[this.width * this.height * 4];
        }

        public Pixmap(string bitmapName)
        {
            // TODO10 : try catch
            byte[] buffer = File.ReadAllBytes(bitmapName);

            ImageResult result = ImageResult.FromMemory(buffer, ColorComponents.RedGreenBlueAlpha);

            pixels = result.Data;

            //File.WriteAllBytes("test.bin", result.Data);

            width = result.Width;
            height = result.Height;
        }

        public void SetBlending(Blending blending)
        {
            this.blending = blending;
        }

        public Blending GetBlending()
        {
            return blending;
        }

        public void SetColor(Color color)
        {
            this.color = color;
        }

        public void FillRectangle(int x, int y, int width, int height)
        {
            FillRect(x, y, width, height, color);
        }

        public Color GetPixel(int x, int y)
        {
            int n = (y * width + x) * 4;

            byte r = pixels[n + 0];
            byte g = pixels[n + 1];
            byte b = pixels[n + 2];
            byte a = pixels[n + 3];

            return new Color(r, g, b, a);
        }

        public void SetPixel(int x, int y, Color c)
        {
            int n = (y * width + x) * 4;

            pixels[n + 0] = c.R;
            pixels[n + 1] = c.G;
            pixels[n + 2] = c.B;
            pixels[n + 3] = c.A;
        }

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }

        public byte[] GetPixels()
        {
            return pixels;
        }

        public void Fill()
        {
            int n = pixels.Length;
            for (int index = 0; index < n; index += 4)
            {
                pixels[index + 0] = color.R;
                pixels[index + 1] = color.G;
                pixels[index + 2] = color.B;
                pixels[index + 3] = color.A;
            }
        }

        public void FillCircle(int x, int y, int radius)
        {
            FillCircle(x, y, radius, color);
        }

        // https://en.wikipedia.org/wiki/Midpoint_circle_algorithm
        // https://www.geeksforgeeks.org/bresenhams-circle-drawing-algorithm/
        // http://rosettacode.org/wiki/Bitmap/Midpoint_circle_algorithm ** c interface
        // https://github.com/libgdx/libgdx/blob/master/gdx/jni/gdx2d/gdx2d.c 
        public void FillCircle(int x0, int y0, int radius, Color color)
        {
            int f = 1 - radius;
            int ddF_x = 1;
            int ddF_y = -2 * radius;
            int px = 0;
            int py = radius;

            HLine(x0, x0, y0 + radius, color);
            HLine(x0, x0, y0 - radius, color);
            HLine(x0 - radius, x0 + radius, y0, color);

            while (px < py)
            {
                if (f >= 0)
                {
                    --py;
                    ddF_y += 2;
                    f += ddF_y;
                }
                ++px;
                ddF_x += 2;
                f += ddF_x;

                HLine(x0 - px, x0 + px, y0 + py, color);
                HLine(x0 - px, x0 + px, y0 - py, color);
                HLine(x0 - py, x0 + py, y0 + px, color);
                HLine(x0 - py, x0 + py, y0 - px, color);
            }
        }

        // https://github.com/libgdx/libgdx/blob/master/gdx/jni/gdx2d/gdx2d.c , gdx2d_fill_rect
        void FillRect(int x, int y, int width, int height, Color color)
        {
            int x2 = x + width - 1;
            int y2 = y + height - 1;

            if (x >= this.width) return;
            if (y >= this.height) return;
            if (x2 < 0) return;
            if (y2 < 0) return;

            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x2 >= this.width) x2 = this.width - 1;
            if (y2 >= this.height) y2 = this.height - 1;

            ++y2;
            while (y != y2)
            {
                HLine(x, x2, y, color);
                ++y;
            }
        }


        // https://github.com/libgdx/libgdx/blob/master/gdx/jni/gdx2d/gdx2d.c 
        private void HLine(int x1, int x2, int y, Color color)
        {
            const int bpp = 4;      // byte per pixel
            Color col_format = color;

            if (y < 0 || y >= height)
                return;

            if (x1 > x2)
            {
                var tmp = x1;
                x1 = x2;
                x2 = tmp;
            }

            if (x1 >= width)
                return;
            if (x2 < 0)
                return;
            if (x1 < 0)
                x1 = 0;
            if (x2 >= width)
                x2 = width - 1;

            int index = (x1 + y * width) * bpp;

            while (x1 <= x2)
            {
                if (blending == Blending.SourceOver)
                {
                    // col_format = to_format(pixmap->format, blend(col, to_RGBA8888(pixmap->format, pget(ptr))));
                    // col_format = blend(col, to_RGBA8888(pixmap->format, pget(ptr)));
                    // col_format = blend(col, pget(ptr));

                    // pget
                    byte r = pixels[index + 0];
                    byte g = pixels[index + 1];
                    byte b = pixels[index + 2];
                    byte a = pixels[index + 3];

                    col_format = Blend(color, new Color(r, g, b, a));
                }

                // pset
                pixels[index + 0] = col_format.R;
                pixels[index + 1] = col_format.G;
                pixels[index + 2] = col_format.B;
                pixels[index + 3] = col_format.A;

                ++x1;
                index += bpp;
            }
        }

        static Color Blend(Color src, Color dst)
        {
            uint src_a = src.A;

            if (src_a == 0)
                return dst;

            uint src_b = src.B;
            uint src_g = src.G;
            uint src_r = src.R;

            uint dst_a = dst.A;
            uint dst_b = dst.B;
            uint dst_g = dst.G;
            uint dst_r = dst.R;

            dst_a -= (dst_a * src_a) / 255;
            uint a = dst_a + src_a;
            dst_r = (dst_r * dst_a + src_r * src_a) / a;
            dst_g = (dst_g * dst_a + src_g * src_a) / a;
            dst_b = (dst_b * dst_a + src_b * src_a) / a;

            return new Color((byte)dst_r, (byte)dst_g, (byte)dst_b, (byte)a);
        }

        public void DrawPixmap(Pixmap image, int x, int y)
        {
            BltSameSize(image, this, 0, 0, x, y, image.GetWidth(), image.GetHeight());
        }

        // static inline void blit_same_size(const gdx2d_pixmap* src_pixmap, const gdx2d_pixmap* dst_pixmap,
        //      int32_t src_x, int32_t src_y,
        //      int32_t dst_x, int32_t dst_y,
        //      uint32_t width, uint32_t height) 
        static void BltSameSize(Pixmap src_pixmap, Pixmap dst_pixmap,
            int src_x, int src_y,
            int dst_x, int dst_y,
            int width, int height)
        {
            const int bpp = 4;

            int spitch = bpp * src_pixmap.GetWidth();
            int dpitch = bpp * dst_pixmap.GetWidth();

            int sx = src_x;
            int sy = src_y;
            int dx = dst_x;
            int dy = dst_y;

            var src_pixels = src_pixmap.GetPixels();
            var dst_pixels = dst_pixmap.GetPixels();

            var blending = dst_pixmap.GetBlending();

            for (; sy < src_y + height; ++sy, ++dy)
            {
                if (sy < 0 || dy < 0)
                    continue;
                if (sy >= src_pixmap.GetHeight() || dy >= dst_pixmap.GetHeight())
                    break;

                for (sx = src_x, dx = dst_x; sx < src_x + width; ++sx, ++dx)
                {
                    if (sx < 0 || dx < 0)
                        continue;
                    if (sx >= src_pixmap.GetWidth() || dx >= dst_pixmap.GetWidth())
                        break;

                    int src_index = sx * bpp + sy * spitch;
                    int dst_index = dx * bpp + dy * dpitch;

                    byte src_r = src_pixels[src_index + 0];
                    byte src_g = src_pixels[src_index + 1];
                    byte src_b = src_pixels[src_index + 2];
                    byte src_a = src_pixels[src_index + 3];
                    var src_col = new Color(src_r, src_g, src_b, src_a);

                    if (blending == Blending.SourceOver)
                    {
                        //uint32_t dst_col = to_RGBA8888(dst_pixmap->format, dpget((void*)dst_ptr));
                        //src_col = to_format(dst_pixmap->format, blend(src_col, dst_col));

                        byte dst_r = dst_pixels[dst_index + 0];
                        byte dst_g = dst_pixels[dst_index + 1];
                        byte dst_b = dst_pixels[dst_index + 2];
                        byte dst_a = dst_pixels[dst_index + 3];
                        var dst_col = new Color(dst_r, dst_g, dst_b, dst_a);

                        src_col = Blend(src_col, dst_col);
                    }

                    dst_pixels[dst_index + 0] = src_col.R;
                    dst_pixels[dst_index + 1] = src_col.G;
                    dst_pixels[dst_index + 2] = src_col.B;
                    dst_pixels[dst_index + 3] = src_col.A;
                }
            }
        }
    }
}