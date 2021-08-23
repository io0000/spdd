using System;
using watabou.utils;
using spdd.levels.painters;
using spdd.levels.traps;

namespace spdd.levels.rooms.standard
{
    public class MinefieldRoom : StandardRoom
    {
        public override float[] SizeCatProbs()
        {
            return new float[] { 4, 1, 0 };
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);
            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }

            int mines = (int)Math.Round(Math.Sqrt(Square()), MidpointRounding.AwayFromZero);

            if (sizeCat == SizeCategory.NORMAL)
            {
                mines -= 3;
            }
            else if (sizeCat == SizeCategory.LARGE)
            {
                mines += 3;
            }
            else
            {
                mines += 9;
            }

            for (int i = 0; i < mines; ++i)
            {
                int pos;
                do
                {
                    pos = level.PointToCell(Random(1));
                }
                while (level.traps[pos] != null);

                //randomly places some embers around the mines
                for (int j = 0; j < 8; ++j)
                {
                    int c = PathFinder.NEIGHBORS8[Rnd.Int(8)];
                    if (level.traps[pos + c] == null && level.map[pos + c] == Terrain.EMPTY)
                    {
                        Painter.Set(level, pos + c, Terrain.EMBERS);
                    }
                }

                Painter.Set(level, pos, Terrain.SECRET_TRAP);
                level.SetTrap((new ExplosiveTrap()).Hide(), pos);
            }
        }
    }
}