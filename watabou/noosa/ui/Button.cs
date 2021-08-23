using watabou.input;
using watabou.utils;

namespace watabou.noosa.ui
{
    public class Button : Component
    {
        public const float longClick = 1f;

        protected HotArea hotArea;

        protected bool pressed;
        protected float pressTime;
        protected bool processed;

        protected override void CreateChildren()
        {
            hotArea = new HotArea(this);
            Add(hotArea);

            keyListener = new KeyListener();
            keyListener.button = this;
            KeyEvent.AddKeyListener(keyListener);
        }

        protected class HotArea : PointerArea
        {
            private Button button;

            public HotArea(Button button)
                : base(0.0f, 0.0f, 0.0f, 0.0f)
            {
                this.button = button;
            }

            public override void OnPointerDown(PointerEvent touch)
            {
                button.pressed = true;
                button.pressTime = 0;
                button.processed = false;
                button.OnPointerDown();
            }

            public override void OnPointerUp(PointerEvent touch)
            {
                button.pressed = false;
                button.OnPointerUp();
            }

            public override void OnClick(PointerEvent touch)
            {
                if (!button.processed)
                    button.OnClick();
            }
        }

        private class KeyListener : Signal<KeyEvent>.IListener
        {
            public Button button;

            public bool OnSignal(KeyEvent ev)
            {
                if (button.active &&
                    ev.pressed &&
                    KeyBindings.GetActionForKey(ev) == button.KeyAction())
                {
                    button.OnClick();
                    return true;
                }
                return false;
            }
        }

        private KeyListener keyListener;

        public virtual GameAction KeyAction()
        {
            return null;
        }

        public override void Update()
        {
            base.Update();

            hotArea.active = visible;

            if (pressed)
            {
                if ((pressTime += Game.elapsed) >= longClick)
                {
                    pressed = false;
                    if (OnLongClick())
                    {
                        hotArea.Reset();
                        processed = true;
                        OnPointerUp();

                        Game.Vibrate(50);   // 50 milliseconds
                    }
                }
            }
        }

        protected virtual void OnPointerDown()
        { }

        protected virtual void OnPointerUp()
        { }

        protected virtual void OnClick()
        { }

        protected virtual bool OnLongClick()
        {
            return false;
        }

        protected override void Layout()
        {
            hotArea.x = x;
            hotArea.y = y;
            hotArea.width = width;
            hotArea.height = height;
        }

        public override void Destroy()
        {
            base.Destroy();
            KeyEvent.RemoveKeyListener(keyListener);
        }
    }
}