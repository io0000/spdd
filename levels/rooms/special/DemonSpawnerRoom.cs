using watabou.noosa;
using watabou.utils;
using spdd.actors.mobs;
using spdd.levels.painters;
using spdd.levels.rooms.standard;
using spdd.tiles;

namespace spdd.levels.rooms.special
{
    public class DemonSpawnerRoom : SpecialRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            Point c = Center();
            int cx = c.x;
            int cy = c.y;

            Door door = Entrance();
            door.Set(Door.Type.UNLOCKED);

            DemonSpawner spawner = new DemonSpawner();
            spawner.pos = cx + cy * level.Width();
            level.mobs.Add(spawner);

            CustomFloor vis = new CustomFloor();
            vis.SetRect(left + 1, top + 1, Width() - 2, Height() - 2);
            level.customTiles.Add(vis);
        }

        public override bool Connect(Room room)
        {
            //cannot connect to entrance, otherwise works normally
            if (room is EntranceRoom)
            {
                return false;
            }
            else
            {
                return base.Connect(room);
            }
        }

        public override bool CanPlaceTrap(Point p)
        {
            return false;
        }

        public override bool CanPlaceWater(Point p)
        {
            return false;
        }

        public override bool CanPlaceGrass(Point p)
        {
            return false;
        }

        [SPDStatic]
        public class CustomFloor : CustomTilemap
        {
            public CustomFloor()
            {
                texture = Assets.Environment.HALLS_SP;
            }

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                int cell = tileX + tileY * Dungeon.level.Width();
                int[] map = Dungeon.level.map;
                int[] data = new int[tileW * tileH];
                for (int i = 0; i < data.Length; ++i)
                {
                    if (i % tileW == 0)
                    {
                        cell = tileX + (tileY + i / tileW) * Dungeon.level.Width();
                    }

                    if (Dungeon.level.FindMob(cell) is DemonSpawner)
                    {
                        data[i - 1] = 5 + 4 * 8;
                        data[i] = 6 + 4 * 8;
                        data[i + 1] = 7 + 4 * 8;
                        ++i;
                        ++cell;
                    }
                    else if (map[cell] == Terrain.EMPTY_DECO)
                    {
                        if (Statistics.amuletObtained)
                            data[i] = 31;
                        else
                            data[i] = 27;
                    }
                    else
                    {
                        data[i] = 19;
                    }

                    ++cell;
                }
                v.Map(data, tileW);
                return v;
            }
        }
    }
}