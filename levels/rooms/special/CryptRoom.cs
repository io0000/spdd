using watabou.utils;
using spdd.levels.painters;
using spdd.items;
using spdd.items.keys;
using spdd.items.armor;

namespace spdd.levels.rooms.special
{
    public class CryptRoom : SpecialRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            Point c = Center();
            int cx = c.x;
            int cy = c.y;

            Door entrance = this.Entrance();

            entrance.Set(Door.Type.LOCKED);
            level.AddItemToSpawn(new IronKey(Dungeon.depth));

            if (entrance.x == left)
            {
                Painter.Set(level, new Point(right - 1, top + 1), Terrain.STATUE);
                Painter.Set(level, new Point(right - 1, bottom - 1), Terrain.STATUE);
                cx = right - 2;
            }
            else if (entrance.x == right)
            {
                Painter.Set(level, new Point(left + 1, top + 1), Terrain.STATUE);
                Painter.Set(level, new Point(left + 1, bottom - 1), Terrain.STATUE);
                cx = left + 2;
            }
            else if (entrance.y == top)
            {
                Painter.Set(level, new Point(left + 1, bottom - 1), Terrain.STATUE);
                Painter.Set(level, new Point(right - 1, bottom - 1), Terrain.STATUE);
                cy = bottom - 2;
            }
            else if (entrance.y == bottom)
            {
                Painter.Set(level, new Point(left + 1, top + 1), Terrain.STATUE);
                Painter.Set(level, new Point(right - 1, top + 1), Terrain.STATUE);
                cy = top + 2;
            }

            level.Drop(Prize(level), cx + cy * level.Width()).type = Heap.Type.TOMB;
        }

        private static Item Prize(Level level)
        {
            //1 floor set higher than normal
            Armor prize = Generator.RandomArmor((Dungeon.depth / 5) + 1);

            if (Challenges.IsItemBlocked(prize))
            {
                return (new Gold()).Random();
            }

            //if it isn't already cursed, give it a free upgrade
            if (!prize.cursed)
            {
                prize.Upgrade();
                //curse the armor, unless it has a glyph
                if (!prize.HasGoodGlyph())
                {
                    prize.Inscribe(Armor.Glyph.RandomCurse());
                }
            }
            prize.cursed = prize.cursedKnown = true;

            return prize;
        }
    }
}