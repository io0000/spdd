using watabou.input;
using watabou.utils;

namespace watabou.noosa
{
    //pointer area with additional support for detecting scrolling events
    public class ScrollArea : PointerArea
    {
        public ScrollArea(Visual target)
            : base(target)
        {
            scrollListener = new ScrollListener(this);
            ScrollEvent.AddScrollListener(scrollListener);
        }

        public ScrollArea(float x, float y, float width, float height)
            : base(x, y, width, height)
        {
            scrollListener = new ScrollListener(this);
            ScrollEvent.AddScrollListener(scrollListener);
        }

        private ScrollListener scrollListener;

        private class ScrollListener : Signal<ScrollEvent>.IListener
        {
            private ScrollArea scrollArea;

            public ScrollListener(ScrollArea scrollArea)
            {
                this.scrollArea = scrollArea;
            }

            public bool OnSignal(ScrollEvent ev)
            {
                return scrollArea.OnSignal(ev);
            }
        }

        private bool OnSignal(ScrollEvent ev)
        {
            bool hit = ev != null && target.OverlapsScreenPoint((int)ev.pos.x, (int)ev.pos.y);

            if (!IsActive())
                return (hit && blockWhenInactive);

            if (hit)
            {
                OnScroll(ev);
                return true;
            }

            return false;
        }

        protected virtual void OnScroll(ScrollEvent ev)
        { }

        public override void Destroy()
        {
            base.Destroy();
            ScrollEvent.RemoveScrollListener(scrollListener);
        }
    }
}