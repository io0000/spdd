using spdd.actors.mobs;
using spdd.levels.rooms.connection;
using spdd.levels.painters;

namespace spdd.levels.rooms.sewerboss
{
    public class ThickPillarsGooRoom : GooBossRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.WATER);

            int pillarW = (Width() - 8) / 2;
            int pillarH = (Height() - 8) / 2;

            Painter.Fill(level, left + 2, top + 2, pillarW + 1, pillarH + 1, Terrain.WALL);
            Painter.Fill(level, left + 2, bottom - 2 - pillarH, pillarW + 1, pillarH + 1, Terrain.WALL);
            Painter.Fill(level, right - 2 - pillarW, top + 2, pillarW + 1, pillarH + 1, Terrain.WALL);
            Painter.Fill(level, right - 2 - pillarW, bottom - 2 - pillarH, pillarW + 1, pillarH + 1, Terrain.WALL);

            PerimeterRoom.FillPerimeterPaths(level, this, Terrain.EMPTY_SP);

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }

            SetupGooNest(level);

            Goo boss = new Goo();
            boss.pos = level.PointToCell(Center());
            level.mobs.Add(boss);
        }
    }
}