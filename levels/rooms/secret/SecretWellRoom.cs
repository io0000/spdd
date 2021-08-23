using System;
using watabou.utils;
using spdd.levels.painters;
using spdd.actors.blobs;

namespace spdd.levels.rooms.secret
{
    public class SecretWellRoom : SecretRoom
    {
        private static readonly Type[] WATERS =
            { typeof(WaterOfAwareness), typeof(WaterOfHealth) };

        public override bool CanConnect(Point p)
        {
            //refuses connections next to corners
            return base.CanConnect(p) && ((p.x > left + 1 && p.x < right - 1) || (p.y > top + 1 && p.y < bottom - 1));
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Point door = Entrance();
            Point well;
            if (door.x == left)
            {
                well = new Point(right - 2, door.y);
            }
            else if (door.x == right)
            {
                well = new Point(left + 2, door.y);
            }
            else if (door.y == top)
            {
                well = new Point(door.x, bottom - 2);
            }
            else
            {
                well = new Point(door.x, top + 2);
            }

            Painter.Fill(level, well.x - 1, well.y - 1, 3, 3, Terrain.CHASM);
            Painter.DrawLine(level, door, well, Terrain.EMPTY);

            Painter.Set(level, well, Terrain.WELL);

            Type waterClass = (Type)Rnd.Element(WATERS);

            WellWater.Seed(well.x + level.Width() * well.y, 1, waterClass, level);

            Entrance().Set(Door.Type.HIDDEN);
        }
    }
}