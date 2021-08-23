using watabou.utils;
using spdd.levels.painters;
using spdd.items;
using spdd.items.bombs;
using spdd.items.keys;

namespace spdd.levels.rooms.special
{
    public class ArmoryRoom : SpecialRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            Door entrance = Entrance();
            Point statue = null;
            if (entrance.x == left)
            {
                statue = new Point(right - 1, Rnd.Int(2) == 0 ? top + 1 : bottom - 1);
            }
            else if (entrance.x == right)
            {
                statue = new Point(left + 1, Rnd.Int(2) == 0 ? top + 1 : bottom - 1);
            }
            else if (entrance.y == top)
            {
                statue = new Point(Rnd.Int(2) == 0 ? left + 1 : right - 1, bottom - 1);
            }
            else if (entrance.y == bottom)
            {
                statue = new Point(Rnd.Int(2) == 0 ? left + 1 : right - 1, top + 1);
            }

            if (statue != null)
            {
                Painter.Set(level, statue, Terrain.STATUE);
            }

            int n = Rnd.IntRange(2, 3);
            for (int i = 0; i < n; ++i)
            {
                int pos;
                do
                {
                    pos = level.PointToCell(Random());
                }
                while (level.map[pos] != Terrain.EMPTY || level.heaps[pos] != null);

                level.Drop(Prize(level), pos);
            }

            entrance.Set(Door.Type.LOCKED);
            level.AddItemToSpawn(new IronKey(Dungeon.depth));
        }

        private static Item Prize(Level level)
        {
            switch (Rnd.Int(4))
            {
                case 0:
                    return (new Bomb()).Random();
                case 1:
                    return Generator.RandomWeapon();
                case 2:
                    return Generator.RandomArmor();
                case 3:
                default:
                    return Generator.RandomMissile();
            }
        }
    }
}