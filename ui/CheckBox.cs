using spdd.scenes;

namespace spdd.ui
{
    public class CheckBox : RedButton
    {
        private bool checkedState;  // checked 는 keyword라서 사용할 수 없음

        public CheckBox(string label)
            : base(label)
        {
            Icon(Icons.UNCHECKED.Get());
        }

        protected override void Layout()
        {
            base.Layout();

            float margin = (Height() - text.Height()) / 2;

            text.SetPos(x + margin, y + margin);
            PixelScene.Align(text);

            margin = (height - icon.height) / 2;

            icon.x = x + width - margin - icon.width;
            icon.y = y + margin;
            PixelScene.Align(icon);
        }

        public bool Checked()
        {
            return checkedState;
        }

        public void Checked(bool value)
        {
            if (checkedState != value)
            {
                checkedState = value;
                icon.Copy(checkedState ? Icons.CHECKED.Get() : Icons.UNCHECKED.Get());
            }
        }

        protected override void OnClick()
        {
            base.OnClick();
            Checked(!checkedState);
        }
    }
}