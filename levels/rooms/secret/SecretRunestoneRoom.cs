using watabou.utils;
using spdd.levels.painters;
using spdd.items;
using spdd.items.stones;
using spdd.items.potions;

namespace spdd.levels.rooms.secret
{
    public class SecretRunestoneRoom : SecretRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            Door entrance = Entrance();
            Point center = Center();

            if (entrance.x == left || entrance.x == right)
            {
                Painter.DrawLine(level,
                    new Point(center.x, top + 1),
                    new Point(center.x, bottom - 1),
                    Terrain.BOOKSHELF);
                if (entrance.x == left)
                {
                    Painter.Fill(level, center.x + 1, top + 1, right - center.x - 1, Height() - 2, Terrain.EMPTY_SP);
                }
                else
                {
                    Painter.Fill(level, left + 1, top + 1, center.x - left - 1, Height() - 2, Terrain.EMPTY_SP);
                }
            }
            else
            {
                Painter.DrawLine(level,
                    new Point(left + 1, center.y),
                    new Point(right - 1, center.y),
                    Terrain.BOOKSHELF);
                if (entrance.y == top)
                {
                    Painter.Fill(level, left + 1, center.y + 1, Width() - 2, bottom - center.y - 1, Terrain.EMPTY_SP);
                }
                else
                {
                    Painter.Fill(level, left + 1, top + 1, Width() - 2, center.y - top - 1, Terrain.EMPTY_SP);
                }
            }

            level.AddItemToSpawn(new PotionOfLiquidFlame());

            int dropPos;

            do
            {
                dropPos = level.PointToCell(Random());
            }
            while (level.map[dropPos] != Terrain.EMPTY);

            level.Drop(Generator.Random(Generator.Category.STONE), dropPos);

            do
            {
                dropPos = level.PointToCell(Random());
            }
            while (level.map[dropPos] != Terrain.EMPTY || level.heaps[dropPos] != null);

            level.Drop(Generator.Random(Generator.Category.STONE), dropPos);

            do
            {
                dropPos = level.PointToCell(Random());
            }
            while (level.map[dropPos] != Terrain.EMPTY_SP);

            level.Drop(new StoneOfEnchantment(), dropPos);

            entrance.Set(Door.Type.HIDDEN);
        }

        public override bool CanPlaceWater(Point p)
        {
            return false;
        }

        public override bool CanPlaceGrass(Point p)
        {
            return false;
        }

        public override bool CanPlaceCharacter(Point p, Level l)
        {
            return base.CanPlaceCharacter(p, l) && l.map[l.PointToCell(p)] != Terrain.EMPTY_SP;
        }
    }
}
