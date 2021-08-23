using watabou.utils;
using spdd.levels.painters;
using spdd.items;
using spdd.items.keys;
using spdd.items.stones;

namespace spdd.levels.rooms.special
{
    public class RunestoneRoom : SpecialRoom
    {
        public override int MinWidth()
        {
            return 6;
        }
        public override int MinHeight()
        {
            return 6;
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.CHASM);

            Painter.DrawInside(level, this, Entrance(), 2, Terrain.EMPTY_SP);
            Painter.Fill(level, this, 2, Terrain.EMPTY);

            int n = Rnd.NormalIntRange(2, 3);
            int dropPos;
            for (int i = 0; i < n; ++i)
            {
                do
                {
                    dropPos = level.PointToCell(Random());
                } 
                while (level.map[dropPos] != Terrain.EMPTY || level.heaps[dropPos] != null);

                level.Drop(Prize(level), dropPos);
            }

            Entrance().Set(Door.Type.LOCKED);
            level.AddItemToSpawn(new IronKey(Dungeon.depth));
        }

        private static Item Prize(Level level)
        {
            Item prize = level.FindPrizeItem(typeof(Runestone));
            if (prize == null)
            {
                prize = Generator.Random(Generator.Category.STONE);
            }

            return prize;
        }
    }
}