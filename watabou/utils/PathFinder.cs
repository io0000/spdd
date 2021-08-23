using System;
using System.Collections.Generic;

namespace watabou.utils
{
    public class PathFinder
    {
        public static int[] distance;
        private static int[] maxVal;

        private static bool[] goals;
        private static int[] queue;

        private static int size;
        private static int width;

        private static int[] dir;
        private static int[] dirLR;

        //performance-light shortcuts for some common pathfinder cases
        //they are in array-access order for increased memory performance
        public static int[] NEIGHBORS4;
        public static int[] NEIGHBORS8;
        public static int[] NEIGHBORS9;

        //similar to their equivalent neighbor arrays, but the order is clockwise.
        //Useful for some logic functions, but is slower due to lack of array-access order.
        public static int[] CIRCLE4;
        public static int[] CIRCLE8;

        public static void SetMapSize(int width, int height)
        {
            PathFinder.width = width;
            PathFinder.size = width * height;

            distance = new int[size];
            goals = new bool[size];
            queue = new int[size];

            maxVal = new int[size];
            //for (int i = 0; i < size; ++i)
            //    maxVal[i] = int.MaxValue;
            Array.Fill(maxVal, int.MaxValue);

            dir = new int[] { -1, +1, -width, +width, -width - 1, -width + 1, +width - 1, +width + 1 };
            dirLR = new int[] { -1 - width, -1, -1 + width, -width, +width, +1 - width, +1, +1 + width };

            NEIGHBORS4 = new int[] { -width, -1, +1, +width };
            NEIGHBORS8 = new int[] { -width - 1, -width, -width + 1, -1, +1, +width - 1, +width, +width + 1 };
            NEIGHBORS9 = new int[] { -width - 1, -width, -width + 1, -1, 0, +1, +width - 1, +width, +width + 1 };

            CIRCLE4 = new int[] { -width, +1, +width, -1 };
            CIRCLE8 = new int[] { -width - 1, -width, -width + 1, +1, +width + 1, +width, +width - 1, -1 };
        }

        public static Path Find(int from, int to, bool[] passable)
        {
            if (!BuildDistanceMap(from, to, passable))
                return null;

            Path result = new Path();
            int s = from;

            // From the starting position we are moving downwards, 
            // until we reach the ending point
            do
            {
                int minD = distance[s];
                int mins = s;

                for (int i = 0; i < dir.Length; ++i)
                {
                    int n = s + dir[i];

                    int thisD = distance[n];
                    if (thisD < minD)
                    {
                        minD = thisD;
                        mins = n;
                    }
                }
                s = mins;
                result.Add(s);
            }
            while (s != to);

            return result;
        }

        public static int GetStep(int from, int to, bool[] passable)
        {
            if (!BuildDistanceMap(from, to, passable))
                return -1;

            // From the starting position we are making one step downwards
            int minD = distance[from];
            int best = from;

            int step, stepD;

            for (int i = 0; i < dir.Length; ++i)
            {
                if ((stepD = distance[step = from + dir[i]]) < minD)
                {
                    minD = stepD;
                    best = step;
                }
            }

            return best;
        }

        public static int GetStepBack(int cur, int from, bool[] passable)
        {
            var d = BuildEscapeDistanceMap(cur, from, 5, passable);
            for (var i = 0; i < size; ++i)
                goals[i] = distance[i] == d;

            if (!BuildDistanceMap(cur, goals, passable))
                return -1;

            int s = cur;

            // From the starting position we are making one step downwards
            int minD = distance[s];
            int mins = s;

            for (int i = 0; i < dir.Length; ++i)
            {
                int n = s + dir[i];
                int thisD = distance[n];

                if (thisD < minD)
                {
                    minD = thisD;
                    mins = n;
                }
            }

            return mins;
        }

        private static bool BuildDistanceMap(int from, int to, bool[] passable)
        {
            if (from == to)
                return false;

            Array.Copy(maxVal, 0, distance, 0, maxVal.Length);

            var pathFound = false;

            int head = 0;
            int tail = 0;

            // Add to queue
            queue[tail++] = to;
            distance[to] = 0;

            while (head < tail)
            {
                // Remove from queue
                var step = queue[head++];
                if (step == from)
                {
                    pathFound = true;
                    break;
                }

                var nextDistance = distance[step] + 1;

                int start = (step % width == 0 ? 3 : 0);
                int end = ((step + 1) % width == 0 ? 3 : 0);
                for (int i = start; i < dirLR.Length - end; ++i)
                {
                    int n = step + dirLR[i];
                    if (n == from || (n >= 0 && n < size && passable[n] && (distance[n] > nextDistance)))
                    {
                        // Add to queue
                        queue[tail++] = n;
                        distance[n] = nextDistance;
                    }
                }
            }

            return pathFound;
        }

