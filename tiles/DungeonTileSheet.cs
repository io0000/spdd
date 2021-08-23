using System.Collections.Generic;
using watabou.utils;
using spdd.levels;

namespace spdd.tiles
{
    public class DungeonTileSheet
    {
        private const int WIDTH = 16;

        private static int XY(int x, int y)
        {
            x -= 1;
            y -= 1;
            return x + WIDTH * y;
        }

        //used in cases like map-edge decision making.
        public const int NULL_TILE = -1;

        ///********************************************************************
        /// Floor Tiles
        /// *********************************************************************

        private static readonly int GROUND = XY(1, 1); //32 slots
        public static readonly int FLOOR = GROUND + 0;
        public static readonly int FLOOR_DECO = GROUND + 1;
        public static readonly int GRASS = GROUND + 2;
        public static readonly int EMBERS = GROUND + 3;
        public static readonly int FLOOR_SP = GROUND + 4;

        public static readonly int FLOOR_ALT_1 = GROUND + 6;
        public static readonly int FLOOR_DECO_ALT = GROUND + 7;
        public static readonly int GRASS_ALT = GROUND + 8;
        public static readonly int EMBERS_ALT = GROUND + 9;
        public static readonly int FLOOR_SP_ALT = GROUND + 10;

        public static readonly int FLOOR_ALT_2 = GROUND + 12;

        public static readonly int ENTRANCE = GROUND + 16;
        public static readonly int EXIT = GROUND + 17;
        public static readonly int WELL = GROUND + 18;
        public static readonly int EMPTY_WELL = GROUND + 19;
        public static readonly int PEDESTAL = GROUND + 20;

        ///********************************************************************
        /// Water Tiles
        /// *********************************************************************

        public static readonly int WATER = XY(1, 3); //16 slots
        //next 15 slots are all water stitching with ground.

        //These tiles can stitch with water
        public static HashSet<int> waterStitcheable = new HashSet<int>
        {
            Terrain.EMPTY, Terrain.GRASS, Terrain.EMPTY_WELL,
            Terrain.ENTRANCE, Terrain.EXIT, Terrain.EMBERS,
            Terrain.BARRICADE, Terrain.HIGH_GRASS, Terrain.FURROWED_GRASS, Terrain.SECRET_TRAP,
            Terrain.TRAP, Terrain.INACTIVE_TRAP, Terrain.EMPTY_DECO,
            Terrain.SIGN, Terrain.WELL, Terrain.STATUE, Terrain.ALCHEMY,
            Terrain.DOOR, Terrain.OPEN_DOOR, Terrain.LOCKED_DOOR
        };

        //+1 for ground above, +2 for ground right, +4 for ground below, +8 for ground left.
        public static int StitchWaterTile(int top, int right, int bottom, int left)
        {
            int result = WATER;
            if (waterStitcheable.Contains(top))
                result += 1;

            if (waterStitcheable.Contains(right))
                result += 2;

            if (waterStitcheable.Contains(bottom))
                result += 4;

            if (waterStitcheable.Contains(left))
                result += 8;

            return result;
        }

        public static bool FloorTile(int tile)
        {
            return tile == Terrain.WATER || directVisuals.Get(tile, CHASM) < CHASM;
        }

        ///********************************************************************
        /// Chasm Tiles
        /// *********************************************************************

        public static readonly int CHASM = XY(1, 4); //16 tiles
        //chasm stitching visuals...
        public static readonly int CHASM_FLOOR = CHASM + 1;
        public static readonly int CHASM_FLOOR_SP = CHASM + 2;
        public static readonly int CHASM_WALL = CHASM + 3;
        public static readonly int CHASM_WATER = CHASM + 4;

        //tiles that can stitch with chasms (from above), and which visual represents the stitching
        public static SparseArray<int> chasmStitcheable = new SparseArray<int>();

