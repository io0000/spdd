using watabou.utils;
using watabou.glwrap;

namespace watabou.noosa
{
    public class Visual : Gizmo
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public PointF scale;
        public PointF origin;

        public float[] matrix;

        public float rm;
        public float gm;
        public float bm;
        public float am;
        public float ra;
        public float ga;
        public float ba;
        public float aa;

        public PointF speed;
        public PointF acc;

        public float angle;
        public float angularSpeed;

        public Visual(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;

            scale = new PointF(1.0f, 1.0f);
            origin = new PointF();

            matrix = new float[16];

            ResetColor();

            speed = new PointF();
            acc = new PointF();
        }

        public override void Update()
        {
            UpdateMotion();
        }

        // diff
        public override void Draw()
        {
            UpdateMatrix();
        }

        protected virtual void UpdateMatrix()
        {
            Matrix.SetIdentity(matrix);
            
            Matrix.Translate(matrix, x, y);
            
            if (origin.x != 0 || origin.y != 0)
                Matrix.Translate(matrix, origin.x, origin.y);
            
            if (angle != 0)
                Matrix.Rotate(matrix, angle);
            
            if (scale.x != 1 || scale.y != 1)
                Matrix.Scale(matrix, scale.x, scale.y);
            
            if (origin.x != 0 || origin.y != 0)
                Matrix.Translate(matrix, -origin.x, -origin.y);
        }

        public PointF Point()
        {
            return new PointF(x, y);
        }

        public PointF Point(PointF p)
        {
            x = p.x;
            y = p.y;
            return p;
        }

        //public virtual Point Point(Point p)
        //{
        //    X = p.X;
        //    Y = p.Y;
        //    return p;
        //}

        public PointF Center()
        {
            return new PointF(x + Width() / 2, y + Height() / 2);
        }

        public PointF Center(PointF p)
        {
            x = p.x - Width() / 2;
            y = p.y - Height() / 2;
            return p;
        }

        //returns the point needed to center the argument visual on this visual
        public PointF Center(Visual v)
        {
            return new PointF(
                    x + (Width() - v.Width()) / 2f,
                    y + (Height() - v.Height()) / 2f
            );
        }

        public virtual float Width()
        {
            return width * scale.x;
        }

        public virtual float Height()
        {
            return height * scale.y;
        }

        public void UpdateMotion()
        {
            float elapsed = Game.elapsed;

            if (acc.x != 0.0f)
                speed.x += acc.x * elapsed;
            if (speed.x != 0.0f)
                x += speed.x * elapsed;

            if (acc.y != 0.0f)
                speed.y += acc.y * elapsed;
            if (speed.y != 0.0f)
                y += speed.y * elapsed;

            if (angularSpeed != 0.0f)
                angle += angularSpeed * elapsed;
        }

        public void Alpha(float value)
        {
            am = value;
            aa = 0.0f;
        }

        public float Alpha()
        {
            return am + aa;
        }

        public void Invert()
        {
            rm = gm = bm = -1.0f;
            ra = ga = ba = +1.0f;
        }

        public void Lightness(float value)
        {
            if (value < 0.5f)
            {
                rm = gm = bm = value * 2.0f;
                ra = ga = ba = 0.0f;
            }
            else
            {
                rm = gm = bm = 2.0f - value * 2.0f;
                ra = ga = ba = value * 2.0f - 1.0f;
            }
        }

        public void Brightness(float value)
        {
            rm = gm = bm = value;
        }

        public void Tint(float r, float g, float b, float strength)
        {
            rm = gm = bm = 1.0f - strength;
            ra = r * strength;
            ga = g * strength;
            ba = b * strength;
        }

        public void Tint(Color color, float strength)
        {
            rm = gm = bm = 1.0f - strength;
            ra = color.R / 255.0f * strength;
            ga = color.G / 255.0f * strength;
            ba = color.B / 255.0f * strength;
        }

        //color must include an alpha component (e.g. 0x80FF0000 for red at 0.5 strength)
        public void Tint(Color color)
        {
            // tint( color & 0xFFFFFF, ((color >> 24) & 0xFF) / (float)0xFF);
            float strength = color.A / (float)0xFF;
            color.A = 0;

            Tint(color, strength);
        }

        //public void color( float r, float g, float b )
        public void SetColor(float r, float g, float b)
        {
            rm = gm = bm = 0.0f;
            ra = r;
            ga = g;
            ba = b;
        }

        //public void color( int color )
        public void SetColor(Color color)
        {
            SetColor(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
        }

        public void Hardlight(float r, float g, float b)
        {
            ra = ga = ba = 0.0f;
            rm = r;
            gm = g;
            bm = b;
        }

        public void Hardlight(Color color)
        {
            Hardlight(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
        }

        public virtual void ResetColor()
        {
            rm = gm = bm = am = 1.0f;
            ra = ga = ba = aa = 0.0f;
        }

        public virtual bool OverlapsPoint(float x, float y)
        {
            bool b1 = x >= this.x;
            bool b2 = x < this.x + width * scale.x;
            bool b3 = y >= this.y;
            bool b4 = y < this.y + height * scale.y;

            return x >= this.x && x < this.x + width * scale.x && y >= this.y && y < this.y + height * scale.y;
        }

        public virtual bool OverlapsScreenPoint(int x, int y)
        {
            var c = GetCamera();
            if (c == null)
                return false;

            var p = c.ScreenToCamera(x, y);
            return OverlapsPoint(p.x, p.y);
        }

        // true if its bounding box intersects its camera's bounds
        public override bool IsVisible()
        {
            Camera c = GetCamera();

            if (c == null || !visible)
                return false;

            //FIXME, the below calculations ignore angle, so assume visible if angle != 0
            if (angle != 0.0f)
                return true;

            //x coord
            if (x > c.scroll.x + c.width)
                return false;
            else if (!(x >= c.scroll.x || x + Width() >= c.scroll.x))
                return false;

            //y coord
            if (y > c.scroll.y + c.height)
                return false;
            else if (!(y >= c.scroll.y || y + Height() >= c.scroll.y))
                return false;

            return true;
        }
    }
}