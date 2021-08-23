using System;
using watabou.noosa;
using watabou.utils;
using spdd.actors;
using spdd.actors.mobs;
using spdd.items;
using spdd.tiles;
using spdd.levels.painters;
using spdd.messages;

namespace spdd.levels
{
    public class LastLevel : Level
    {
        public LastLevel()
        {
            color1 = new Color(0x80, 0x15, 0x00, 0xff);
            color2 = new Color(0xa6, 0x85, 0x21, 0xff);

            viewDistance = Math.Min(4, viewDistance);
        }

        public override string TilesTex()
        {
            return Assets.Environment.TILES_HALLS;
        }

        public override string WaterTex()
        {
            return Assets.Environment.WATER_HALLS;
        }

        public override void Create()
        {
            base.Create();
            for (int i = 0; i < Length(); ++i)
            {
                int flags = Terrain.flags[map[i]];
                if ((flags & Terrain.PIT) != 0)
                {
                    passable[i] = avoid[i] = false;
                    solid[i] = true;
                }
            }
            for (int i = (height - ROOM_TOP + 2) * width; i < length; ++i)
            {
                passable[i] = avoid[i] = false;
                solid[i] = true;
            }
            for (int i = (height - ROOM_TOP + 1) * width; i < length; ++i)
            {
                if (i % width < 4 || i % width > 12 || i >= (length - width))
                {
                    discoverable[i] = false;
                }
                else
                {
                    visited[i] = true;
                }
            }
        }

        private const int ROOM_TOP = 10;

        public override bool Build()
        {
            SetSize(16, 64);
            //Arrays.fill( map, Terrain.CHASM );
            Array.Fill(map, Terrain.CHASM);
            int MID = width / 2;

            Painter.Fill(this, 0, height - 1, width, 1, Terrain.WALL);
            Painter.Fill(this, MID - 1, 10, 3, (height - 11), Terrain.EMPTY);
            Painter.Fill(this, MID - 2, height - 3, 5, 1, Terrain.EMPTY);
            Painter.Fill(this, MID - 3, height - 2, 7, 1, Terrain.EMPTY);

            entrance = (height - ROOM_TOP) * Width() + MID;
            Painter.Fill(this, 0, height - ROOM_TOP, width, 2, Terrain.WALL);
            map[entrance] = Terrain.ENTRANCE;
            map[entrance + width] = Terrain.ENTRANCE;
            Painter.Fill(this, 0, height - ROOM_TOP + 2, width, 8, Terrain.EMPTY);
            Painter.Fill(this, MID - 1, height - ROOM_TOP + 2, 3, 1, Terrain.ENTRANCE);

            exit = 12 * (Width()) + MID;

            for (int i = 0; i < Length(); ++i)
            {
                if (map[i] == Terrain.EMPTY && Rnd.Int(5) == 0)
                {
                    map[i] = Terrain.EMPTY_DECO;
                }
            }

            Painter.Fill(this, MID - 2, 9, 5, 7, Terrain.EMPTY);
            Painter.Fill(this, MID - 3, 10, 7, 5, Terrain.EMPTY);

            feeling = Feeling.NONE;
            viewDistance = 4;

            CustomTilemap vis = new CustomFloor();
            vis.SetRect(5, 0, 7, height - ROOM_TOP);
            customTiles.Add(vis);

            vis = new CenterPieceVisuals();
            vis.Pos(0, height - ROOM_TOP);
            customTiles.Add(vis);

            vis = new CenterPieceWalls();
            vis.Pos(0, height - ROOM_TOP - 1);
            customWalls.Add(vis);

            return true;
        }

        public override Mob CreateMob()
        {
            return null;
        }

        public override void CreateMobs()
        { }

        public override Actor AddRespawner()
        {
            return null;
        }

        public override void CreateItems()
        {
            Drop(new Amulet(), exit);
        }

        public override int RandomRespawnCell(Character ch)
        {
            int cell;
            do
            {
                cell = entrance + PathFinder.NEIGHBORS8[Rnd.Int(8)];
            }
            while (!passable[cell] ||
                     (Character.HasProp(ch, Character.Property.LARGE) && !openSpace[cell]) ||
                     Actor.FindChar(cell) != null);

            return cell;
        }

        public override string TileName(int tile)
        {
            switch (tile)
            {
                case Terrain.WATER:
                    return Messages.Get(typeof(HallsLevel), "water_name");
                case Terrain.GRASS:
                    return Messages.Get(typeof(HallsLevel), "grass_name");
                case Terrain.HIGH_GRASS:
                    return Messages.Get(typeof(HallsLevel), "high_grass_name");
                case Terrain.STATUE:
                case Terrain.STATUE_SP:
                    return Messages.Get(typeof(HallsLevel), "statue_name");
                default:
                    return base.TileName(tile);
            }
        }

        public override string TileDesc(int tile)
        {
            switch (tile)
            {
                case Terrain.WATER:
                    return Messages.Get(typeof(HallsLevel), "water_desc");
                case Terrain.STATUE:
                case Terrain.STATUE_SP:
                    return Messages.Get(typeof(HallsLevel), "statue_desc");
                case Terrain.BOOKSHELF:
                    return Messages.Get(typeof(HallsLevel), "bookshelf_desc");
                default:
                    return base.TileDesc(tile);
            }
        }

