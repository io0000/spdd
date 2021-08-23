using watabou.utils;
using spdd.actors.mobs;
using spdd.levels.painters;

namespace spdd.levels.rooms.sewerboss
{
    public class DiamondGooRoom : GooBossRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);

            Painter.FillDiamond(level, this, 1, Terrain.EMPTY);

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
                Point dir;
                if (door.x == left)
                {
                    dir = new Point(1, 0);
                }
                else if (door.y == top)
                {
                    dir = new Point(0, 1);
                }
                else if (door.x == right)
                {
                    dir = new Point(-1, 0);
                }
                else
                {
                    dir = new Point(0, -1);
                }

                Point curr = new Point(door);
                do
                {
                    Painter.Set(level, curr, Terrain.EMPTY_SP);
                    curr.x += dir.x;
                    curr.y += dir.y;
                }
                while (level.map[level.PointToCell(curr)] == Terrain.WALL);
            }

            Painter.Fill(level, left + Width() / 2 - 1, top + Height() / 2 - 2, 2 + Width() % 2, 4 + Height() % 2, Terrain.WATER);
            Painter.Fill(level, left + Width() / 2 - 2, top + Height() / 2 - 1, 4 + Width() % 2, 2 + Height() % 2, Terrain.WATER);

            SetupGooNest(level);

            Goo boss = new Goo();
            boss.pos = level.PointToCell(Center());
            level.mobs.Add(boss);
        }

        public override bool CanPlaceWater(Point p)
        {
            return false;
        }
    }
}