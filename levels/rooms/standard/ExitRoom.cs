using System;
using watabou.utils;
using spdd.levels.painters;

namespace spdd.levels.rooms.standard
{
    public class ExitRoom : StandardRoom
    {
        public override int MinWidth()
        {
            return Math.Max(base.MinWidth(), 5);
        }

        public override int MinHeight()
        {
            return Math.Max(base.MinHeight(), 5);
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            foreach (Room.Door door in connected.Values)
            {
                door.Set(Room.Door.Type.REGULAR);
            }

            level.exit = level.PointToCell(Random(2));
            Painter.Set(level, level.exit, Terrain.EXIT);
        }

        public override bool CanPlaceCharacter(Point p, Level l)
        {
            return base.CanPlaceCharacter(p, l) && l.PointToCell(p) != l.exit;
        }
    }
}