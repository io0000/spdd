using System;
using System.Linq;
using System.Collections.Generic;
using watabou.utils;

namespace spdd.levels.rooms.standard
{
    public abstract class StandardRoom : Room
    {
        public class SizeCategory
        {
            public static SizeCategory NORMAL = new SizeCategory(4, 10, 1);
            public static SizeCategory LARGE = new SizeCategory(10, 14, 2);
            public static SizeCategory GIANT = new SizeCategory(14, 18, 3);

            public int minDim, maxDim;
            public int roomValue;

            public SizeCategory(int min, int max, int val)
            {
                minDim = min;
                maxDim = max;
                roomValue = val;
            }

            public int ConnectionWeight()
            {
                return roomValue * roomValue;
            }

            public static IEnumerable<SizeCategory> Values()
            {
                yield return NORMAL;
                yield return LARGE;
                yield return GIANT;
            }
        }

        public SizeCategory sizeCat;

        public StandardRoom()
        {
            SetSizeCat();
        }

        //Note that if a room wishes to allow itself to be forced to a certain size category,
        //but would (effectively) never roll that size category, consider using Float.MIN_VALUE
        public virtual float[] SizeCatProbs()
        {
            //always normal by default
            return new float[] { 1, 0, 0 };
        }

        public bool SetSizeCat()
        {
            return SetSizeCat(0, SizeCategory.Values().Count() - 1);
        }

        //assumes room value is always ordinal+1
        public bool SetSizeCat(int maxRoomValue)
        {
            return SetSizeCat(0, maxRoomValue - 1);
        }

        //returns false if size cannot be set
        public bool SetSizeCat(int minOrdinal, int maxOrdinal)
        {
            float[] probs = SizeCatProbs();
            var categories = SizeCategory.Values();

            if (probs.Length != categories.Count())
                return false;

            for (int i = 0; i < minOrdinal; ++i)
                probs[i] = 0;
            for (int i = maxOrdinal + 1; i < categories.Count(); ++i)
                probs[i] = 0;

            int ordinal = Rnd.Chances(probs);

            if (ordinal != -1)
            {
                sizeCat = categories.ElementAt(ordinal);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int MinWidth()
        {
            return sizeCat.minDim;
        }

        public override int MaxWidth()
        {
            return sizeCat.maxDim;
        }

        public override int MinHeight()
        {
            return sizeCat.minDim;
        }

        public override int MaxHeight()
        {
            return sizeCat.maxDim;
        }

        //FIXME this is a very messy way of handing variable standard rooms
        private static List<Type> rooms = new List<Type>();

        static StandardRoom()
        {
            rooms.Add(typeof(EmptyRoom));

            rooms.Add(typeof(SewerPipeRoom));
            rooms.Add(typeof(RingRoom));

            rooms.Add(typeof(SegmentedRoom));
            rooms.Add(typeof(StatuesRoom));

            rooms.Add(typeof(CaveRoom));
            rooms.Add(typeof(CirclePitRoom));

            rooms.Add(typeof(HallwayRoom));
            rooms.Add(typeof(PillarsRoom));

            rooms.Add(typeof(RuinsRoom));
            rooms.Add(typeof(SkullsRoom));

            rooms.Add(typeof(PlantsRoom));
            rooms.Add(typeof(AquariumRoom));
            rooms.Add(typeof(PlatformRoom));
            rooms.Add(typeof(BurnedRoom));
            rooms.Add(typeof(FissureRoom));
            rooms.Add(typeof(GrassyGraveRoom));
            rooms.Add(typeof(StripedRoom));
            rooms.Add(typeof(StudyRoom));
            rooms.Add(typeof(SuspiciousChestRoom));
            rooms.Add(typeof(MinefieldRoom));

            chances[1] = new float[] { 20, 15, 5, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 0 };
            chances[2] = new float[] { 20, 15, 5, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            chances[4] = chances[3] = chances[2];
            chances[5] = new float[] { 20, 15, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            chances[6] = new float[] { 20, 0, 0, 15, 5, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            chances[10] = chances[9] = chances[8] = chances[7] = chances[6];

            chances[11] = new float[] { 20, 0, 0, 0, 0, 15, 5, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            chances[15] = chances[14] = chances[13] = chances[12] = chances[11];

            chances[16] = new float[] { 20, 0, 0, 0, 0, 0, 0, 15, 5, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            chances[20] = chances[19] = chances[18] = chances[17] = chances[16];

            chances[21] = new float[] { 20, 0, 0, 0, 0, 0, 0, 0, 0, 15, 5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            chances[26] = chances[25] = chances[24] = chances[23] = chances[22] = chances[21];
        }

        private static float[][] chances = new float[27][];

        public static StandardRoom CreateRoom()
        {
            int index = Rnd.Chances(chances[Dungeon.depth]);
            return (StandardRoom)Reflection.NewInstance(rooms[index]);
        }
    }
}