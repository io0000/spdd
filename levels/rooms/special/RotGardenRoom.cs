using watabou.utils;
using spdd.actors.mobs;
using spdd.items.keys;
using spdd.levels.painters;

namespace spdd.levels.rooms.special
{
    public class RotGardenRoom : SpecialRoom
    {
        public override int MinWidth()
        {
            return 7;
        }

        public override int MinHeight()
        {
            return 7;
        }

        public override void Paint(Level level)
        {
            Door entrance = Entrance();
            entrance.Set(Door.Type.LOCKED);
            level.AddItemToSpawn(new IronKey(Dungeon.depth));

            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.GRASS);

            int heartX = Rnd.IntRange(left + 1, right - 1);
            int heartY = Rnd.IntRange(top + 1, bottom - 1);

            if (entrance.x == left)
            {
                heartX = right - 1;
            }
            else if (entrance.x == right)
            {
                heartX = left + 1;
            }
            else if (entrance.y == top)
            {
                heartY = bottom - 1;
            }
            else if (entrance.y == bottom)
            {
                heartY = top + 1;
            }

            PlacePlant(level, heartX + heartY * level.Width(), new RotHeart());

            int lashers = ((Width() - 2) * (Height() - 2)) / 8;

            for (int i = 1; i <= lashers; ++i)
            {
                int pos;
                do
                {
                    pos = level.PointToCell(Random());
                }
                while (!ValidPlantPos(level, pos));

                PlacePlant(level, pos, new RotLasher());
            }
        }

        private static bool ValidPlantPos(Level level, int pos)
        {
            if (level.map[pos] != Terrain.GRASS)
                return false;

            foreach (int i in PathFinder.NEIGHBORS9)
            {
                if (level.FindMob(pos + i) != null)
                {
                    return false;
                }
            }

            return true;
        }

        private static void PlacePlant(Level level, int pos, Mob plant)
        {
            plant.pos = pos;
            level.mobs.Add(plant);

            foreach (int i in PathFinder.NEIGHBORS8)
            {
                if (level.map[pos + i] == Terrain.GRASS)
                {
                    Painter.Set(level, pos + i, Terrain.HIGH_GRASS);
                }
            }
        }
    }
}