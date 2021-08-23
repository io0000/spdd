using watabou.utils;
using spdd.levels.painters;
using spdd.items;
using spdd.items.scrolls;
using spdd.items.keys;

namespace spdd.levels.rooms.special
{
    public class LibraryRoom : SpecialRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY_SP);

            Door entrance = this.Entrance();

            Painter.Fill(level, left + 1, top + 1, Width() - 2, 1, Terrain.BOOKSHELF);
            Painter.DrawInside(level, this, entrance, 1, Terrain.EMPTY_SP);

            int n = Rnd.NormalIntRange(1, 3);
            for (int i = 0; i < n; ++i)
            {
                int pos;
                do
                {
                    pos = level.PointToCell(Random());
                }
                while (level.map[pos] != Terrain.EMPTY_SP || level.heaps[pos] != null);

                Item item;
                if (i == 0)
                {
                    item = Rnd.Int(2) == 0 ? new ScrollOfIdentify() : new ScrollOfRemoveCurse();
                }
                else
                {
                    item = Prize(level);
                }
                level.Drop(item, pos);
            }

            entrance.Set(Door.Type.LOCKED);

            level.AddItemToSpawn(new IronKey(Dungeon.depth));
        }

        private static Item Prize(Level level)
        {
            Item prize = level.FindPrizeItem(typeof(Scroll));
            if (prize == null)
            {
                prize = Generator.Random(Generator.Category.SCROLL);
            }

            return prize;
        }
    }
}