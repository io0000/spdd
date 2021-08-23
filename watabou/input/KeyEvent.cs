using System.Collections.Generic;
using watabou.utils;

namespace watabou.input
{
    public class KeyEvent
    {
        public int code;
        public bool pressed;

        public KeyEvent(int code, bool pressed)
        {
            this.code = code;
            this.pressed = pressed;
        }

        // **********************
        // *** Static members ***
        // **********************

        private static Signal<KeyEvent> keySignal = new Signal<KeyEvent>(true);

        public static void AddKeyListener(Signal<KeyEvent>.IListener listener)
        {
            keySignal.Add(listener);
        }

        public static void RemoveKeyListener(Signal<KeyEvent>.IListener listener)
        {
            keySignal.Remove(listener);
        }

        public static void ClearListeners()
        {
            keySignal.RemoveAll();
        }

        //Accumulated key events
        private static List<KeyEvent> keyEvents = new List<KeyEvent>();

        public static void AddKeyEvent(KeyEvent ev)
        {
            keyEvents.Add(ev);
        }

        public static void ProcessKeyEvents()
        {
            foreach (KeyEvent k in keyEvents)
            {
                keySignal.Dispatch(k);
            }
            keyEvents.Clear();
        }
    }
}