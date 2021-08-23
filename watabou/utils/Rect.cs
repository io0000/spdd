using System;
using System.Collections.Generic;

namespace watabou.utils
{
    public class Rect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public Rect()
            : this(0, 0, 0, 0)
        { }

        public Rect(Rect rect)
            : this(rect.left, rect.top, rect.right, rect.bottom)
        { }

        public Rect(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public virtual int Width()
        {
            return right - left;
        }

        public virtual int Height()
        {
            return bottom - top;
        }

        public int Square()
        {
            return (right - left) * (bottom - top);
        }

        public Rect Set(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            return this;
        }

        public Rect Set(Rect rect)
        {
            return Set(rect.left, rect.top, rect.right, rect.bottom);
        }

        public Rect SetPos(int x, int y)
        {
            return Set(x, y, x + (right - left), y + (bottom - top));
        }

        public Rect Shift(int x, int y)
        {
            return Set(left + x, top + y, right + x, bottom + y);
        }

        public Rect Resize(int w, int h)
        {
            return Set(left, top, left + w, top + h);
        }

        public bool IsEmpty()
        {
            return right <= left || bottom <= top;
        }

        public Rect SetEmpty()
        {
            left = right = top = bottom = 0;
            return this;
        }

        public Rect Intersect(Rect other)
        {
            var result = new Rect();
            result.left = Math.Max(left, other.left);
            result.right = Math.Min(right, other.right);
            result.top = Math.Max(top, other.top);
            result.bottom = Math.Min(bottom, other.bottom);
            return result;
        }

        public Rect Union(Rect other)
        {
            Rect result = new Rect();
            result.left = Math.Min(left, other.left);
            result.right = Math.Max(right, other.right);
            result.top = Math.Min(top, other.top);
            result.bottom = Math.Max(bottom, other.bottom);
            return result;
        }

        public Rect Union(int x, int y)
        {
            if (IsEmpty())
                return Set(x, y, x + 1, y + 1);

            if (x < left)
                left = x;
            else if (x >= right)
                right = x + 1;

            if (y < top)
                top = y;
            else if (y >= bottom)
                bottom = y + 1;

            return this;
        }

        //public Rect Union(Point p)
        //{
        //    return Union(p.x, p.y);
        //}

        public virtual bool Inside(Point p)
        {
            return p.x >= left && p.x < right && p.y >= top && p.y < bottom;
        }

        public virtual Point Center()
        {
            return new Point(
                    (left + right) / 2 + (((right - left) % 2) == 0 ? Rnd.Int(2) : 0),
                    (top + bottom) / 2 + (((bottom - top) % 2) == 0 ? Rnd.Int(2) : 0));
        }

        public Rect Shrink(int d)
        {
            return new Rect(left + d, top + d, right - d, bottom - d);
        }

        //public Rect Shrink()
        //{
        //    return Shrink(1);
        //}

        public List<Point> GetPoints()
        {
            List<Point> points = new List<Point>();
            for (int i = left; i <= right; ++i)
            {
                for (int j = top; j <= bottom; ++j)
                {
                    points.Add(new Point(i, j));
                }
            }

            return points;
        }
    }
}