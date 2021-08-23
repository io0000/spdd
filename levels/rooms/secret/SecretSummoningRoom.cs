using watabou.utils;
using spdd.levels.painters;
using spdd.levels.traps;
using spdd.items;

namespace spdd.levels.rooms.secret
{
    public class SecretSummoningRoom : SecretRoom
    {
        //minimum of 3x3 traps, max of 6x6 traps

        public override int MaxWidth()
        {
            return 8;
        }

        public override int MaxHeight()
        {
            return 8;
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.SECRET_TRAP);

            Point center = Center();
            level.Drop(Generator.Random(), level.PointToCell(center)).SetHauntedIfCursed().type = Heap.Type.SKELETON;

            foreach (Point p in GetPoints())
            {
                int cell = level.PointToCell(p);
                if (level.map[cell] == Terrain.SECRET_TRAP)
                {
                    level.SetTrap((new SummoningTrap()).Hide(), cell);
                }
            }

            Entrance().Set(Door.Type.HIDDEN);
        }
    }
}