        static DungeonTileSheet()
        {
            InitChasmStitcheable();
            InitDirectVisuals();
            InitDirectFlatVisuals();
            InitCommonAltVisuals();
            InitRareAltVisuals();
        }

        static void InitChasmStitcheable()
        {
            //floor
            chasmStitcheable.Add(Terrain.EMPTY, CHASM_FLOOR);
            chasmStitcheable.Add(Terrain.GRASS, CHASM_FLOOR);
            chasmStitcheable.Add(Terrain.EMBERS, CHASM_FLOOR);
            chasmStitcheable.Add(Terrain.EMPTY_WELL, CHASM_FLOOR);
            chasmStitcheable.Add(Terrain.HIGH_GRASS, CHASM_FLOOR);
            chasmStitcheable.Add(Terrain.FURROWED_GRASS, CHASM_FLOOR);
            chasmStitcheable.Add(Terrain.EMPTY_DECO, CHASM_FLOOR);
            chasmStitcheable.Add(Terrain.SIGN, CHASM_FLOOR);
            //chasmStitcheable.Add(Terrain.EMPTY_WELL, CHASM_FLOOR);    코드중복( 같은 key + 같은 value )
            chasmStitcheable.Add(Terrain.WELL, CHASM_FLOOR);
            chasmStitcheable.Add(Terrain.STATUE, CHASM_FLOOR);
            chasmStitcheable.Add(Terrain.SECRET_TRAP, CHASM_FLOOR);
            chasmStitcheable.Add(Terrain.INACTIVE_TRAP, CHASM_FLOOR);
            chasmStitcheable.Add(Terrain.TRAP, CHASM_FLOOR);
            chasmStitcheable.Add(Terrain.BOOKSHELF, CHASM_FLOOR);
            chasmStitcheable.Add(Terrain.BARRICADE, CHASM_FLOOR);
            chasmStitcheable.Add(Terrain.PEDESTAL, CHASM_FLOOR);

            //special floor
            chasmStitcheable.Add(Terrain.EMPTY_SP, CHASM_FLOOR_SP);
            chasmStitcheable.Add(Terrain.STATUE_SP, CHASM_FLOOR_SP);

            //wall
            chasmStitcheable.Add(Terrain.WALL, CHASM_WALL);
            chasmStitcheable.Add(Terrain.DOOR, CHASM_WALL);
            chasmStitcheable.Add(Terrain.OPEN_DOOR, CHASM_WALL);
            chasmStitcheable.Add(Terrain.LOCKED_DOOR, CHASM_WALL);
            chasmStitcheable.Add(Terrain.SECRET_DOOR, CHASM_WALL);
            chasmStitcheable.Add(Terrain.WALL_DECO, CHASM_WALL);

            //water
            chasmStitcheable.Add(Terrain.WATER, CHASM_WATER);
        }

        public static int StitchChasmTile(int above)
        {
            return chasmStitcheable.Get(above, CHASM);
        }

        /**********************************************************************
         Flat Tiles
         **********************************************************************/
        private static readonly int FLAT_WALLS = XY(1, 5);   //16 slots
        public static readonly int FLAT_WALL = FLAT_WALLS + 0;
        public static readonly int FLAT_WALL_DECO = FLAT_WALLS + 1;
        public static readonly int FLAT_BOOKSHELF = FLAT_WALLS + 2;

        public static readonly int FLAT_WALL_ALT = FLAT_WALLS + 4;
        public static readonly int FLAT_WALL_DECO_ALT = FLAT_WALLS + 5;
        public static readonly int FLAT_BOOKSHELF_ALT = FLAT_WALLS + 6;

        private static readonly int FLAT_DOORS = XY(1, 6);   //16 slots
        public static readonly int FLAT_DOOR = FLAT_DOORS + 0;
        public static readonly int FLAT_DOOR_OPEN = FLAT_DOORS + 1;
        public static readonly int FLAT_DOOR_LOCKED = FLAT_DOORS + 2;
        public static readonly int UNLOCKED_EXIT = FLAT_DOORS + 3;
        public static readonly int LOCKED_EXIT = FLAT_DOORS + 4;

