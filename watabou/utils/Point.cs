using System;

namespace watabou.utils
{
    public class Point : IEquatable<Point>
    {
        public int x;
        public int y;

        public Point()
        { }

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Point(Point p)
        {
            this.x = p.x;
            this.y = p.y;
        }

        public Point Set(int x, int y)
        {
            this.x = x;
            this.y = y;
            return this;
        }

        public Point Set(Point p)
        {
            this.x = p.x;
            this.y = p.y;
            return this;
        }

        public Point Clone()
        {
            return new Point(this);
        }

        public Point Scale(float f)
        {
            x = (int)(x * f);
            y = (int)(y * f);
            return this;
        }

        public Point Offset(int dx, int dy)
        {
            x += dx;
            y += dy;
            return this;
        }

        public Point Offset(Point d)
        {
            x += d.x;
            y += d.y;
            return this;
        }

        public override bool Equals(object obj)
        {
            return (obj is Point) && this.Equals((Point)obj);
        }

        public bool Equals(Point other)
        {
            return this.x == other.x && this.y == other.y;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        public static bool operator !=(Point a, Point b)
        {
            if (((object)a) == null || ((object)b) == null)
                return !object.Equals(a, b);

            return !a.Equals(b);
        }

        public static bool operator ==(Point a, Point b)
        {
            if (((object)a) == null || ((object)b) == null)
                return object.Equals(a, b);

            return a.Equals(b);
        }
    }
}

