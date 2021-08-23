using watabou.utils;
using spdd.levels.painters;

namespace spdd.levels.rooms.standard
{
    public class HallwayRoom : EmptyRoom
    {
        //FIXME lots of copy-pasta from tunnel rooms here
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            if (connected.Count < 2)
            {
                //don't want to make a hallway between doors that don't exist
                return;
            }

            Rect c = GetConnectionSpace();

            foreach (Door door in connected.Values)
            {
                Point start;
                Point mid;
                Point end;

                start = new Point(door);
                if (start.x == left)
                    ++start.x;
                else if (start.y == top)
                    ++start.y;
                else if (start.x == right)
                    --start.x;
                else if (start.y == bottom)
                    --start.y;

                int rightShift;
                int downShift;

                if (start.x < c.left)
                    rightShift = c.left - start.x;
                else if (start.x > c.right)
                    rightShift = c.right - start.x;
                else
                    rightShift = 0;

                if (start.y < c.top)
                    downShift = c.top - start.y;
                else if (start.y > c.bottom)
                    downShift = c.bottom - start.y;
                else
                    downShift = 0;

                //always goes inward first
                if (door.x == left || door.x == right)
                {
                    mid = new Point(start.x + rightShift, start.y);
                    end = new Point(mid.x, mid.y + downShift);
                }
                else
                {
                    mid = new Point(start.x, start.y + downShift);
                    end = new Point(mid.x + rightShift, mid.y);
                }

                Painter.DrawLine(level, start, mid, Terrain.EMPTY_SP);
                Painter.DrawLine(level, mid, end, Terrain.EMPTY_SP);
            }

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }
        }

        //returns the space which all doors must connect to (usually 1 cell, but can be more)
        //Note that, like rooms, this space is inclusive to its right and bottom sides
        protected Rect GetConnectionSpace()
        {
            Point c = connected.Count <= 1 ? Center() : GetDoorCenter();

            return new Rect(c.x, c.y, c.x, c.y);
        }

        //returns a point equidistant from all doors this room has
        protected Point GetDoorCenter()
        {
            PointF doorCenter = new PointF(0, 0);

            foreach (Door door in connected.Values)
            {
                doorCenter.x += door.x;
                doorCenter.y += door.y;
            }

            Point c = new Point((int)doorCenter.x / connected.Count, (int)doorCenter.y / connected.Count);
            if (Rnd.Float() < doorCenter.x % 1)
                ++c.x;

            if (Rnd.Float() < doorCenter.y % 1)
                ++c.y;

            c.x = (int)GameMath.Gate(left + 2, c.x, right - 2);
            c.y = (int)GameMath.Gate(top + 2, c.y, bottom - 2);

            return c;
        }
    }
}