        public static readonly int FLAT_OTHER = XY(1, 7);   //16 slots
        public static readonly int FLAT_SIGN = FLAT_OTHER + 0;
        public static readonly int FLAT_STATUE = FLAT_OTHER + 1;
        public static readonly int FLAT_STATUE_SP = FLAT_OTHER + 2;
        public static readonly int FLAT_ALCHEMY_POT = FLAT_OTHER + 3;
        public static readonly int FLAT_BARRICADE = FLAT_OTHER + 4;
        public static readonly int FLAT_HIGH_GRASS = FLAT_OTHER + 5;
        public static readonly int FLAT_FURROWED_GRASS = FLAT_OTHER + 6;

        public static readonly int FLAT_HIGH_GRASS_ALT = FLAT_OTHER + 8;
        public static readonly int FLAT_FURROWED_ALT = FLAT_OTHER + 9;

        /**********************************************************************
         * Raised Tiles, Lower Layer
         **********************************************************************/

        private static readonly int RAISED_WALLS = XY(1, 8);   //32 slots
        //+1 for open to the right, +2 for open to the left
        public static readonly int RAISED_WALL = RAISED_WALLS + 0;
        public static readonly int RAISED_WALL_DECO = RAISED_WALLS + 4;
        //wall that appears behind a top/bottom doorway
        public static readonly int RAISED_WALL_DOOR = RAISED_WALLS + 8;
        public static readonly int RAISED_WALL_BOOKSHELF = RAISED_WALLS + 12;

        public static readonly int RAISED_WALL_ALT = RAISED_WALLS + 16;
        public static readonly int RAISED_WALL_DECO_ALT = RAISED_WALLS + 20;
        public static readonly int RAISED_WALL_BOOKSHELF_ALT = RAISED_WALLS + 28;

        //we use an array instead of a collection because the small element count
        // makes array traversal much faster than something like HashSet.contains.

        //These tiles count as wall for the purposes of wall stitching
        private static int[] wallStitcheable = new int[]{
            Terrain.WALL, Terrain.WALL_DECO, Terrain.SECRET_DOOR,
            Terrain.LOCKED_EXIT, Terrain.UNLOCKED_EXIT, Terrain.BOOKSHELF, NULL_TILE
        };

        public static bool WallStitcheable(int tile)
        {
            foreach (int i in wallStitcheable)
            {
                if (tile == i)
                    return true;
            }
            return false;
        }

        public static int GetRaisedWallTile(int tile, int pos, int right, int below, int left)
        {
            int result;

            if (below == -1 || WallStitcheable(below))
                return -1;
            else if (DoorTile(below))
                result = RAISED_WALL_DOOR;
            else if (tile == Terrain.WALL || tile == Terrain.SECRET_DOOR)
                result = RAISED_WALL;
            else if (tile == Terrain.WALL_DECO)
                result = RAISED_WALL_DECO;
            else if (tile == Terrain.BOOKSHELF)
                result = RAISED_WALL_BOOKSHELF;
            else
                return -1;

            result = GetVisualWithAlts(result, pos);

            if (!WallStitcheable(right))
                result += 1;
            if (!WallStitcheable(left))
                result += 2;
            return result;
        }

        private static readonly int RAISED_DOORS = XY(1, 10);  //16 slots
        public static readonly int RAISED_DOOR = RAISED_DOORS + 0;
        public static readonly int RAISED_DOOR_OPEN = RAISED_DOORS + 1;
        public static readonly int RAISED_DOOR_LOCKED = RAISED_DOORS + 2;
        //floor tile that appears on a top/bottom doorway
        public static readonly int RAISED_DOOR_SIDEWAYS = RAISED_DOORS + 3;

