using System.Collections.Generic;
using watabou.utils;
using spdd.actors.mobs.npcs;
using spdd.levels.rooms;
using spdd.levels.rooms.standard;

namespace spdd.levels.painters
{
    public class PrisonPainter : RegularPainter
    {
        protected override void Decorate(Level level, List<Room> rooms)
        {
            foreach (Room r in rooms)
            {
                if (r is EntranceRoom)
                {
                    Wandmaker.Quest.SpawnWandmaker(level, r);
                    break;
                }
            }

            int w = level.Width();
            int l = level.Length();
            var map = level.map;

            for (int i = w + 1; i < l - w - 1; ++i)
            {
                if (map[i] == Terrain.EMPTY)
                {
                    float c = 0.05f;
                    if (map[i + 1] == Terrain.WALL && map[i + w] == Terrain.WALL)
                        c += 0.2f;

                    if (map[i - 1] == Terrain.WALL && map[i + w] == Terrain.WALL)
                        c += 0.2f;

                    if (map[i + 1] == Terrain.WALL && map[i - w] == Terrain.WALL)
                        c += 0.2f;

                    if (map[i - 1] == Terrain.WALL && map[i - w] == Terrain.WALL)
                        c += 0.2f;

                    if (Rnd.Float() < c)
                        map[i] = Terrain.EMPTY_DECO;
                }
            }

            for (int i = 0; i < w; ++i)
            {
                if (map[i] == Terrain.WALL &&
                    (map[i + w] == Terrain.EMPTY || map[i + w] == Terrain.EMPTY_SP) &&
                    Rnd.Int(6) == 0)
                {
                    map[i] = Terrain.WALL_DECO;
                }
            }

            for (int i = w; i < l - w; ++i)
            {
                if (map[i] == Terrain.WALL &&
                    map[i - w] == Terrain.WALL &&
                    (map[i + w] == Terrain.EMPTY || map[i + w] == Terrain.EMPTY_SP) &&
                    Rnd.Int(3) == 0)
                {
                    map[i] = Terrain.WALL_DECO;
                }
            }
        }
    }
}