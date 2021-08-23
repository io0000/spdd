using System;
using watabou.utils;
using spdd.levels.painters;
using spdd.items;

namespace spdd.levels.rooms.standard
{
    public class StudyRoom : StandardRoom
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
            return new float[] { 2, 1, 0 };
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.BOOKSHELF);
            Painter.Fill(level, this, 2, Terrain.EMPTY_SP);

            foreach (Door door in connected.Values)
            {
                Painter.DrawInside(level, this, door, 2, Terrain.EMPTY_SP);
                door.Set(Door.Type.REGULAR);
            }

            //TODO add support for giant size as well
            if (sizeCat == SizeCategory.LARGE)
            {
                int pillarW = (Width() - 7) / 2;
                int pillarH = (Height() - 7) / 2;

                Painter.Fill(level, left + 3, top + 3, pillarW, 1, Terrain.BOOKSHELF);
                Painter.Fill(level, left + 3, top + 3, 1, pillarH, Terrain.BOOKSHELF);

                Painter.Fill(level, left + 3, bottom - 2 - 1, pillarW, 1, Terrain.BOOKSHELF);
                Painter.Fill(level, left + 3, bottom - 2 - pillarH, 1, pillarH, Terrain.BOOKSHELF);

                Painter.Fill(level, right - 2 - pillarW, top + 3, pillarW, 1, Terrain.BOOKSHELF);
                Painter.Fill(level, right - 2 - 1, top + 3, 1, pillarH, Terrain.BOOKSHELF);

                Painter.Fill(level, right - 2 - pillarW, bottom - 2 - 1, pillarW, 1, Terrain.BOOKSHELF);
                Painter.Fill(level, right - 2 - 1, bottom - 2 - pillarH, 1, pillarH, Terrain.BOOKSHELF);
            }

            Point center = this.Center();
            Painter.Set(level, center, Terrain.PEDESTAL);

            Item prize = (Rnd.Int(2) == 0) ? level.FindPrizeItem() : null;

            if (prize != null)
            {
                level.Drop(prize, (center.x + center.y * level.Width()));
            }
            else
            {
                level.Drop(Generator.Random(Rnd.OneOf(
                    Generator.Category.POTION,
                    Generator.Category.SCROLL)),
                    (center.x + center.y * level.Width()));
            }
        }
    }
}