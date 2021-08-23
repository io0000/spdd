using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using watabou.noosa;

// JObject: JSON Object 입니다
// (key, value) pair 들을 가질 수 있습니다.
//    key : string
//    value : JToken

// JToken          - abstract base class
//     JContainer      - abstract base class of JTokens that can contain other JTokens
//         JArray      - represents a JSON array(contains an ordered list of JTokens)
//         JObject     - represents a JSON object (contains a collection of JProperties)
//         JProperty   - represents a JSON property(a name/JToken pair inside a JObject)
//     JValue          - represents a primitive JSON value(string, number, boolean, null)

namespace watabou.utils
{
    public class Bundle
    {
        private const string CLASS_NAME = "__className";
        private const string DEFAULT_KEY = "key";
        //private static HashMap<string, string> aliases = new HashMap<>();
        public JObject data;

        public Bundle()
            : this(new JObject())
        { }

        public override string ToString()
        {
            return data.ToString();
        }

        public Bundle(JObject data)
        {
            this.data = data;
        }

        public bool IsNull()
        {
            return data == null;
        }

        public bool Contains(string key)
        {
            return data.ContainsKey(key);
        }

        public bool GetBoolean(string key)
        {
            var token = data.GetValue(key);
            return (token == null) ? false : token.ToObject<bool>();
        }

        public int GetInt(string key)
        {
            var token = data.GetValue(key);
            return (token == null) ? 0 : token.ToObject<int>();
        }

        public long GetLong(string key)
        {
            var token = data.GetValue(key);
            return (token == null) ? 0L : token.ToObject<long>();
        }

        public float GetFloat(string key)
        {
            var token = data.GetValue(key);
            return (token == null) ? 0.0f : token.ToObject<float>();
        }

        public string GetString(string key)
        {
            var token = data.GetValue(key);
            return (token == null) ? "" : token.ToObject<string>();
        }

        public Type GetClass(string key)
        {
            var clName = GetString(key);
            if (clName != "")
            {
                return Reflection.ForName(clName);
            }
            else
            {
                return null;
            }
        }

        public Bundle GetBundle(string key)
        {
            var token = data.GetValue(key);
            if (token != null && token.Type == JTokenType.Object)
                return new Bundle((JObject)token);

            return new Bundle((JObject)null);
        }

        private IBundlable Get()
        {
            if (data == null)
                return null;

            var clName = GetString(CLASS_NAME);
            //if (aliases.containsKey(clName))
            //{
            //    clName = aliases.get(clName);
            //}

            Type cl = Reflection.ForName(clName);
            //Skip none-static inner classes as they can't be instantiated through bundle restoring
            //Classes which make use of none-static inner classes must manage instantiation manually
            if (cl != null && (!Reflection.IsMemberClass(cl) || Reflection.IsStatic(cl)))
            {
                var obj = (IBundlable)Reflection.NewInstance(cl);
                if (obj != null)
                {
                    obj.RestoreFromBundle(this);
                    return obj;
                }
            }

            return null;
        }

        public IBundlable Get(string key)
        {
            return GetBundle(key).Get();
        }

        public virtual T GetEnum<T>(string key) where T : struct
        {
            try
            {
                return (T)Enum.Parse(typeof(T), GetString(key));
            }
            catch (System.Exception e)
            {
                Game.ReportException(e);
                var values = Enum.GetValues(typeof(T));
                return (T)values.GetValue(0);
            }
        }

        private T[] GetTArray<T>(string key)
        {
            try
            {
                var token = data.GetValue(key);
                if (token == null)
                    return null;

                if (token.Type != JTokenType.Array)
                    return null;

                JArray arr = token as JArray;

                int length = arr.Count;
                T[] result = new T[length];
                for (int i = 0; i < length; ++i)
                {
                    result[i] = arr[i].ToObject<T>();
                }

                return result;
            }
            catch (System.Exception e)
            {
                Game.ReportException(e);
                return null;
            }
        }

        public int[] GetIntArray(string key)
        {
            return GetTArray<int>(key);
        }

        public float[] GetFloatArray(string key)
        {
            return GetTArray<float>(key);
        }

        public bool[] GetBooleanArray(string key)
        {
            return GetTArray<bool>(key);
        }

        public string[] GetStringArray(string key)
        {
            return GetTArray<string>(key);
        }

        public Type[] GetClassArray(string key)
        {
            var arr = GetStringArray(key);
            Type[] result = new Type[arr.Length];
            for (int i = 0; i < arr.Length; ++i)
            {
                Type type = Reflection.ForName(arr[i]);
                result[i] = type;
            }
            return result;
        }

