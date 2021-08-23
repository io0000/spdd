using System;
using watabou.utils;
using spdd.levels.painters;

namespace spdd.levels.rooms.standard
{
    public class SegmentedRoom : StandardRoom
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
                //set door areas to be empty to help with create walls logic
                Painter.Set(level, door, Terrain.EMPTY);
            }

            CreateWalls(level, new Rect(left + 1, top + 1, right - 1, bottom - 1));
        }

        private void CreateWalls(Level level, Rect area)
        {
            if (Math.Max(area.Width() + 1, area.Height() + 1) < 5 || Math.Min(area.Width() + 1, area.Height() + 1) < 3)
                return;

            int tries = 10;

            //splitting top/bottom
            if (area.Width() > area.Height() ||
                (area.Width() == area.Height() && Rnd.Int(2) == 0))
            {
                do
                {
                    int splitX = Rnd.IntRange(area.left + 2, area.right - 2);

                    if (level.map[splitX + level.Width() * (area.top - 1)] == Terrain.WALL &&
                        level.map[splitX + level.Width() * (area.bottom + 1)] == Terrain.WALL)
                    {
                        tries = 0;

                        Painter.DrawLine(level, new Point(splitX, area.top), new Point(splitX, area.bottom), Terrain.WALL);

                        int spaceTop = Rnd.IntRange(area.top, area.bottom - 1);
                        Painter.Set(level, splitX, spaceTop, Terrain.EMPTY);
                        Painter.Set(level, splitX, spaceTop + 1, Terrain.EMPTY);

                        CreateWalls(level, new Rect(area.left, area.top, splitX - 1, area.bottom));
                        CreateWalls(level, new Rect(splitX + 1, area.top, area.right, area.bottom));
                    }

                }
                while (--tries > 0);
            }
            //splitting left/right
            else
            {
                do
                {
                    int splitY = Rnd.IntRange(area.top + 2, area.bottom - 2);

                    if (level.map[area.left - 1 + level.Width() * splitY] == Terrain.WALL &&
                        level.map[area.right + 1 + level.Width() * splitY] == Terrain.WALL)
                    {
                        tries = 0;

                        Painter.DrawLine(level, new Point(area.left, splitY), new Point(area.right, splitY), Terrain.WALL);

                        int spaceLeft = Rnd.IntRange(area.left, area.right - 1);
                        Painter.Set(level, spaceLeft, splitY, Terrain.EMPTY);
                        Painter.Set(level, spaceLeft + 1, splitY, Terrain.EMPTY);

                        CreateWalls(level, new Rect(area.left, area.top, area.right, splitY - 1));
                        CreateWalls(level, new Rect(area.left, splitY + 1, area.right, area.bottom));
                    }

                }
                while (--tries > 0);
            }
        }
    }
}