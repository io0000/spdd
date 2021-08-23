using System.Collections.Generic;

namespace watabou.utils
{
    public class SparseArray<T> : Dictionary<int, T>
    {
        public T Get(int key, T defaultValue)
        {
            if (TryGetValue(key, out T value))
                return value;
            else
                return defaultValue;
        }

        public new T this[int key]
        {
            get
            {
                if (TryGetValue(key, out T value))
                    return value;
                else
                    return default(T);
            }
            set
            {
                base[key] = value;
            }
        }
    }
}