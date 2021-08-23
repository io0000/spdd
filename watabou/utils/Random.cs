using System;
using System.Collections.Generic;
using System.Linq;
using watabou.noosa;

namespace watabou.utils
{
    public class Rnd
    {
        private static Stack<Random> generators;
        //private static Random random = new Random();

        static Rnd()
        {
            ResetGenerators();
        }

        public static void ResetGenerators()
        {
            generators = new Stack<Random>();
            generators.Push(new Random());
        }

        public static void PushGenerator()
        {
            generators.Push(new Random());
        }

        public static void PushGenerator(int seed)
        {
            generators.Push(new Random(seed));
        }

        public static void PopGenerator()
        {
            if (generators.Count == 1)
            {
                Game.ReportException(new Exception("tried to pop the last random number generator!"));
            }
            else
            {
                generators.Pop();
            }
        }

        public static Random Peek()
        {
            return generators.Peek();
        }

        //returns a uniformly distributed float in the range [0, 1)
        public static float Float()
        {
            return (float)Peek().NextDouble();
        }

        //returns a uniformly distributed float in the range [0, max)
        public static float Float(float max)
        {
            return (float)(Float() * max);
        }

        //returns a uniformly distributed float in the range [min, max)
        public static float Float(float min, float max)
        {
            return (float)(min + Float() * (max - min));
        }

        //returns a triangularly distributed float in the range [min, max)
        public static float NormalFloat(float min, float max)
        {
            return min + ((Float(max - min) + Float(max - min)) / 2.0f);
        }

        //returns a uniformly distributed int in the range [0, max)
        public static int Int(int max)
        {
            if (max > 0)
            {
                return Peek().Next(max);
            }

            return 0;
        }

        //returns a uniformly distributed int in the range [min, max)
        public static int Int(int min, int max)
        {
            return min + Int(max - min);
        }

        //returns a uniformly distributed int in the range [min, max]
        public static int IntRange(int min, int max)
        {
            return min + Int(max - min + 1);
        }

        //returns a triangularly distributed int in the range [min, max]
        public static int NormalIntRange(int min, int max)
        {
            return min + (int)((Float() + Float()) * (max - min + 1) / 2f);
        }

        /*
        //returns a uniformly distributed long in the range [-2^63, 2^63)
        public static long Long() {
            return generators.peek().nextLong();
        }

        //returns a uniformly distributed long in the range [0, max)
        public static long Long( long max ) {
            long result = Long();
            if (result < 0) result += Long.MAX_VALUE;
            return result % max;
        } 
         */

        //returns an index from chances, the probability of each index is the weight values in changes
        public static int Chances(float[] chances)
        {
            var sum = chances.Sum();

            var value = Float(sum);
            sum = 0;

            for (var i = 0; i < chances.Length; ++i)
            {
                sum += chances[i];
                if (value < sum)
                    return i;
            }

            return -1;
        }

        // returns a key element from chances, 
        // the probability of each key is the weight value it maps to
        public static T Chances<T>(IDictionary<T, float> chances)
        {
            var size = chances.Count;

            var values = chances.Keys.ToArray();
            var probs = new float[size];
            float sum = 0.0f;

            for (var i = 0; i < size; ++i)
            {
                probs[i] = chances[values[i]];
                sum += probs[i];
            }

            if (sum <= 0)
                return default(T);

            float value = Float(sum);

            sum = probs[0];
            for (int i = 0; i < size; ++i)
            {
                if (value < sum)
                    return (T)values[i];

                sum += probs[i + 1];
            }

            return default(T);
        }

        public static int Index<T>(ICollection<T> collection)
        {
            return Int(collection.Count);
        }

        public static T OneOf<T>(params T[] array)
        {
            return array[Int(array.Length)];
        }

        public static T Element<T>(T[] array)
        {
            return Element(array, array.Length);
        }

        public static T Element<T>(T[] array, int max)
        {
            return array[Int(max)];
        }

        public static T Element<T>(IList<T> collection)
        {
            var size = collection.Count;

            return size > 0 ? collection[Int(size)] : default(T);
        }

        public static void Shuffle<T>(IList<T> list)
        {
            //Collections.shuffle(list, generators.peek());
            int n = list.Count;
            while (n > 1)
            {
                --n;

                int k = Int(n + 1);

                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        //public static void Shuffle<T>(T[] array)
        //{
        //    for (var i = 0; i < array.Length - 1; ++i)
        //    {
        //        var j = Int(i, array.Length);
        //
        //        if (j != i)
        //        {
        //            var t = array[i];
        //            array[i] = array[j];
        //            array[j] = t;
        //        }
        //    }
        //}

        //public static void Shuffle<TU, TV>(TU[] u, TV[] v)
        //{
        //    for (var i = 0; i < u.Length - 1; ++i)
        //    {
        //        var j = Int(i, u.Length);
        //        if (j == i)
        //            continue;
        //
        //        var ut = u[i];
        //        u[i] = u[j];
        //        u[j] = ut;
        //
        //        var vt = v[i];
        //        v[i] = v[j];
        //        v[j] = vt;
        //    }
        //}
    }
}