        public static void BuildDistanceMap(int to, bool[] passable, int limit)
        {
            Array.Copy(maxVal, 0, distance, 0, maxVal.Length);

            int head = 0;
            int tail = 0;

            // Add to queue
            queue[tail++] = to;
            distance[to] = 0;

            while (head < tail)
            {
                // Remove from queue
                var step = queue[head++];

                var nextDistance = distance[step] + 1;
                if (nextDistance > limit)
                    return;

                int start = (step % width == 0 ? 3 : 0);
                int end = ((step + 1) % width == 0 ? 3 : 0);
                for (int i = start; i < dirLR.Length - end; ++i)
                {
                    int n = step + dirLR[i];
                    if (n >= 0 && n < size && passable[n] && (distance[n] > nextDistance))
                    {
                        // Add to queue
                        queue[tail++] = n;
                        distance[n] = nextDistance;
                    }
                }
            }
        }

        private static bool BuildDistanceMap(int from, bool[] to, bool[] passable)
        {
            if (to[from])
                return false;

            Array.Copy(maxVal, 0, distance, 0, maxVal.Length);

            var pathFound = false;

            int head = 0;
            int tail = 0;

            // Add to queue
            for (int i = 0; i < size; ++i)
            {
                if (to[i])
                {
                    queue[tail++] = i;
                    distance[i] = 0;
                }
            }

            while (head < tail)
            {
                // Remove from queue
                var step = queue[head++];
                if (step == from)
                {
                    pathFound = true;
                    break;
                }
                int nextDistance = distance[step] + 1;

                int start = (step % width == 0 ? 3 : 0);
                int end = ((step + 1) % width == 0 ? 3 : 0);
                for (int i = start; i < dirLR.Length - end; ++i)
                {
                    int n = step + dirLR[i];
                    if (n == from || (n >= 0 && n < size && passable[n] && (distance[n] > nextDistance)))
                    {
                        // Add to queue
                        queue[tail++] = n;
                        distance[n] = nextDistance;
                    }
                }
            }

            return pathFound;
        }

        private static int BuildEscapeDistanceMap(int cur, int from, int lookAhead, bool[] passable)
        {
            Array.Copy(maxVal, 0, distance, 0, maxVal.Length);

            int destDist = int.MaxValue;

            int head = 0;
            int tail = 0;

            // Add to queue
            queue[tail++] = from;
            distance[from] = 0;

            int dist = 0;

            while (head < tail)
            {
                // Remove from queue
                var step = queue[head++];
                dist = distance[step];

                if (dist > destDist)
                    return destDist;

                if (step == cur)
                    destDist = dist + lookAhead;

                int nextDistance = dist + 1;

                int start = (step % width == 0 ? 3 : 0);
                int end = ((step + 1) % width == 0 ? 3 : 0);
                for (int i = start; i < dirLR.Length - end; ++i)
                {
                    int n = step + dirLR[i];
                    if (n >= 0 && n < size && passable[n] && distance[n] > nextDistance)
                    {
                        // Add to queue
                        queue[tail++] = n;
                        distance[n] = nextDistance;
                    }
                }
            }

            return dist;
        }

        public static void BuildDistanceMap(int to, bool[] passable)
        {
            Array.Copy(maxVal, 0, distance, 0, maxVal.Length);

            int head = 0;
            int tail = 0;

            // Add to queue
            queue[tail++] = to;
            distance[to] = 0;

            while (head < tail)
            {
                // Remove from queue
                int step = queue[head++];
                int nextDistance = distance[step] + 1;

                int start = (step % width == 0 ? 3 : 0);
                int end = ((step + 1) % width == 0 ? 3 : 0);
                for (int i = start; i < dirLR.Length - end; ++i)
                {
                    int n = step + dirLR[i];
                    if (n >= 0 && n < size && passable[n] && (distance[n] > nextDistance))
                    {
                        // Add to queue
                        queue[tail++] = n;
                        distance[n] = nextDistance;
                    }
                }
            }
        }

        public class Path : List<int>
        { }
    }
}