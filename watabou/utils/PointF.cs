using System;

namespace watabou.utils
{
    public class PointF : IEquatable<PointF>
    {
        public const float PI = 3.1415926f;
        public const float PI2 = PI * 2.0f;
        public const float G2R = PI / 180.0f;

        public float x;
        public float y;

        public PointF()
        { }

        public PointF(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public PointF(PointF p)
        {
            this.x = p.x;
            this.y = p.y;
        }

        public PointF(Point p)
        {
            this.x = p.x;
            this.y = p.y;
        }

        //public PointF Clone()
        //{
        //    return new PointF(this);
        //}

        public PointF Scale(float f)
        {
            this.x *= f;
            this.y *= f;
            return this;
        }

        public PointF InvScale(float f)
        {
            this.x /= f;
            this.y /= f;
            return this;
        }

        public PointF Set(float x, float y)
        {
            this.x = x;
            this.y = y;
            return this;
        }

        public PointF Set(PointF p)
        {
            this.x = p.x;
            this.y = p.y;
            return this;
        }

        public PointF Set(float v)
        {
            this.x = v;
            this.y = v;
            return this;
        }

        public PointF Polar(float a, float l)
        {
            this.x = l * (float)Math.Cos(a);
            this.y = l * (float)Math.Sin(a);
            return this;
        }

        public PointF Offset(float dx, float dy)
        {
            x += dx;
            y += dy;
            return this;
        }

        public PointF Offset(PointF p)
        {
            x += p.x;
            y += p.y;
            return this;
        }

        public PointF Negate()
        {
            x = -x;
            y = -y;
            return this;
        }

        public PointF Normalize()
        {
            var l = Length();
            x /= l;
            y /= l;
            return this;
        }

        //public Point Floor()
        //{
        //    return new Point((int)x, (int)y);
        //}

        public float Length()
        {
            float value = x * x + y * y;
            return (float)Math.Sqrt(value);
        }

        //public static PointF Sum(PointF a, PointF b)
        //{
        //    return new PointF(a.x + b.x, a.y + b.y);
        //}

        public static PointF Diff(PointF a, PointF b)
        {
            return new PointF(a.x - b.x, a.y - b.y);
        }

        public static PointF Inter(PointF a, PointF b, float d)
        {
            return new PointF(a.x + (b.x - a.x) * d, a.y + (b.y - a.y) * d);
        }

        public static float Distance(PointF a, PointF b)
        {
            var dx = a.x - b.x;
            var dy = a.y - b.y;
            float value = dx * dx + dy * dy;
            return (float)Math.Sqrt(value);
        }

        public static float Angle(PointF start, PointF end)
        {
            return (float)Math.Atan2(end.y - start.y, end.x - start.x);
        }

        public override string ToString()
        {
            return "" + x + ", " + y;
        }

        public override bool Equals(object obj)
        {
            return (obj is PointF) && this.Equals((PointF)obj);
        }

        public bool Equals(PointF other)
        {
            return this.x == other.x && this.y == other.y;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        public static bool operator !=(PointF a, PointF b)
        {
            if (((object)a) == null || ((object)b) == null)
                return !object.Equals(a, b);

            return !a.Equals(b);
        }

        public static bool operator ==(PointF a, PointF b)
        {
            if (((object)a) == null || ((object)b) == null)
                return object.Equals(a, b);

            return a.Equals(b);
        }
    }
}