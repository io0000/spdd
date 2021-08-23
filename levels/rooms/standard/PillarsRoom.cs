using System;
using watabou.utils;
using spdd.levels.painters;

namespace spdd.levels.rooms.standard
{
    public class PillarsRoom : StandardRoom
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

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }

            int minDim = Math.Min(Width(), Height());

            if (minDim == 7 || Rnd.Int(2) == 0)
            {
                //2 pillars
                int pillarInset = minDim >= 11 ? 2 : 1;
                int pillarSize = ((minDim - 3) / 2) - pillarInset;

                int pillarX, pillarY;
                if (Rnd.Int(2) == 0)
                {
                    pillarX = Rnd.IntRange(left + 1 + pillarInset, right - pillarSize - pillarInset);
                    pillarY = top + 1 + pillarInset;
                }
                else
                {
                    pillarX = left + 1 + pillarInset;
                    pillarY = Rnd.IntRange(top + 1 + pillarInset, bottom - pillarSize - pillarInset);
                }

                //first pillar
                Painter.Fill(level, pillarX, pillarY, pillarSize, pillarSize, Terrain.WALL);

                //invert for second pillar
                pillarX = right - (pillarX - left + pillarSize - 1);
                pillarY = bottom - (pillarY - top + pillarSize - 1);
                Painter.Fill(level, pillarX, pillarY, pillarSize, pillarSize, Terrain.WALL);
            }
            else
            {
                //4 pillars
                int pillarInset = minDim >= 12 ? 2 : 1;
                int pillarSize = (minDim - 6) / (pillarInset + 1);

                float xSpaces = Width() - 2 * pillarInset - pillarSize - 2;
                float ySpaces = Height() - 2 * pillarInset - pillarSize - 2;
                float minSpaces = Math.Min(xSpaces, ySpaces);

                float percentSkew = (long)Math.Round(Rnd.Float() * minSpaces, MidpointRounding.AwayFromZero) / minSpaces;

                //top-left, skews right
                Painter.Fill(level, left + 1 + pillarInset + (int)Math.Round(percentSkew * xSpaces, MidpointRounding.AwayFromZero), top + 1 + pillarInset, pillarSize, pillarSize, Terrain.WALL);

                //top-right, skews down
                Painter.Fill(level, right - pillarSize - pillarInset, top + 1 + pillarInset + (int)Math.Round(percentSkew * ySpaces, MidpointRounding.AwayFromZero), pillarSize, pillarSize, Terrain.WALL);

                //bottom-right, skews left
                Painter.Fill(level, right - pillarSize - pillarInset - (int)Math.Round(percentSkew * xSpaces, MidpointRounding.AwayFromZero), bottom - pillarSize - pillarInset, pillarSize, pillarSize, Terrain.WALL);

                //bottom-left, skews up
                Painter.Fill(level, left + 1 + pillarInset, bottom - pillarSize - pillarInset - (int)Math.Round(percentSkew * ySpaces, MidpointRounding.AwayFromZero), pillarSize, pillarSize, Terrain.WALL);
            }
        }
    }
}