        public Bundle[] GetBundleArray()
        {
            return GetBundleArray(DEFAULT_KEY);
        }

        public Bundle[] GetBundleArray(string key)
        {
            try
            {
                var token = data.GetValue(key);
                if (token == null)
                    return null;

                if (token.Type != JTokenType.Array)
                    return null;

                JArray arr = token as JArray;

                int length = arr.Count;
                Bundle[] result = new Bundle[length];
                for (int i = 0; i < length; ++i)
                {
                    JToken value = arr[i];
                    result[i] = new Bundle((JObject)value);
                }

                return result;
            }
            catch (System.Exception e)
            {
                Game.ReportException(e);
                return null;
            }
        }

        public ICollection<IBundlable> GetCollection(string key)
        {
            var list = new List<IBundlable>();

            try
            {
                var token = data.GetValue(key);
                if (token == null)
                    return list;

                if (token.Type != JTokenType.Array)
                    return list;

                JArray arr = token as JArray;
                int length = arr.Count;

                for (int i = 0; i < length; ++i)
                {
                    JToken value = arr[i];
                    var bundle = new Bundle((JObject)value);
                    list.Add(bundle.Get());  // RestoreFromBundle가 호출됨
                }
            }
            catch (System.Exception e)
            {
                Game.ReportException(e);
            }

            return list;
        }

        ///
        /// Put
        ///

        public void Put(string key, bool value)
        {
            data[key] = value;
        }

        public void Put(string key, int value)
        {
            data[key] = value;
        }

        public void Put(string key, float value)
        {
            data[key] = value;
        }

        public void Put(string key, string value)
        {
            data[key] = value;
        }

        public void Put(string key, Type value)
        {
            if (value != null)                     // null일 수 있음
                data[key] = value.FullName;
        }

        public void Put(string key, Bundle bundle)
        {
            data[key] = bundle.data;
        }

        public void Put(string key, IBundlable bundlable)
        {
            if (bundlable == null)
                return;

            var bundle = new Bundle();
            bundle.Put(CLASS_NAME, bundlable.GetType().FullName);
            bundlable.StoreInBundle(bundle);
            data[key] = bundle.data;
        }

        public void Put(string key, int[] arr)
        {
            data.Add(key, JArray.FromObject(arr));
        }

        public void Put(string key, float[] arr)
        {
            data.Add(key, JArray.FromObject(arr));
        }

        public void Put(string key, bool[] arr)
        {
            data.Add(key, JArray.FromObject(arr));
        }

        public void Put(string key, string[] arr)
        {
            data.Add(key, JArray.FromObject(arr));
        }

        public void Put(string key, Type[] arr)
        {
            int length = arr.Length;
            string[] strArr = new string[length];
            for (int i = 0; i < length; ++i)
                strArr[i] = arr[i].FullName;

            Put(key, strArr);
        }

        public void Put<T>(string key, ICollection<T> collection) where T : IBundlable
        {
            var array = new JArray();

            foreach (var obj in collection)
            {
                //Skip none-static inner classes as they can't be instantiated through bundle restoring
                //Classes which make use of none-static inner classes must manage instantiation manually
                if (obj != null)
                {
                    Type cl = obj.GetType();
                    if ((!Reflection.IsMemberClass(cl) || Reflection.IsStatic(cl)))
                    {
                        var bundle = new Bundle();
                        bundle.Put(CLASS_NAME, cl.FullName);
                        obj.StoreInBundle(bundle);
                        array.Add(bundle.data);
                    }
                }
            }

            data[key] = array;
        }

        //public static Bundle Read(Stream stream)
        //{
        //    using (var sr = new StreamReader(stream))
        //    {
        //        string str = sr.ReadToEnd();
        //        JObject obj = JObject.Parse(str);
        //        return new Bundle(obj);
        //    }
        //}

        //public static Bundle Read(byte[] bytes)
        //{
        //    string str = System.Text.Encoding.UTF8.GetString(bytes);
        //    JObject obj = JObject.Parse(str);
        //    return new Bundle(obj);
        //}

        //public static bool Write(Bundle bundle, Stream stream)
        //{
        //    using (var writer = new StreamWriter(stream))
        //    {
        //        writer.Write(bundle.data.ToString());
        //        //writer.Write(bundle.data.ToString(Formatting.None));
        //        writer.Close();
        //    }
        //
        //    return true;
        //}
    }
}