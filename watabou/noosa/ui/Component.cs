namespace watabou.noosa.ui
{
    public class Component : Group
    {
        protected float x;
        protected float y;
        protected float width;
        protected float height;

        public Component()
        {
            CreateChildren();
        }

        public Component SetPos(float x, float y)
        {
            this.x = x;
            this.y = y;
            Layout();

            return this;
        }

        public Component SetSize(float width, float height)
        {
            this.width = width;
            this.height = height;
            Layout();

            return this;
        }

        public Component SetRect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            Layout();

            return this;
        }

        public bool Inside(float x, float y)
        {
            return x >= this.x && y >= this.y && x < (this.x + width) && y < (this.y + height);
        }

        public void Fill(Component c)
        {
            SetRect(c.x, c.y, c.width, c.height);
        }

        public float Left()
        {
            return x;
        }

        public float Right()
        {
            return x + width;
        }

        public float CenterX()
        {
            return x + width / 2;
        }

        public float Top()
        {
            return y;
        }

        public float Bottom()
        {
            return y + height;
        }

        public float CenterY()
        {
            return y + height / 2;
        }

        public float Width()
        {
            return width;
        }

        public float Height()
        {
            return height;
        }

        protected virtual void CreateChildren()
        { }

        protected virtual void Layout()
        { }
    }
}