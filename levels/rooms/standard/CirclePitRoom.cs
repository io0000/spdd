using System;
using spdd.levels.painters;

namespace spdd.levels.rooms.standard
{
    public class CirclePitRoom : StandardRoom
    {
        public override int MinWidth()
        {
            return Math.Max(8, base.MinWidth());
        }

        public override int MinHeight()
        {
            return Math.Max(8, base.MinHeight());
        }

        public override float[] SizeCatProbs()
        {
            return new float[] { 4, 2, 1 };
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);

            Painter.FillEllipse(level, this, 1, Terrain.EMPTY);

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
                if (door.x == left || door.x == right)
                {
                    Painter.DrawInside(level, this, door, Width() / 2, Terrain.EMPTY);
                }
                else
                {
                    Painter.DrawInside(level, this, door, Height() / 2, Terrain.EMPTY);
                }
            }

            Painter.FillEllipse(level, this, 3, Terrain.CHASM);
        }
    }
}