        public static int GetRaisedDoorTile(int tile, int below)
        {
            if (WallStitcheable(below)) 
                return RAISED_DOOR_SIDEWAYS;
            else if (tile == Terrain.DOOR) 
                return DungeonTileSheet.RAISED_DOOR;
            else if (tile == Terrain.OPEN_DOOR) 
                return DungeonTileSheet.RAISED_DOOR_OPEN;
            else if (tile == Terrain.LOCKED_DOOR) 
                return DungeonTileSheet.RAISED_DOOR_LOCKED;
            else 
                return -1;
        }

        private static int[] doorTiles = new int[] 
        {
            Terrain.DOOR, Terrain.LOCKED_DOOR, Terrain.OPEN_DOOR
        };

        public static bool DoorTile(int tile)
        {
            foreach (int i in doorTiles)
            {
                if (tile == i)
                    return true;
            }
            return false;
        }

        private static readonly int RAISED_OTHER = XY(1, 11);  //16 slots
        public static readonly int RAISED_SIGN = RAISED_OTHER + 0;
        public static readonly int RAISED_STATUE = RAISED_OTHER + 1;
        public static readonly int RAISED_STATUE_SP = RAISED_OTHER + 2;
        public static readonly int RAISED_ALCHEMY_POT = RAISED_OTHER + 3;
        public static readonly int RAISED_BARRICADE = RAISED_OTHER + 4;
        public static readonly int RAISED_HIGH_GRASS = RAISED_OTHER + 5;
        public static readonly int RAISED_FURROWED_GRASS = RAISED_OTHER + 6;

        public static readonly int RAISED_HIGH_GRASS_ALT = RAISED_OTHER + 9;
        public static readonly int RAISED_FURROWED_ALT = RAISED_OTHER + 10;

        /**********************************************************************
         * Raised Tiles, Upper Layer
         **********************************************************************/

        //+1 for open right, +2 for open right-below, +4 for open left-below, +8 for open left.
        public static readonly int WALLS_INTERNAL = XY(1, 12);  //32 slots
        private static readonly int WALL_INTERNAL = WALLS_INTERNAL + 0;
        private static readonly int WALL_INTERNAL_WOODEN = WALLS_INTERNAL + 16;

        public static int StitchInternalWallTile(int tile, int right, int rightBelow, int below, int leftBelow, int left)
        {
            int result;

            if (tile == Terrain.BOOKSHELF || below == Terrain.BOOKSHELF)
                result = WALL_INTERNAL_WOODEN;
            else
                result = WALL_INTERNAL;

            if (!WallStitcheable(right))
                result += 1;
            if (!WallStitcheable(rightBelow))
                result += 2;
            if (!WallStitcheable(leftBelow))
                result += 4;
            if (!WallStitcheable(left))
                result += 8;
            return result;
        }

        //+1 for open to the down-right, +2 for open to the down-left
        private static readonly int WALLS_OVERHANG = XY(1, 14);  //32 slots
        public static readonly int WALL_OVERHANG = WALLS_OVERHANG + 0;
        public static readonly int DOOR_SIDEWAYS_OVERHANG = WALLS_OVERHANG + 4;
        public static readonly int DOOR_SIDEWAYS_OVERHANG_OPEN = WALLS_OVERHANG + 8;
        public static readonly int DOOR_SIDEWAYS_OVERHANG_LOCKED = WALLS_OVERHANG + 12;
        public static readonly int WALL_OVERHANG_WOODEN = WALLS_OVERHANG + 16;

        public static int StitchWallOverhangTile(int tile, int rightBelow, int below, int leftBelow)
        {
            int visual;
            if (tile == Terrain.DOOR)
                visual = DOOR_SIDEWAYS_OVERHANG;
            else if (tile == Terrain.OPEN_DOOR)
                visual = DOOR_SIDEWAYS_OVERHANG_OPEN;
            else if (tile == Terrain.LOCKED_DOOR)
                visual = DOOR_SIDEWAYS_OVERHANG_LOCKED;
            else if (below == Terrain.BOOKSHELF)
                visual = WALL_OVERHANG_WOODEN;
            else
                visual = WALL_OVERHANG;

            if (!WallStitcheable(rightBelow))
                visual += 1;
            if (!WallStitcheable(leftBelow))
                visual += 2;

            return visual;
        }

