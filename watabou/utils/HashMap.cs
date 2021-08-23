using System.Collections.Generic;

namespace watabou.utils
{
    public class HashMap<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public TValue Get(TKey key)
        {
            if (TryGetValue(key, out TValue value))
                return value;
            else
                return default(TValue);
        }
    }
}