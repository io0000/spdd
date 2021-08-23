using System;
using watabou.utils;
using spdd.items;
using spdd.levels.painters;
using spdd.plants;

namespace spdd.levels.rooms.standard
{
    public class PlantsRoom : StandardRoom
    {
        public override int MinWidth()
        {
            return Math.Max(base.MinWidth(), 5);
        }

        public override int MinHeight()
        {
            return Math.Max(base.MinHeight(), 5);
        }

        public override float[] SizeCatProbs()
        {
            return new float[] { 3, 1, 0 };
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.GRASS);
            Painter.Fill(level, this, 2, Terrain.HIGH_GRASS);

            if (Math.Min(Width(), Height()) >= 7)
            {
                Painter.Fill(level, this, 3, Terrain.GRASS);
            }

            Point center = Center();

            //place at least 2 plants for rooms with at least 9 in one dimensions
            if (Math.Max(Width(), Height()) >= 9)
            {
                if (Math.Min(Width(), Height()) >= 11)
                {
                    //place 4 plants for very large rooms
                    Painter.DrawLine(level, new Point(left + 2, center.y), new Point(right - 2, center.y), Terrain.HIGH_GRASS);
                    Painter.DrawLine(level, new Point(center.x, top + 2), new Point(center.x, bottom - 2), Terrain.HIGH_GRASS);
                    level.Plant(RandomSeed(), level.PointToCell(new Point(center.x - 1, center.y - 1)));
                    level.Plant(RandomSeed(), level.PointToCell(new Point(center.x + 1, center.y - 1)));
                    level.Plant(RandomSeed(), level.PointToCell(new Point(center.x - 1, center.y + 1)));
                    level.Plant(RandomSeed(), level.PointToCell(new Point(center.x + 1, center.y + 1)));
                }
                else if (Width() > Height() || (Width() == Height() && Rnd.Int(2) == 0))
                {
                    //place 2 plants otherwise
                    //left/right
                    Painter.DrawLine(level, new Point(center.x, top + 2), new Point(center.x, bottom - 2), Terrain.HIGH_GRASS);
                    level.Plant(RandomSeed(), level.PointToCell(new Point(center.x - 1, center.y)));
                    level.Plant(RandomSeed(), level.PointToCell(new Point(center.x + 1, center.y)));
                }
                else
                {
                    //top/bottom
                    Painter.DrawLine(level, new Point(left + 2, center.y), new Point(right - 2, center.y), Terrain.HIGH_GRASS);
                    level.Plant(RandomSeed(), level.PointToCell(new Point(center.x, center.y - 1)));
                    level.Plant(RandomSeed(), level.PointToCell(new Point(center.x, center.y + 1)));
                }
            }
            else
            {
                //place just one plant for smaller sized rooms
                level.Plant(RandomSeed(), level.PointToCell(center));
            }

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }
        }

        private static Plant.Seed RandomSeed()
        {
            Plant.Seed result;
            do
            {
                result = (Plant.Seed)Generator.RandomUsingDefaults(Generator.Category.SEED);
            }
            while (result is Firebloom.Seed);

            return result;
        }
    }
}