        //no attachment to adjacent walls
        public static readonly int DOOR_OVERHANG = WALL_OVERHANG + 21;
        public static readonly int DOOR_OVERHANG_OPEN = WALL_OVERHANG + 22;
        public static readonly int DOOR_SIDEWAYS = WALL_OVERHANG + 23;
        public static readonly int DOOR_SIDEWAYS_LOCKED = WALL_OVERHANG + 24;

        public static readonly int STATUE_OVERHANG = WALL_OVERHANG + 32;
        public static readonly int ALCHEMY_POT_OVERHANG = WALL_OVERHANG + 33;
        public static readonly int BARRICADE_OVERHANG = WALL_OVERHANG + 34;
        public static readonly int HIGH_GRASS_OVERHANG = WALL_OVERHANG + 35;
        public static readonly int FURROWED_OVERHANG = WALL_OVERHANG + 36;

        public static readonly int HIGH_GRASS_OVERHANG_ALT = WALL_OVERHANG + 38;
        public static readonly int FURROWED_OVERHANG_ALT = WALL_OVERHANG + 39;

        /**********************************************************************
         * Logic for the selection of tile visuals
         **********************************************************************/

        //These visuals always directly represent a game tile with no stitching required
        public static SparseArray<int> directVisuals = new SparseArray<int>();

        static void InitDirectVisuals()
        {
            directVisuals.Add(Terrain.EMPTY, FLOOR);
            directVisuals.Add(Terrain.GRASS, GRASS);
            directVisuals.Add(Terrain.EMPTY_WELL, EMPTY_WELL);
            directVisuals.Add(Terrain.ENTRANCE, ENTRANCE);
            directVisuals.Add(Terrain.EXIT, EXIT);
            directVisuals.Add(Terrain.EMBERS, EMBERS);
            directVisuals.Add(Terrain.PEDESTAL, PEDESTAL);
            directVisuals.Add(Terrain.EMPTY_SP, FLOOR_SP);

            directVisuals.Add(Terrain.SECRET_TRAP, directVisuals[Terrain.EMPTY]);
            directVisuals.Add(Terrain.TRAP, directVisuals[Terrain.EMPTY]);
            directVisuals.Add(Terrain.INACTIVE_TRAP, directVisuals[Terrain.EMPTY]);

            directVisuals.Add(Terrain.EMPTY_DECO, FLOOR_DECO);
            directVisuals.Add(Terrain.LOCKED_EXIT, LOCKED_EXIT);
            directVisuals.Add(Terrain.UNLOCKED_EXIT, UNLOCKED_EXIT);
            directVisuals.Add(Terrain.WELL, WELL);
        }

        //These visuals directly represent game tiles (no stitching) when terrain is being shown as flat
        public static SparseArray<int> directFlatVisuals = new SparseArray<int>();

