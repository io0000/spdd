using System.Collections.Generic;
using watabou.utils;
using spdd.levels.rooms;
using spdd.levels.rooms.standard;
using spdd.levels.rooms.connection;
using spdd.tiles;

namespace spdd.levels.painters
{
    public class CavesPainter : RegularPainter
    {
        protected override void Decorate(Level level, List<Room> rooms)
        {
            int w = level.Width();
            int l = level.Length();
            int[] map = level.map;

            foreach (Room room in rooms)
            {
                if (!(room is EmptyRoom || room is CaveRoom))
                    continue;

                if (room.Width() <= 4 || room.Height() <= 4)
                    continue;

                int s = room.Square();

                if (Rnd.Int(s) > 8)
                {
                    int corner = (room.left + 1) + (room.top + 1) * w;
                    if (map[corner - 1] == Terrain.WALL && map[corner - w] == Terrain.WALL)
                    {
                        map[corner] = Terrain.WALL;
                        level.traps.Remove(corner);
                    }
                }

                if (Rnd.Int(s) > 8)
                {
                    int corner = (room.right - 1) + (room.top + 1) * w;
                    if (map[corner + 1] == Terrain.WALL && map[corner - w] == Terrain.WALL)
                    {
                        map[corner] = Terrain.WALL;
                        level.traps.Remove(corner);
                    }
                }

                if (Rnd.Int(s) > 8)
                {
                    int corner = (room.left + 1) + (room.bottom - 1) * w;
                    if (map[corner - 1] == Terrain.WALL && map[corner + w] == Terrain.WALL)
                    {
                        map[corner] = Terrain.WALL;
                        level.traps.Remove(corner);
                    }
                }

                if (Rnd.Int(s) > 8)
                {
                    int corner = (room.right - 1) + (room.bottom - 1) * w;
                    if (map[corner + 1] == Terrain.WALL && map[corner + w] == Terrain.WALL)
                    {
                        map[corner] = Terrain.WALL;
                        level.traps.Remove(corner);
                    }
                }

                foreach (Room n in room.connected.Keys)
                {
                    if ((n is StandardRoom || n is ConnectionRoom) && Rnd.Int(3) == 0)
                    {
                        Painter.Set(level, room.connected[n], Terrain.EMPTY_DECO);
                    }
                }
            }

            for (int i = w + 1; i < l - w; ++i)
            {
                if (map[i] == Terrain.EMPTY)
                {
                    int n = 0;
                    if (map[i + 1] == Terrain.WALL)
                        ++n;

                    if (map[i - 1] == Terrain.WALL)
                        ++n;

                    if (map[i + w] == Terrain.WALL)
                        ++n;

                    if (map[i - w] == Terrain.WALL)
                        ++n;

                    if (Rnd.Int(6) <= n)
                        map[i] = Terrain.EMPTY_DECO;
                }
            }

            for (int i = 0; i < l - w; ++i)
            {
                if (map[i] == Terrain.WALL &&
                    DungeonTileSheet.FloorTile(map[i + w]) &&
                    Rnd.Int(4) == 0)
                {
                    map[i] = Terrain.WALL_DECO;
                }
            }

            foreach (Room r in rooms)
            {
                if (r is EmptyRoom)
                {
                    foreach (Room n in r.neighbors)
                    {
                        if (n is EmptyRoom && !r.connected.ContainsKey(n))
                        {
                            Rect i = r.Intersect(n);
                            if (i.left == i.right && i.bottom - i.top >= 5)
                            {
                                i.top += 2;
                                i.bottom -= 1;

                                ++i.right;

                                Painter.Fill(level, i.left, i.top, 1, i.Height(), Terrain.CHASM);
                            }
                            else if (i.top == i.bottom && i.right - i.left >= 5)
                            {
                                i.left += 2;
                                i.right -= 1;

                                ++i.bottom;

                                Painter.Fill(level, i.left, i.top, i.Width(), 1, Terrain.CHASM);
                            }
                        }
                    }
                }
            }
        }
    }
}