        public override Group AddVisuals()
        {
            base.AddVisuals();
            HallsLevel.AddHallsVisuals(this, visuals);
            return visuals;
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            for (int i = 0; i < Length(); ++i)
            {
                int flags = Terrain.flags[map[i]];
                if ((flags & Terrain.PIT) != 0)
                {
                    passable[i] = avoid[i] = false;
                    solid[i] = true;
                }
            }
            for (int i = (height - ROOM_TOP + 2) * width; i < length; ++i)
            {
                passable[i] = avoid[i] = false;
                solid[i] = true;
            }
            for (int i = (height - ROOM_TOP + 1) * width; i < length; ++i)
            {
                if (i % width < 4 || i % width > 12 || i >= (length - width))
                {
                    discoverable[i] = false;
                }
                else
                {
                    visited[i] = true;
                }
            }
        }

        [SPDStatic]
        public class CustomFloor : CustomTilemap
        {
            public CustomFloor()
            {
                texture = Assets.Environment.HALLS_SP;
            }

            private static int[] CANDLES = new int[]{
                -1, 42, 46, 46, 46, 43, -1,
                42, 46, 46, 46, 46, 46, 43,
                46, 46, 45, 19, 44, 46, 46,
                46, 46, 19, 19, 19, 46, 46,
                46, 46, 43, 19, 42, 46, 46,
                44, 46, 46, 19, 46, 46, 45,
                -1, 44, 45, 19, 44, 45, -1
            };

            public override Tilemap Create()
            {
                Tilemap v = base.Create();

                int candlesStart = Dungeon.level.exit - 3 - 3 * Dungeon.level.Width();

                int cell = tileX + tileY * Dungeon.level.Width();
                int[] map = Dungeon.level.map;
                int[] data = new int[tileW * tileH];
                for (int i = 0; i < data.Length; ++i)
                {
                    if (i % tileW == 0)
                        cell = tileX + (tileY + i / tileW) * Dungeon.level.Width();

                    if (cell == candlesStart)
                    {
                        foreach (int candle in CANDLES)
                        {
                            if (data[i] == 0)
                                data[i] = candle;

                            if (data[i] == 46 && DungeonTileSheet.tileVariance[cell] >= 50)
                                ++data[i];

                            if (Statistics.amuletObtained && data[i] > 40)
                                data[i] += 8;

                            if (map[cell] != Terrain.CHASM && map[cell + Dungeon.level.width] == Terrain.CHASM)
                                data[i + tileW] = 6;

                            ++i;
                            ++cell;
                            if (i % tileW == 0)
                                cell = tileX + (tileY + i / tileW) * Dungeon.level.Width();
                        }
                    }
                    if (map[cell] == Terrain.EMPTY_DECO)
                    {
                        if (Statistics.amuletObtained)
                            data[i] = 31;
                        else
                            data[i] = 27;
                    }
                    else if (map[cell] == Terrain.EMPTY)
                    {
                        data[i] = 19;
                    }
                    else if (data[i] == 0)
                    {
                        data[i] = -1;
                    }
                    ++cell;
                }
                v.Map(data, tileW);
                return v;
            }
        }

        [SPDStatic]
        public class CenterPieceVisuals : CustomTilemap
        {
            public CenterPieceVisuals()
            {
                texture = Assets.Environment.HALLS_SP;

                tileW = 16;
                tileH = 10;
            }

            private static int[] map = new int[]{
                -1, -1, -1, -1, -1, -1, -1, -1, 19, -1, -1, -1, -1, -1, -1, -1,
                 0,  0,  0,  0,  8,  9, 10, 11, 19, 11, 12, 13, 14,  0,  0,  0,
                 0,  0,  0,  0, 16, 17, 18, 31, 19, 31, 20, 21, 22,  0,  0,  0,
                 0,  0,  0,  0, 24, 25, 26, 19, 19, 19, 28, 29, 30,  0,  0,  0,
                 0,  0,  0,  0, 24, 25, 26, 19, 19, 19, 28, 29, 30,  0,  0,  0,
                 0,  0,  0,  0, 24, 25, 26, 19, 19, 19, 28, 29, 30,  0,  0,  0,
                 0,  0,  0,  0, 24, 25, 34, 35, 35, 35, 34, 29, 30,  0,  0,  0,
                 0,  0,  0,  0, 40, 41, 36, 36, 36, 36, 36, 40, 41,  0,  0,  0,
                 0,  0,  0,  0, 48, 49, 36, 36, 36, 36, 36, 48, 49,  0,  0,  0,
                 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0
            };


            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                v.Map(map, tileW);
                return v;
            }
        }

        [SPDStatic]
        public class CenterPieceWalls : CustomTilemap
        {
            public CenterPieceWalls()
            {
                texture = Assets.Environment.HALLS_SP;

                tileW = 16;
                tileH = 9;
            }

            private static int[] map = new int[]{
                 4,  4,  4,  4,  4,  4,  4,  5,  7,  3,  4,  4,  4,  4,  4,  4,
                 0,  0,  0,  0,  0,  0,  0,  1, 15,  2,  0,  0,  0,  0,  0,  0,
                -1, -1, -1, -1, -1, -1, -1, -1, 23, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, 32, 33, -1, -1, -1, -1, -1, 32, 33, -1, -1, -1,
                -1, -1, -1, -1, 40, 41, -1, -1, -1, -1, -1, 40, 41, -1, -1, -1
            };

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                v.Map(map, tileW);
                return v;
            }
        }
    }
}