        static void InitDirectFlatVisuals()
        {
            directFlatVisuals.Add(Terrain.WALL, FLAT_WALL);
            directFlatVisuals.Add(Terrain.DOOR, FLAT_DOOR);
            directFlatVisuals.Add(Terrain.OPEN_DOOR, FLAT_DOOR_OPEN);
            directFlatVisuals.Add(Terrain.LOCKED_DOOR, FLAT_DOOR_LOCKED);
            directFlatVisuals.Add(Terrain.WALL_DECO, FLAT_WALL_DECO);
            directFlatVisuals.Add(Terrain.BOOKSHELF, FLAT_BOOKSHELF);
            directFlatVisuals.Add(Terrain.SIGN, FLAT_SIGN);
            directFlatVisuals.Add(Terrain.STATUE, FLAT_STATUE);
            directFlatVisuals.Add(Terrain.STATUE_SP, FLAT_STATUE_SP);
            directFlatVisuals.Add(Terrain.ALCHEMY, FLAT_ALCHEMY_POT);
            directFlatVisuals.Add(Terrain.BARRICADE, FLAT_BARRICADE);
            directFlatVisuals.Add(Terrain.HIGH_GRASS, FLAT_HIGH_GRASS);
            directFlatVisuals.Add(Terrain.FURROWED_GRASS, FLAT_FURROWED_GRASS);

            directFlatVisuals.Add(Terrain.SECRET_DOOR, directFlatVisuals[Terrain.WALL]);
        }

        /**********************************************************************
         * Logic for the selection of alternate tile visuals
         **********************************************************************/

        public static sbyte[] tileVariance;

        public static void SetupVariance(int size, long seed)
        {
            Rnd.PushGenerator((int)seed);

            tileVariance = new sbyte[size];
            for (int i = 0; i < tileVariance.Length; ++i)
            {
                tileVariance[i] = (sbyte)Rnd.Int(100);
            }

            Rnd.PopGenerator();
        }

        //These alt visuals will trigger 50% of the time (45% of the time if a rare alt is also present)
        public static SparseArray<int> commonAltVisuals = new SparseArray<int>();

        static void InitCommonAltVisuals()
        {
            commonAltVisuals.Add(FLOOR, FLOOR_ALT_1);
            commonAltVisuals.Add(GRASS, GRASS_ALT);
            commonAltVisuals.Add(FLAT_WALL, FLAT_WALL_ALT);
            commonAltVisuals.Add(EMBERS, EMBERS_ALT);
            commonAltVisuals.Add(FLAT_WALL_DECO, FLAT_WALL_DECO_ALT);
            commonAltVisuals.Add(FLOOR_SP, FLOOR_SP_ALT);
            commonAltVisuals.Add(FLOOR_DECO, FLOOR_DECO_ALT);

            commonAltVisuals.Add(FLAT_BOOKSHELF, FLAT_BOOKSHELF_ALT);
            commonAltVisuals.Add(FLAT_HIGH_GRASS, FLAT_HIGH_GRASS_ALT);
            commonAltVisuals.Add(FLAT_FURROWED_GRASS, FLAT_FURROWED_ALT);

            commonAltVisuals.Add(RAISED_WALL, RAISED_WALL_ALT);
            commonAltVisuals.Add(RAISED_WALL_DECO, RAISED_WALL_DECO_ALT);
            commonAltVisuals.Add(RAISED_WALL_BOOKSHELF, RAISED_WALL_BOOKSHELF_ALT);

            commonAltVisuals.Add(RAISED_HIGH_GRASS, RAISED_HIGH_GRASS_ALT);
            commonAltVisuals.Add(RAISED_FURROWED_GRASS, RAISED_FURROWED_ALT);
            commonAltVisuals.Add(HIGH_GRASS_OVERHANG, HIGH_GRASS_OVERHANG_ALT);
            commonAltVisuals.Add(FURROWED_OVERHANG, FURROWED_OVERHANG_ALT);
        }

        //These alt visuals trigger 5% of the time (and also override common alts when they show up)
        public static SparseArray<int> rareAltVisuals = new SparseArray<int>();

        static void InitRareAltVisuals()
        {
            rareAltVisuals.Add(FLOOR, FLOOR_ALT_2);
        }

        public static int GetVisualWithAlts(int visual, int pos)
        {
            if (tileVariance[pos] >= 95 && rareAltVisuals.ContainsKey(visual))
                return rareAltVisuals[visual];
            else if (tileVariance[pos] >= 50 && commonAltVisuals.ContainsKey(visual))
                return commonAltVisuals[visual];
            else
                return visual;
        }
    }
}