using System;
using watabou.utils;
using spdd.levels.painters;

namespace spdd.levels.rooms.standard
{
    public class StripedRoom : StandardRoom
    {
        public override float[] SizeCatProbs()
        {
            return new float[] { 2, 1, 0 };
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }

            if (sizeCat == SizeCategory.NORMAL)
            {
                Painter.Fill(level, this, 1, Terrain.EMPTY_SP);
                if (Width() > Height() || (Width() == Height() && Rnd.Int(2) == 0))
                {
                    for (int i = left + 2; i < right; i += 2)
                    {
                        Painter.Fill(level, i, top + 1, 1, Height() - 2, Terrain.HIGH_GRASS);
                    }
                }
                else
                {
                    for (int i = top + 2; i < bottom; i += 2)
                    {
                        Painter.Fill(level, left + 1, i, Width() - 2, 1, Terrain.HIGH_GRASS);
                    }
                }

            }
            else if (sizeCat == SizeCategory.LARGE)
            {
                int layers = (Math.Min(Width(), Height()) - 1) / 2;
                for (int i = 1; i <= layers; ++i)
                {
                    Painter.Fill(level, this, i, (i % 2 == 1) ? Terrain.EMPTY_SP : Terrain.HIGH_GRASS);
                }
            }
        }
    }
}