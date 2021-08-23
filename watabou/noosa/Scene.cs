using watabou.input;
using watabou.utils;

namespace watabou.noosa
{
    public class Scene : Group
    {
        private Signal<KeyEvent>.IListener keyListener;

        public virtual void Create()
        {
            keyListener = new KeyListener(this);
            KeyEvent.AddKeyListener(keyListener);
        }

        class KeyListener : Signal<KeyEvent>.IListener
        {
            private Scene scene;
            public KeyListener(Scene scene)
            {
                this.scene = scene;
            }

            public bool OnSignal(KeyEvent ev)
            {
                if (Game.instance != null && ev.pressed)
                {
                    if (KeyBindings.GetActionForKey(ev) == GameAction.BACK)
                    {
                        scene.OnBackPressed();
                    }
                }
                return false;
            }
        }

        public override void Destroy()
        {
            KeyEvent.RemoveKeyListener(keyListener);
            base.Destroy();
        }

        public virtual void OnPause()
        { }

        //public virtual void OnResume()
        //{
        //}

        public static bool Landscape()
        {
            return Game.width > Game.height;
        }

        public override void Update()
        {
            base.Update();
        }

        //public Camera camera()
        public override Camera GetCamera()
        {
            return Camera.main;
        }

        public virtual void OnBackPressed()
        {
            Game.instance.Finish();
        }
    }
}