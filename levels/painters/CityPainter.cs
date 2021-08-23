using System.Collections.Generic;
using watabou.utils;
using spdd.tiles;
using spdd.levels.rooms;

namespace spdd.levels.painters
{
    public class CityPainter : RegularPainter
    {
        protected override void Decorate(Level level, List<Room> rooms)
        {
            var map = level.map;
            int w = level.Width();
            int l = level.Length();

            for (int i = 0; i < l - w; ++i)
            {
                if (map[i] == Terrain.EMPTY &&
                    Rnd.Int(10) == 0)
                {
                    map[i] = Terrain.EMPTY_DECO;
                }
                else if (map[i] == Terrain.WALL &&
                    !DungeonTileSheet.WallStitcheable(map[i + w]) &&
                    Rnd.Int(21 - Dungeon.depth) == 0)
                {
                    map[i] = Terrain.WALL_DECO;
                }
            }
        }
    }
}