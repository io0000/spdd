using System.Collections.Generic;
using watabou.utils;
using spdd.levels.rooms;

namespace spdd.levels.painters
{
    public class HallsPainter : RegularPainter
    {
        protected override void Decorate(Level level, List<Room> rooms)
        {
            var map = level.map;
            int w = level.Width();
            int l = level.Length();

            for (int i = w + 1; i < l - w - 1; ++i)
            {
                if (map[i] == Terrain.EMPTY)
                {
                    int count = 0;
                    for (int j = 0; j < PathFinder.NEIGHBORS8.Length; ++j)
                    {
                        if ((Terrain.flags[map[i + PathFinder.NEIGHBORS8[j]]] & Terrain.PASSABLE) > 0)
                        {
                            ++count;
                        }
                    }

                    if (Rnd.Int(80) < count)
                        map[i] = Terrain.EMPTY_DECO;
                }
                else
                {
                    if (map[i] == Terrain.WALL &&
                        map[i - 1] != Terrain.WALL_DECO &&
                        map[i - w] != Terrain.WALL_DECO &&
                        Rnd.Int(20) == 0)
                    {
                        map[i] = Terrain.WALL_DECO;
                    }
                }
            }
        }
    }
}
