using watabou.utils;
using spdd.actors.mobs;
using spdd.items.keys;
using spdd.levels.painters;

namespace spdd.levels.rooms.special
{
    public class StatueRoom : SpecialRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            Point c = Center();
            int cx = c.x;
            int cy = c.y;

            Door door = Entrance();

            door.Set(Door.Type.LOCKED);
            level.AddItemToSpawn(new IronKey(Dungeon.depth));

            if (door.x == left)
            {
                Painter.Fill(level, right - 1, top + 1, 1, Height() - 2, Terrain.STATUE);
                cx = right - 2;
            }
            else if (door.x == right)
            {
                Painter.Fill(level, left + 1, top + 1, 1, Height() - 2, Terrain.STATUE);
                cx = left + 2;
            }
            else if (door.y == top)
            {
                Painter.Fill(level, left + 1, bottom - 1, Width() - 2, 1, Terrain.STATUE);
                cy = bottom - 2;
            }
            else if (door.y == bottom)
            {
                Painter.Fill(level, left + 1, top + 1, Width() - 2, 1, Terrain.STATUE);
                cy = top + 2;
            }

            Statue statue = Statue.Random();
            statue.pos = cx + cy * level.Width();
            level.mobs.Add(statue);
        }
    }
}