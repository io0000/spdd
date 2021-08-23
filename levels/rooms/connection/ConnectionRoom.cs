using System;
using System.Collections.Generic;
using watabou.utils;

namespace spdd.levels.rooms.connection
{
    public abstract class ConnectionRoom : Room
    {
        public override int MinWidth()
        {
            return 3;
        }
        public override int MaxWidth()
        {
            return 10;
        }

        public override int MinHeight()
        {
            return 3;
        }
        public override int MaxHeight()
        {
            return 10;
        }

        public override int MinConnections(int direction)
        {
            if (direction == ALL)
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }

        public override bool CanPlaceTrap(Point p)
        {
            //traps cannot appear in connection rooms on floor 1
            return base.CanPlaceTrap(p) && Dungeon.depth > 1;
        }

        //FIXME this is a very messy way of handing variable connection rooms
        private static List<Type> rooms = new List<Type>();

        static ConnectionRoom()
        {
            rooms.Add(typeof(TunnelRoom));
            rooms.Add(typeof(BridgeRoom));

            rooms.Add(typeof(PerimeterRoom));
            rooms.Add(typeof(WalkwayRoom));

            rooms.Add(typeof(RingTunnelRoom));
            rooms.Add(typeof(RingBridgeRoom));

            chances[1] = new float[] { 20, 1, 0, 2, 2, 1 };
            chances[4] = chances[3] = chances[2] = chances[1];
            chances[5] = new float[] { 20, 0, 0, 0, 0, 0 };

            chances[6] = new float[] { 0, 0, 22, 3, 0, 0 };
            chances[10] = chances[9] = chances[8] = chances[7] = chances[6];

            chances[11] = new float[] { 12, 0, 0, 5, 5, 3 };
            chances[15] = chances[14] = chances[13] = chances[12] = chances[11];

            chances[16] = new float[] { 0, 0, 18, 3, 3, 1 };
            chances[20] = chances[19] = chances[18] = chances[17] = chances[16];

            chances[21] = chances[5];

            chances[22] = new float[] { 15, 4, 0, 2, 3, 2 };
            chances[26] = chances[25] = chances[24] = chances[23] = chances[22];
        }

        private static float[][] chances = new float[27][];

        public static ConnectionRoom CreateRoom()
        {
            return (ConnectionRoom)Reflection.NewInstance(rooms[Rnd.Chances(chances[Dungeon.depth])]);
        }
    }

}