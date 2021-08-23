using System.Collections.Generic;

namespace watabou.utils
{
    // T - key, touch
    public class Signal<T>
    {
        private List<IListener> listeners = new List<IListener>();

        private bool stackMode;

        public Signal()
            : this(false)
        { }

        public Signal(bool stackMode)
        {
            this.stackMode = stackMode;
        }

        public void Add(IListener listener)
        {
            if (listeners.Contains(listener))
                return;

            if (stackMode)
                listeners.Insert(0, listener);
            else
                listeners.Add(listener);
        }

        public void Remove(IListener listener)
        {
            listeners.Remove(listener);
        }

        public void RemoveAll()
        {
            listeners.Clear();
        }

        public void Replace(IListener listener)
        {
            RemoveAll();
            Add(listener);
        }

        public void Dispatch(T t)
        {
            var arr = listeners.ToArray();

            foreach (var listener in arr)
            {
                if (listeners.Contains(listener))
                {
                    if (listener.OnSignal(t) == true)
                        return;
                }
            }
        }

        public interface IListener
        {
            //return true if the signal has been handled
            bool OnSignal(T t);
        }
    }
}