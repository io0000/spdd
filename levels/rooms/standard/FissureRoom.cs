using System;
using watabou.utils;
using spdd.levels.painters;

namespace spdd.levels.rooms.standard
{
    public class FissureRoom : StandardRoom
    {
        public override float[] SizeCatProbs()
        {
            return new float[] { 6, 3, 1 };
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            foreach (Door door in connected.Values)
                door.Set(Door.Type.REGULAR);

            Painter.Fill(level, this, 1, Terrain.EMPTY);

            if (Square() <= 25)
            {
                //just fill in one tile if the room is tiny
                Point p = Center();
                Painter.Set(level, p.x, p.y, Terrain.CHASM);
            }
            else
            {
                int smallestDim = Math.Min(Width(), Height());
                int floorW = (int)Math.Sqrt(smallestDim);
                //chance for a tile at the edge of the floor to remain a floor tile
                float edgeFloorChance = (float)Math.Sqrt(smallestDim) % 1;
                //the wider the floor the more edge chances tend toward 50%
                edgeFloorChance = (edgeFloorChance + (floorW - 1) * 0.5f) / (float)floorW;

                for (int i = top + 2; i <= bottom - 2; ++i)
                {
                    for (int j = left + 2; j <= right - 2; ++j)
                    {
                        int v = Math.Min(i - top, bottom - i);
                        int h = Math.Min(j - left, right - j);
                        if (Math.Min(v, h) > floorW ||
                            (Math.Min(v, h) == floorW && Rnd.Float() > edgeFloorChance))
                        {
                            Painter.Set(level, j, i, Terrain.CHASM);
                        }
                    }
                }
            }
        }
    }
}