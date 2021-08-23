using System;

namespace watabou.glwrap
{
    public class Matrix
    {
        public const float G2RAD = 0.01745329251994329576923690768489f;

        //public static float[] Clone(float[] m)
        //{
        //    var n = m.Length;
        //    var res = new float[n];
        //    do
        //    {
        //        res[--n] = m[n];
        //    } while (n > 0);
        //
        //    return res;
        //}

        public static void Copy(float[] src, float[] dst)
        {
            var n = src.Length;
            do
            {
                dst[--n] = src[n];
            }
            while (n > 0);
        }

        private static float[] identity = new float[]{
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        };

        public static void SetIdentity(float[] m)
        {
            Array.Copy(identity, 0, m, 0, identity.Length);
        }

        public static void Rotate(float[] m, float a)
        {
            a *= G2RAD;
            var sin = (float)Math.Sin(a);
            var cos = (float)Math.Cos(a);
            var m0 = m[0];
            var m1 = m[1];
            var m4 = m[4];
            var m5 = m[5];
            m[0] = m0 * cos + m4 * sin;
            m[1] = m1 * cos + m5 * sin;
            m[4] = -m0 * sin + m4 * cos;
            m[5] = -m1 * sin + m5 * cos;
        }

        public static void SkewX(float[] m, float a)
        {
            var t = (float)Math.Tan(a * G2RAD);
            m[4] += -m[0] * t;
            m[5] += -m[1] * t;
        }

        public static void SkewY(float[] m, float a)
        {
            var t = (float)Math.Tan(a * G2RAD);
            m[0] += m[4] * t;
            m[1] += m[5] * t;
        }

        public static void Scale(float[] m, float x, float y)
        {
            m[0] *= x;
            m[1] *= x;
            m[2] *= x;
            m[3] *= x;
            m[4] *= y;
            m[5] *= y;
            m[6] *= y;
            m[7] *= y;
        }

        public static void Translate(float[] m, float x, float y)
        {
            m[12] += m[0] * x + m[4] * y;
            m[13] += m[1] * x + m[5] * y;
        }

        //public static void Multiply(float[] left, float[] right, float[] result)
        //{
        //    float ax1 = left[0];
        //    float ay1 = left[1];
        //    float az1 = left[2];
        //    float aw1 = left[3];
        //
        //    float ax2 = left[4];
        //    float ay2 = left[5];
        //    float az2 = left[6];
        //    float aw2 = left[7];
        //
        //    float ax3 = left[8];
        //    float ay3 = left[9];
        //    float az3 = left[10];
        //    float aw3 = left[11];
        //
        //    float ax4 = left[12];
        //    float ay4 = left[13];
        //    float az4 = left[14];
        //    float aw4 = left[15];
        //
        //    float bx1 = right[0];
        //    float by1 = right[1];
        //    float bz1 = right[2];
        //    float bw1 = right[3];
        //
        //    float bx2 = right[4];
        //    float by2 = right[5];
        //    float bz2 = right[6];
        //    float bw2 = right[7];
        //
        //    float bx3 = right[8];
        //    float by3 = right[9];
        //    float bz3 = right[10];
        //    float bw3 = right[11];
        //
        //    float bx4 = right[12];
        //    float by4 = right[13];
        //    float bz4 = right[14];
        //    float bw4 = right[15];
        //
        //    result[0] = ax1 * bx1 + ax2 * by1 + ax3 * bz1 + ax4 * bw1;
        //    result[1] = ay1 * bx1 + ay2 * by1 + ay3 * bz1 + ay4 * bw1;
        //    result[2] = az1 * bx1 + az2 * by1 + az3 * bz1 + az4 * bw1;
        //    result[3] = aw1 * bx1 + aw2 * by1 + aw3 * bz1 + aw4 * bw1;
        //
        //    result[4] = ax1 * bx2 + ax2 * by2 + ax3 * bz2 + ax4 * bw2;
        //    result[5] = ay1 * bx2 + ay2 * by2 + ay3 * bz2 + ay4 * bw2;
        //    result[6] = az1 * bx2 + az2 * by2 + az3 * bz2 + az4 * bw2;
        //    result[7] = aw1 * bx2 + aw2 * by2 + aw3 * bz2 + aw4 * bw2;
        //
        //    result[8] = ax1 * bx3 + ax2 * by3 + ax3 * bz3 + ax4 * bw3;
        //    result[9] = ay1 * bx3 + ay2 * by3 + ay3 * bz3 + ay4 * bw3;
        //    result[10] = az1 * bx3 + az2 * by3 + az3 * bz3 + az4 * bw3;
        //    result[11] = aw1 * bx3 + aw2 * by3 + aw3 * bz3 + aw4 * bw3;
        //
        //    result[12] = ax1 * bx4 + ax2 * by4 + ax3 * bz4 + ax4 * bw4;
        //    result[13] = ay1 * bx4 + ay2 * by4 + ay3 * bz4 + ay4 * bw4;
        //    result[14] = az1 * bx4 + az2 * by4 + az3 * bz4 + az4 * bw4;
        //    result[15] = aw1 * bx4 + aw2 * by4 + aw3 * bz4 + aw4 * bw4;
        //}
    }
}