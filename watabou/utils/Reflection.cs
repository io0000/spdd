using System;
using System.Linq;
using System.Reflection;
using watabou.noosa;

namespace watabou.utils
{
    public class SPDStaticAttribute : Attribute
    { }

    //wrapper for LibGDX reflection
    public class Reflection
    {
        public static bool IsMemberClass(Type type)
        {
            //return ClassReflection.isMemberClass(cls);
            return type.IsNested;
        }

        // c#은 java와 달리 static inner classes가 없음, 대안으로 attribute를 사용 
        public static bool IsStatic(Type type)
        {
            // return ClassReflection.isStaticClass(cls);
            var result = type.GetCustomAttributes<SPDStaticAttribute>().Any();
            return result;
        }

        public static object NewInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                Game.ReportException(e);
                return null;
            }
        }

        public static object NewInstanceUnhandled(Type type)
        {
            return Activator.CreateInstance(type);
        }

        public static Type ForName(string name)
        {
            try
            {
                return Type.GetType(name, true);    // true - throw
            }
            catch (Exception e)
            {
                Game.ReportException(e);
                return null;
            }
        }

        //public static Type ForNameUnhandled(string name)
        //{
        //    return Type.GetType(name, true);
        //}
    }
}