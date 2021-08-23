using watabou.input;
using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;
using spdd.scenes;

namespace spdd.ui
{
    public class ScrollPane : Component
    {
        protected static Color THUMB_COLOR = new Color(0x7b, 0x80, 0x73, 0xFF);
        protected const float THUMB_ALPHA = 0.5f;

        protected PointerController controller;
        protected Component content;
        protected ColorBlock thumb;

        public ScrollPane(Component content)
        {
            this.content = content;
            AddToBack(content);

            width = content.Width();
            height = content.Height();

            content.camera = new Camera(0, 0, 1, 1, PixelScene.defaultZoom);
            Camera.Add(content.camera);
        }

        public override void Destroy()
        {
            base.Destroy();
            Camera.Remove(content.camera);
        }

        public void ScrollTo(float x, float y)
        {
            content.camera.scroll.Set(x, y);
        }

        protected override void CreateChildren()
        {
            controller = new PointerController(this);
            Add(controller);

            thumb = new ColorBlock(1, 1, THUMB_COLOR);
            thumb.am = THUMB_ALPHA;
            Add(thumb);
        }

        protected override void Layout()
        {
            content.SetPos(0, 0);
            controller.x = x;
            controller.y = y;
            controller.width = width;
            controller.height = height;

            var p = GetCamera().CameraToScreen(x, y);
            var cs = content.camera;
            cs.x = p.x;
            cs.y = p.y;
            cs.Resize((int)width, (int)height);

            thumb.visible = height < content.Height();
            if (thumb.visible)
            {
                thumb.scale.Set(2, height * height / content.Height());
                thumb.x = Right() - thumb.Width();
                thumb.y = y;
            }
        }

        public Component Content()
        {
            return content;
        }

        public virtual void OnClick(float x, float y)
        { }

        public class PointerController : ScrollArea
        {
            ScrollPane scrollPane;
            private float dragThreshold;

            public PointerController(ScrollPane scrollPane)
                : base(0, 0, 0, 0)
            {
                this.scrollPane = scrollPane;
                dragThreshold = PixelScene.defaultZoom * 8;
            }

            protected override void OnScroll(ScrollEvent ev)
            {
                PointF newPt = new PointF(lastPos);
                newPt.y -= ev.amount * scrollPane.content.camera.zoom * 10;
                Scroll(newPt);
                dragging = false;
            }

            public override void OnPointerUp(PointerEvent ev)
            {
                if (dragging)
                {
                    dragging = false;
                    scrollPane.thumb.am = THUMB_ALPHA;
                }
                else
                {
                    PointF p = scrollPane.content.camera.ScreenToCamera((int)ev.current.x, (int)ev.current.y);
                    scrollPane.OnClick(p.x, p.y);
                }
            }

            private bool dragging;
            private PointF lastPos = new PointF();

            public override void OnDrag(PointerEvent ev)
            {
                if (dragging)
                {
                    Scroll(ev.current);
                }
                else if (PointF.Distance(ev.current, ev.start) > dragThreshold)
                {
                    dragging = true;
                    lastPos.Set(ev.current);
                    scrollPane.thumb.am = 1.0f;
                }
            }

            private void Scroll(PointF current)
            {
                var content = scrollPane.Content();
                var c = content.GetCamera();

                c.Shift(PointF.Diff(lastPos, current).InvScale(c.zoom));

                if (c.scroll.x + width > content.Width())
                    c.scroll.x = content.Width() - width;

                if (c.scroll.x < 0)
                    c.scroll.x = 0;

                if (c.scroll.y + height > content.Height())
                    c.scroll.y = content.Height() - height;

                if (c.scroll.y < 0)
                    c.scroll.y = 0;

                scrollPane.thumb.y = y + height * c.scroll.y / content.Height();

                lastPos.Set(current);
            }
        }
    }
}