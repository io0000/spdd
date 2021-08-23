using System;
using spdd.levels.painters;

namespace spdd.levels.rooms.standard
{
    public class StatuesRoom : StandardRoom
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
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }

            int rows = (Width() + 1) / 6;
            int cols = (Height() + 1) / 6;

            int w = (Width() - 4 - (rows - 1)) / rows;
            int h = (Height() - 4 - (cols - 1)) / cols;

            int Wspacing = rows % 2 == Width() % 2 ? 2 : 1;
            int Hspacing = cols % 2 == Height() % 2 ? 2 : 1;

            for (int x = 0; x < rows; ++x)
            {
                for (int y = 0; y < cols; ++y)
                {
                    int left = this.left + 2 + (x * (w + Wspacing));
                    int top = this.top + 2 + (y * (h + Hspacing));

                    Painter.Fill(level, left, top, w, h, Terrain.EMPTY_SP);

                    Painter.Set(level, left, top, Terrain.STATUE_SP);
                    Painter.Set(level, left + w - 1, top, Terrain.STATUE_SP);
                    Painter.Set(level, left, top + h - 1, Terrain.STATUE_SP);
                    Painter.Set(level, left + w - 1, top + h - 1, Terrain.STATUE_SP);
                }
            }
        }
    }
}