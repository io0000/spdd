using System;

namespace spdd.utils
{
    public class BArray
    {
        private static bool[] falseArray;

        public static void SetFalse(bool[] toBeFalse)
        {
            if (falseArray == null || falseArray.Length < toBeFalse.Length)
                falseArray = new bool[toBeFalse.Length];

            Array.Copy(falseArray, 0, toBeFalse, 0, toBeFalse.Length);
        }

        public static bool[] And(bool[] a, bool[] b, bool[] result)
        {
            int length = a.Length;

            if (result == null)
                result = new bool[length];

            for (int i = 0; i < length; ++i)
                result[i] = a[i] && b[i];

            return result;
        }

        public static bool[] Or(bool[] a, bool[] b, bool[] result)
        {
            return Or(a, b, 0, a.Length, result);
        }

        public static bool[] Or(bool[] a, bool[] b, int offset, int length, bool[] result)
        {
            if (result == null)
                result = new bool[length];

            for (int i = offset; i < offset + length; ++i)
                result[i] = a[i] || b[i];

            return result;
        }

        public static bool[] Not(bool[] a, bool[] result)
        {
            int length = a.Length;

            if (result == null)
                result = new bool[length];

            for (int i = 0; i < length; ++i)
                result[i] = !a[i];

            return result;
        }

        public static bool[] Is(int[] a, bool[] result, int v1)
        {
            int length = a.Length;

            if (result == null)
                result = new bool[length];

            for (int i = 0; i < length; ++i)
                result[i] = (a[i] == v1);

            return result;
        }

        /*
        
        public static boolean[] isOneOf( int[] a, boolean[] result, int... v ) {
        
            int length = a.length;
            int nv = v.length;
        
            if (result == null) {
                result = new boolean[length];
            }
        
            for (int i=0; i < length; i++) {
                result[i] = false;
                for (int j=0; j < nv; j++) {
                    if (a[i] == v[j]) {
                        result[i] = true;
                        break;
                    }
                }
            }
        
            return result;
        }
    
        public static boolean[] isNot( int[] a, boolean[] result, int v1 ) {
        
            int length = a.length;
        
            if (result == null) {
                result = new boolean[length];
            }
        
            for (int i=0; i < length; i++) {
                result[i] = a[i] != v1;
            }
        
            return result;
        }
    
        public static boolean[] isNotOneOf( int[] a, boolean[] result, int... v ) {
        
            int length = a.length;
            int nv = v.length;
        
            if (result == null) {
                result = new boolean[length];
            }
        
            for (int i=0; i < length; i++) {
                result[i] = true;
                for (int j=0; j < nv; j++) {
                    if (a[i] == v[j]) {
                        result[i] = false;
                        break;
                    }
                }
            }
        
            return result;
        }
        */
    }
}