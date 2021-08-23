using System;

namespace spdd.ui
{
    public class RedButton : StyledButton
    {
        public RedButton(string label)
            : this(label, 9)
        { }

        public RedButton(string label, int size)
            : base(Chrome.Type.RED_BUTTON, label, size)
        { }
    }

    public class ActionRedButton : RedButton
    {
        public Action action;

        public ActionRedButton(string label)
            : base(label)
        { }

        public ActionRedButton(string label, int size)
            : base(label, size)
        { }

        protected override void OnClick()
        {
            action();
        }
    }
}