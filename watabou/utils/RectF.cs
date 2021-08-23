//using System;

namespace watabou.utils
{
    public class RectF
    {
        public float left;
        public float top;
        public float right;
        public float bottom;

        public RectF()
            : this(0.0f, 0.0f, 0.0f, 0.0f)
        { }

        public RectF(RectF rect)
            : this(rect.left, rect.top, rect.right, rect.bottom)
        { }

        public RectF(float left, float top, float right, float bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public float Width()
        {
            return right - left;
        }

        public float Height()
        {
            return bottom - top;
        }

        //public float Square()
        //{
        //    return (Right - Left) * (Bottom - Top);
        //}

        public RectF Set(float left, float top, float right, float bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            return this;
        }

        public RectF Set(RectF rect)
        {
            return Set(rect.left, rect.top, rect.right, rect.bottom);
        }

        public RectF SetPos(float x, float y)
        {
            return Set(x, y, x + (right - left), y + (bottom - top));
        }

        public RectF Shift(float x, float y)
        {
            return Set(left + x, top + y, right + x, bottom + y);
        }

        public RectF Resize(float w, float h)
        {
            return Set(left, top, left + w, top + h);
        }

        public bool IsEmpty()
        {
            return right <= left || bottom <= top;
        }

        //public RectF SetEmpty()
        //{
        //    left = right = top = bottom = 0.0f;
        //    return this;
        //}

        //public RectF Intersect(RectF other)
        //{
        //    var result = new RectF();
        //    result.left = Math.Max(left, other.left);
        //    result.right = Math.Min(right, other.right);
        //    result.top = Math.Max(top, other.top);
        //    result.bottom = Math.Min(bottom, other.bottom);
        //    return result;
        //}

        public RectF Union(float x, float y)
        {
            if (IsEmpty())
                return Set(x, y, x + 1.0f, y + 1.0f);

            if (x < left)
                left = x;
            else if (x >= right)
                right = x + 1.0f;

            if (y < top)
                top = y;
            else if (y >= bottom)
                bottom = y + 1.0f;

            return this;
        }

        //public RectF Union(Point p)
        //{
        //    return Union(p.x, p.y);
        //}

        public bool Inside(Point p)
        {
            return p.x >= left && p.x < right && p.y >= top && p.y < bottom;
        }

        public RectF Shrink(float d)
        {
            return new RectF(left + d, top + d, right - d, bottom - d);
        }

        //public RectF Shrink()
        //{
        //    return Shrink(1);
        //}
    }
}