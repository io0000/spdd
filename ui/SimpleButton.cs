using watabou.input;
using watabou.noosa;
using watabou.noosa.ui;

namespace spdd.ui
{
    public class SimpleButton : Component
    {
        private Image image;

        public SimpleButton(Image image)
        {
            this.image.Copy(image);
            width = image.width;
            height = image.height;
        }

        // base class의 ctor 에서 호출됨 SimpleButton()보다 먼저 호출됨
        protected override void CreateChildren()
        {
            image = new Image();
            Add(image);

            var pointerArea = new SimplePointerArea(this, image);
            Add(pointerArea);
        }

        protected override void Layout()
        {
            image.x = x;
            image.y = y;
        }

        protected virtual void OnClick()
        { }

        private class SimplePointerArea : PointerArea
        {
            private SimpleButton button;

            public SimplePointerArea(SimpleButton button, Visual target)
                : base(target)
            {
                this.button = button;
            }

            public override void OnPointerDown(PointerEvent touch)
            {
                button.image.Brightness(1.2f);
            }

            public override void OnPointerUp(PointerEvent touch)
            {
                button.image.Brightness(1.0f);
            }

            public override void OnClick(PointerEvent touch)
            {
                button.OnClick();
            }
        }
    }
}