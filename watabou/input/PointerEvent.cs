using System.Collections.Generic;
using watabou.utils;

namespace watabou.input
{
    public class PointerEvent
    {
        public PointF start;
        public PointF current;
        public int id;
        public bool down;

        public PointerEvent(int x, int y, int id, bool down)
        {
            start = current = new PointF(x, y);
            this.id = id;
            this.down = down;
        }

        public void Update(PointerEvent other)
        {
            this.current = other.current;
        }

        public void Update(float x, float y)
        {
            current.Set(x, y);
        }

        public PointerEvent Up()
        {
            down = false;
            return this;
        }


        // **********************
        // *** Static members ***
        // **********************

        private static Signal<PointerEvent> pointerSignal = new Signal<PointerEvent>(true);

        public static void AddPointerListener(Signal<PointerEvent>.IListener listener)
        {
            pointerSignal.Add(listener);
        }

        public static void RemovePointerListener(Signal<PointerEvent>.IListener listener)
        {
            pointerSignal.Remove(listener);
        }

        public static void ClearListeners()
        {
            pointerSignal.RemoveAll();
        }

        // Accumulated pointer events
        private static List<PointerEvent> pointerEvents = new List<PointerEvent>();
        private static Dictionary<int, PointerEvent> activePointers = new Dictionary<int, PointerEvent>();

        public static void AddPointerEvent(PointerEvent ev)
        {
            pointerEvents.Add(ev);
        }

        public static void ProcessPointerEvents()
        {
            foreach (PointerEvent p in pointerEvents)
            {
                if (activePointers.ContainsKey(p.id))
                {
                    PointerEvent existing = activePointers[p.id];
                    existing.current = p.current;
                    if (existing.down == p.down)
                    {
                        pointerSignal.Dispatch(null);
                    }
                    else if (p.down)
                    {
                        pointerSignal.Dispatch(existing);
                    }
                    else
                    {
                        activePointers.Remove(existing.id);
                        pointerSignal.Dispatch(existing.Up());
                    }
                }
                else
                {
                    if (p.down)
                    {
                        activePointers.Add(p.id, p);
                    }
                    pointerSignal.Dispatch(p);
                }
            }
            pointerEvents.Clear();
        }
    }
}