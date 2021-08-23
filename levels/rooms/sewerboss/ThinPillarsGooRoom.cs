using spdd.actors.mobs;
using spdd.levels.rooms.connection;
using spdd.levels.painters;

namespace spdd.levels.rooms.sewerboss
{
    public class ThinPillarsGooRoom : GooBossRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.WATER);

            int pillarW = (Width() == 14 ? 4 : 2) + Width() % 2;
            int pillarH = (Height() == 14 ? 4 : 2) + Height() % 2;

            if (Height() < 12)
            {
                Painter.Fill(level, left + (Width() - pillarW) / 2, top + 2, pillarW, 1, Terrain.WALL);
                Painter.Fill(level, left + (Width() - pillarW) / 2, bottom - 2, pillarW, 1, Terrain.WALL);
            }
            else
            {
                Painter.Fill(level, left + (Width() - pillarW) / 2, top + 3, pillarW, 1, Terrain.WALL);
                Painter.Fill(level, left + (Width() - pillarW) / 2, bottom - 3, pillarW, 1, Terrain.WALL);
            }

            if (Width() < 12)
            {
                Painter.Fill(level, left + 2, top + (Height() - pillarH) / 2, 1, pillarH, Terrain.WALL);
                Painter.Fill(level, right - 2, top + (Height() - pillarH) / 2, 1, pillarH, Terrain.WALL);
            }
            else
            {
                Painter.Fill(level, left + 3, top + (Height() - pillarH) / 2, 1, pillarH, Terrain.WALL);
                Painter.Fill(level, right - 3, top + (Height() - pillarH) / 2, 1, pillarH, Terrain.WALL);
            }

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