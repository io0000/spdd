using watabou.input;
using watabou.noosa;
using watabou.utils;
using spdd.effects;
using spdd.scenes;

namespace spdd.ui
{
    public class Window : Group, Signal<KeyEvent>.IListener
    {
        public int width;
        public int height;

        protected int yOffset;

        protected WindowPointerArea blocker;
        protected ShadowBox shadow;
        protected NinePatch chrome;

        public static Color WHITE = new Color(0xFF, 0xFF, 0xFF, 0xFF);
        public static Color TITLE_COLOR = new Color(0xFF, 0xFF, 0x44, 0xFF);
        public static Color SHPX_COLOR = new Color(0x33, 0xBB, 0x33, 0xFF);

        public Window()
            : this(0, 0, 0, Chrome.Get(Chrome.Type.WINDOW))
        { }

        public Window(int width, int height)
            : this(width, height, 0, Chrome.Get(Chrome.Type.WINDOW))
        { }

        public Window(int width, int height, NinePatch chrome)
            : this(width, height, 0, chrome)
        { }

        public Window(int width, int height, int yOffset, NinePatch chrome)
        {
            this.yOffset = yOffset;

            blocker = new WindowPointerArea(0, 0, PixelScene.uiCamera.width, PixelScene.uiCamera.height);
            blocker.wnd = this;
            blocker.camera = PixelScene.uiCamera;
            Add(blocker);

            this.chrome = chrome;

            this.width = width;
            this.height = height;

            shadow = new ShadowBox();
            shadow.am = 0.5f;
            shadow.camera = PixelScene.uiCamera.visible ?
                    PixelScene.uiCamera : Camera.main;
            Add(shadow);

            chrome.x = -chrome.MarginLeft();
            chrome.y = -chrome.MarginTop();
            chrome.Size(
                width - chrome.x + chrome.MarginRight(),
                height - chrome.y + chrome.MarginBottom());
            Add(chrome);

            camera = new Camera(0, 0,
                (int)chrome.width,
                (int)chrome.height,
                PixelScene.defaultZoom);
            camera.x = (int)(Game.width - camera.width * camera.zoom) / 2;
            camera.y = (int)(Game.height - camera.height * camera.zoom) / 2;
            camera.y -= (int)(yOffset * camera.zoom);
            camera.scroll.Set(chrome.x, chrome.y);
            Camera.Add(camera);

            shadow.BoxRect(
                    camera.x / camera.zoom,
                    camera.y / camera.zoom,
                    chrome.Width(), chrome.height);

            KeyEvent.AddKeyListener(this);
        }

        public class WindowPointerArea : PointerArea
        {
            public Window wnd;
            public WindowPointerArea(float x, float y, float width, float height)
                : base(x, y, width, height)
            { }

            public override void OnClick(PointerEvent ev)
            {
                if (wnd.parent != null && !wnd.chrome.OverlapsScreenPoint(
                    (int)ev.current.x,
                    (int)ev.current.y))
                {
                    wnd.OnBackPressed();
                }
            }
        }

        public virtual void Resize(int w, int h)
        {
            this.width = w;
            this.height = h;

            chrome.Size(
                width + chrome.MarginHor(),
                height + chrome.MarginVer());

            camera.Resize((int)chrome.width, (int)chrome.height);
            camera.x = (int)(Game.width - camera.ScreenWidth()) / 2;
            camera.y = (int)(Game.height - camera.ScreenHeight()) / 2;
            camera.y += (int)(yOffset * camera.zoom);

            shadow.BoxRect(camera.x / camera.zoom, camera.y / camera.zoom, chrome.Width(), chrome.height);
        }

        public void Offset(int yOffset)
        {
            camera.y -= (int)(this.yOffset * camera.zoom);
            this.yOffset = yOffset;
            camera.y += (int)(yOffset * camera.zoom);

            shadow.BoxRect(camera.x / camera.zoom, camera.y / camera.zoom, chrome.Width(), chrome.height);
        }

        public virtual void Hide()
        {
            if (parent != null)
                parent.Erase(this);
            Destroy();
        }

        public override void Destroy()
        {
            base.Destroy();

            Camera.Remove(base.GetCamera());
            KeyEvent.RemoveKeyListener(this);
        }

        // Signal<KeyEvent>.IListener
        public virtual bool OnSignal(KeyEvent ev)
        {
            if (ev.pressed)
            {
                if (KeyBindings.GetActionForKey(ev) == SPDAction.BACK)
                {
                    OnBackPressed();
                }
            }

            //TODO currently always eats the key event as windows always take full focus
            // if they are ever made more flexible, might not want to do this in all cases
            return true;
        }

        public virtual void OnBackPressed()
        {
            Hide();
        }
    }
}