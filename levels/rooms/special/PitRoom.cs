using watabou.utils;
using spdd.levels.painters;
using spdd.items;
using spdd.items.keys;

namespace spdd.levels.rooms.special
{
    public class PitRoom : SpecialRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            Door entrance = Entrance();
            entrance.Set(Door.Type.LOCKED);

            Point well = null;
            if (entrance.x == left)
            {
                well = new Point(right - 1, Rnd.Int(2) == 0 ? top + 1 : bottom - 1);
            }
            else if (entrance.x == right)
            {
                well = new Point(left + 1, Rnd.Int(2) == 0 ? top + 1 : bottom - 1);
            }
            else if (entrance.y == top)
            {
                well = new Point(Rnd.Int(2) == 0 ? left + 1 : right - 1, bottom - 1);
            }
            else if (entrance.y == bottom)
            {
                well = new Point(Rnd.Int(2) == 0 ? left + 1 : right - 1, top + 1);
            }
            Painter.Set(level, well, Terrain.EMPTY_WELL);

            int remains = level.PointToCell(Random());
            while (level.map[remains] == Terrain.EMPTY_WELL)
            {
                remains = level.PointToCell(Random());
            }

            level.Drop(new IronKey(Dungeon.depth), remains).type = Heap.Type.SKELETON;
            Item mainLoot = null;
            do
            {
                switch (Rnd.Int(3))
                {
                    case 0:
                        mainLoot = Generator.Random(Generator.Category.RING);
                        break;
                    case 1:
                        mainLoot = Generator.Random(Generator.Category.ARTIFACT);
                        break;
                    case 2:
                        mainLoot = Generator.Random(Rnd.OneOf(
                            Generator.Category.WEAPON,
                            Generator.Category.ARMOR));
                        break;
                }
            } 
            while (mainLoot == null || Challenges.IsItemBlocked(mainLoot));

            level.Drop(mainLoot, remains).SetHauntedIfCursed();

            int n = Rnd.IntRange(1, 2);
            for (int i = 0; i < n; ++i)
            {
                level.Drop(Prize(level), remains).SetHauntedIfCursed();
            }
        }

        private static Item Prize(Level level)
        {
            if (Rnd.Int(2) != 0)
            {
                Item prize = level.FindPrizeItem();
                if (prize != null)
                {
                    return prize;
                }
            }

            return Generator.Random(Rnd.OneOf(
                Generator.Category.POTION,
                Generator.Category.SCROLL,
                Generator.Category.FOOD,
                Generator.Category.GOLD));
        }

        public override bool CanPlaceTrap(Point p)
        {
            //the player is already weak after landing, and will likely need to kite the ghost.
            //having traps here just seems unfair
            return false;
        }
    }
}