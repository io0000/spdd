using System.Collections.Generic;
using watabou.utils;
using spdd.levels.rooms;

namespace spdd.levels.painters
{
    public class SewerPainter : RegularPainter
    {
        protected override void Decorate(Level level, List<Room> rooms)
        {
            int[] map = level.map;
            int w = level.Width();
            int l = level.Length();

            for (int i = 0; i < w; ++i)
            {
                if (map[i] == Terrain.WALL &&
                    map[i + w] == Terrain.WATER &&
                    Rnd.Int(4) == 0)
                {
                    map[i] = Terrain.WALL_DECO;
                }
            }

            for (int i = w; i < l - w; ++i)
            {
                if (map[i] == Terrain.WALL &&
                    map[i - w] == Terrain.WALL &&
                    map[i + w] == Terrain.WATER &&
                    Rnd.Int(2) == 0)
                {
                    map[i] = Terrain.WALL_DECO;
                }
            }

            for (int i = w + 1; i < l - w - 1; ++i)
            {
                if (map[i] == Terrain.EMPTY)
                {
                    int count = (map[i + 1] == Terrain.WALL ? 1 : 0) +
                        (map[i - 1] == Terrain.WALL ? 1 : 0) +
                        (map[i + w] == Terrain.WALL ? 1 : 0) +
                        (map[i - w] == Terrain.WALL ? 1 : 0);

                    if (Rnd.Int(16) < count * count)
                    {
                        map[i] = Terrain.EMPTY_DECO;
                    }
                }
            }
        }
    }
}
