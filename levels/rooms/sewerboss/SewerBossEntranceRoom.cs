using spdd.levels.rooms.standard;
using spdd.levels.painters;

namespace spdd.levels.rooms.sewerboss
{
    public class SewerBossEntranceRoom : EntranceRoom
    {
        public override int MinHeight()
        {
            return 6;
        }

        public override int MinWidth()
        {
            return 8;
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            Painter.Fill(level, left + 1, top + 1, Width() - 2, 1, Terrain.WALL_DECO);
            Painter.Fill(level, left + 1, top + 2, Width() - 2, 1, Terrain.WATER);

            do
            {
                level.entrance = level.PointToCell(Random(3));
            }
            while (level.FindMob(level.entrance) != null);

            Painter.Set(level, level.entrance, Terrain.ENTRANCE);

            foreach (Room.Door door in connected.Values)
            {
                door.Set(Room.Door.Type.REGULAR);

                if (door.y == top || door.y == top + 1)
                {
                    Painter.DrawInside(level, this, door, 1, Terrain.WATER);
                }
            }
        }
    }
}