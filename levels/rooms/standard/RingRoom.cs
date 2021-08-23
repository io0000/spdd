using System;
using spdd.levels.painters;

namespace spdd.levels.rooms.standard
{
    public class RingRoom : StandardRoom
    {
        public override int MinWidth()
        {
            return Math.Max(base.MinWidth(), 7);
        }

        public override int MinHeight()
        {
            return Math.Max(base.MinHeight(), 7);
        }

        public override float[] SizeCatProbs()
        {
            return new float[] { 9, 3, 1 };
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            int minDim = Math.Min(Width(), Height());
            int passageWidth = (int)Math.Floor(0.25f * (minDim + 1));
            Painter.Fill(level, this, passageWidth + 1, Terrain.WALL);

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }
        }
    }
}