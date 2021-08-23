namespace spdd.levels
{
    public class Terrain
    {
        public const int CHASM = 0;
        public const int EMPTY = 1;
        public const int GRASS = 2;
        public const int EMPTY_WELL = 3;
        public const int WALL = 4;
        public const int DOOR = 5;
        public const int OPEN_DOOR = 6;
        public const int ENTRANCE = 7;
        public const int EXIT = 8;
        public const int EMBERS = 9;
        public const int LOCKED_DOOR = 10;
        public const int PEDESTAL = 11;
        public const int WALL_DECO = 12;
        public const int BARRICADE = 13;
        public const int EMPTY_SP = 14;
        public const int HIGH_GRASS = 15;
        public const int FURROWED_GRASS = 30;

        public const int SECRET_DOOR = 16;
        public const int SECRET_TRAP = 17;
        public const int TRAP = 18;
        public const int INACTIVE_TRAP = 19;

        public const int EMPTY_DECO = 20;
        public const int LOCKED_EXIT = 21;
        public const int UNLOCKED_EXIT = 22;
        public const int SIGN = 23;
        public const int WELL = 24;
        public const int STATUE = 25;
        public const int STATUE_SP = 26;
        public const int BOOKSHELF = 27;
        public const int ALCHEMY = 28;

        public const int WATER = 29;

        public const int PASSABLE = 0x01;
        public const int LOS_BLOCKING = 0x02;
        public const int FLAMABLE = 0x04;
        public const int SECRET = 0x08;
        public const int SOLID = 0x10;
        public const int AVOID = 0x20;
        public const int LIQUID = 0x40;
        public const int PIT = 0x80;

        public static readonly int[] flags = new int[256];
        static Terrain()
        {
            flags[CHASM] = AVOID | PIT;
            flags[EMPTY] = PASSABLE;
            flags[GRASS] = PASSABLE | FLAMABLE;
            flags[EMPTY_WELL] = PASSABLE;
            flags[WATER] = PASSABLE | LIQUID;
            flags[WALL] = LOS_BLOCKING | SOLID;
            flags[DOOR] = PASSABLE | LOS_BLOCKING | FLAMABLE | SOLID;
            flags[OPEN_DOOR] = PASSABLE | FLAMABLE;
            flags[ENTRANCE] = PASSABLE/* | SOLID*/;
            flags[EXIT] = PASSABLE;
            flags[EMBERS] = PASSABLE;
            flags[LOCKED_DOOR] = LOS_BLOCKING | SOLID;
            flags[PEDESTAL] = PASSABLE;
            flags[WALL_DECO] = flags[WALL];
            flags[BARRICADE] = FLAMABLE | SOLID | LOS_BLOCKING;
            flags[EMPTY_SP] = flags[EMPTY];
            flags[HIGH_GRASS] = PASSABLE | LOS_BLOCKING | FLAMABLE;
            flags[FURROWED_GRASS] = flags[HIGH_GRASS];

            flags[SECRET_DOOR] = flags[WALL] | SECRET;
            flags[SECRET_TRAP] = flags[EMPTY] | SECRET;
            flags[TRAP] = AVOID;
            flags[INACTIVE_TRAP] = flags[EMPTY];

            flags[EMPTY_DECO] = flags[EMPTY];
            flags[LOCKED_EXIT] = SOLID;
            flags[UNLOCKED_EXIT] = PASSABLE;
            flags[SIGN] = SOLID; //Currently these are unused except for visual tile overrides where we want terrain to be solid with no other properties
            flags[WELL] = AVOID;
            flags[STATUE] = SOLID;
            flags[STATUE_SP] = flags[STATUE];
            flags[BOOKSHELF] = flags[BARRICADE];
            flags[ALCHEMY] = SOLID;
        }

        public static int Discover(int terr)
        {
            switch (terr)
            {
                case SECRET_DOOR:
                    return DOOR;
                case SECRET_TRAP:
                    return TRAP;
                default:
                    return terr;
            }
        }

        ////removes signs, places floors instead
        //public static int[] ConvertTilesFrom0_6_0b(int[] map)
        //{
        //    for (int i = 0; i < map.Length; ++i)
        //    {
        //        if (map[i] == 23)
        //        {
        //            map[i] = 1;
        //        }
        //    }
        //    return map;
        //}
    }
}