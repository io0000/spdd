using System.Collections.Generic;
using watabou.utils;

namespace watabou.input
{
    public class ScrollEvent
    {
        public PointF pos;
        public int amount;

        public ScrollEvent(PointF mousePos, int amount)
        {
            this.amount = amount;
            this.pos = mousePos;
        }

        // **********************
        // *** Static members ***
        // **********************

        private static Signal<ScrollEvent> scrollSignal = new Signal<ScrollEvent>(true);

        // ScrollArea에서 사용
        public static void AddScrollListener(Signal<ScrollEvent>.IListener listener)
        {
            scrollSignal.Add(listener);
        }

        // ScrollArea에서 사용
        public static void RemoveScrollListener(Signal<ScrollEvent>.IListener listener)
        {
            scrollSignal.Remove(listener);
        }

        public static void ClearListeners()
        {
            scrollSignal.RemoveAll();
        }

        //Accumulated key events
        private static List<ScrollEvent> scrollEvents = new List<ScrollEvent>();

        public static void AddScrollEvent(ScrollEvent ev)
        {
            scrollEvents.Add(ev);
        }

        public static void ProcessScrollEvents()
        {
            foreach (ScrollEvent ev in scrollEvents)
            {
                scrollSignal.Dispatch(ev);
            }
            
            scrollEvents.Clear();
        }
    }
}