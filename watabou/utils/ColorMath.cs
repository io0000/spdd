using System.Runtime.InteropServices;

namespace watabou.utils
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Color
    {
        [FieldOffset(0)]
        public int i32value;

        [FieldOffset(0)]
        public byte R;

        [FieldOffset(1)]
        public byte G;

        [FieldOffset(2)]
        public byte B;

        [FieldOffset(3)]
        public byte A;

        public Color(byte r, byte g, byte b, byte a)
        {
            i32value = 0;

            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color(int i32value)
        {
            R = 0;
            G = 0;
            B = 0;
            A = 0;

            this.i32value = i32value;
        }
    }

    public class ColorMath
    {
        public static Color Interpolate(Color c1, Color c2, float p)
        {
            if (p <= 0)
                return c1;
            else if (p >= 1)
                return c2;

            int r1 = c1.R;
            int g1 = c1.G;
            int b1 = c1.B;

            int r2 = c2.R;
            int g2 = c2.G;
            int b2 = c2.B;

            float p1 = 1.0f - p;

            int r = (int)(p1 * r1 + p * r2);
            int g = (int)(p1 * g1 + p * g2);
            int b = (int)(p1 * b1 + p * b2);

            return new Color((byte)r, (byte)g, (byte)b, 0xff);
        }

        public static Color Interpolate(float p, params Color[] colors)
        {
            if (p <= 0)
                return colors[0];

            int n = colors.Length - 1;

            if (p >= 1)
                return colors[n];

            var segment = (int)(n * p);
            return Interpolate(colors[segment], colors[segment + 1], (p * n) % 1);
        }

        public static Color Random(Color a, Color b)
        {
            return Interpolate(a, b, utils.Rnd.Float());
        }
    }
}