using System;

namespace watabou.utils
{
    public interface ICallback
    {
        void Call();
    }

    class ActionCallback : ICallback
    {
        public Action action = null;

        public void Call()
        {
            action();
        }
    }
}