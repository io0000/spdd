using watabou.utils;
using spdd.actors.mobs;
using spdd.levels.painters;

namespace spdd.levels.rooms.sewerboss
{
    public class WalledGooRoom : GooBossRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY_SP);
            Painter.Fill(level, this, 2, Terrain.EMPTY);

            int pillarW = (Width() - 2) / 3;
            int pillarH = (Height() - 2) / 3;

            Painter.Fill(level, left + 2, top + 2, pillarW, 1, Terrain.WALL);
            Painter.Fill(level, left + 2, top + 2, 1, pillarH, Terrain.WALL);

            Painter.Fill(level, left + 2, bottom - 2, pillarW, 1, Terrain.WALL);
            Painter.Fill(level, left + 2, bottom - 1 - pillarH, 1, pillarH, Terrain.WALL);

            Painter.Fill(level, right - 1 - pillarW, top + 2, pillarW, 1, Terrain.WALL);
            Painter.Fill(level, right - 2, top + 2, 1, pillarH, Terrain.WALL);

            Painter.Fill(level, right - 1 - pillarW, bottom - 2, pillarW, 1, Terrain.WALL);
            Painter.Fill(level, right - 2, bottom - 1 - pillarH, 1, pillarH, Terrain.WALL);

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
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