using System;
using spdd.levels.painters;

namespace spdd.levels.rooms.standard
{
    public class SkullsRoom : StandardRoom
    {
        public override int MinWidth()
        {
            return Math.Max(7, base.MinWidth());
        }

        public override int MinHeight()
        {
            return Math.Max(7, base.MinHeight());
        }

        public override float[] SizeCatProbs()
        {
            return new float[] { 9, 3, 1 };
        }

        public override void Paint(Level level)
        {
            int minDim = Math.Min(Width(), Height());

            Painter.Fill(level, this, Terrain.WALL);

            if (minDim >= 9)
            {
                Painter.FillEllipse(level, this, 2, Terrain.EMPTY);
            }
            else
            {
                Painter.Fill(level, this, 2, Terrain.EMPTY);
            }

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
                if (door.x == left || door.x == right)
                {
                    Painter.DrawInside(level, this, door, (Width() - 3) / 2, Terrain.EMPTY);
                }
                else
                {
                    Painter.DrawInside(level, this, door, (Height() - 3) / 2, Terrain.EMPTY);
                }
            }

            bool oddWidth = Width() % 2 == 1;
            bool oddHeight = Height() % 2 == 1;

            if (minDim >= 12)
            {
                Painter.FillEllipse(level, this, 5, Terrain.STATUE);
                Painter.FillEllipse(level, this, 6, Terrain.WALL);
            }
            else
            {
                Painter.Fill(level,
                    left + Width() / 2 + (oddWidth ? 0 : -1),
                    top + Height() / 2 + (oddHeight ? 0 : -1),
                    oddWidth ? 1 : 2,
                    oddHeight ? 1 : 2,
                    Terrain.STATUE);
            }
        }
    }
}