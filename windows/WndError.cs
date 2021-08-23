using spdd.ui;
using spdd.messages;

namespace spdd.windows
{
    public class WndError : WndTitledMessage
    {
        public WndError(string message)
            : base(Icons.WARNING.Get(), Messages.Get(typeof(WndError), "title"), message)
        { }
    }
}