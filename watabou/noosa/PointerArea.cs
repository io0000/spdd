using watabou.input;
using watabou.utils;

namespace watabou.noosa
{
    public class PointerArea : Visual, Signal<PointerEvent>.IListener
    {
        // Its target can be toucharea itself
        protected Visual target;

        protected PointerEvent curEvent;

        //if true, this PointerArea will always block input, even when it is inactive
        public bool blockWhenInactive;

        public PointerArea(Visual target)
            : base(0, 0, 0, 0)
        {
            this.target = target;

            PointerEvent.AddPointerListener(this);
        }

        public PointerArea(float x, float y, float width, float height)
            : base(x, y, width, height)
        {
            target = this;

            visible = false;

            PointerEvent.AddPointerListener(this);
        }

        public virtual bool OnSignal(PointerEvent ev)
        {
            var hit = ev != null && target.OverlapsScreenPoint((int)ev.current.x, (int)ev.current.y);

            if (!IsActive())
                return (hit && blockWhenInactive);

            if (hit)
            {
                bool returnValue = (ev.down || ev == curEvent);

                if (ev.down)
                {
                    if (curEvent == null)
                        curEvent = ev;

                    OnPointerDown(ev);
                }
                else
                {
                    OnPointerUp(ev);

                    if (curEvent == ev)
                    {
                        curEvent = null;
                        OnClick(ev);
                    }
                }

                return returnValue;
            }
            else
            {
                if (ev == null && curEvent != null)
                {
                    OnDrag(curEvent);
                }
                else if (curEvent != null && !ev.down)
                {
                    OnPointerUp(ev);
                    curEvent = null;
                }

                return false;
            }
        }

        public virtual void OnPointerDown(PointerEvent ev)
        { }

        public virtual void OnPointerUp(PointerEvent ev)
        { }

        public virtual void OnClick(PointerEvent ev)
        { }

        public virtual void OnDrag(PointerEvent ev)
        { }

        public virtual void Reset()
        {
            curEvent = null;
        }

        public override void Destroy()
        {
            PointerEvent.RemovePointerListener(this);
            base.Destroy();
        }
    }
}