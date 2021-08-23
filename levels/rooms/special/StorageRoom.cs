using watabou.utils;
using spdd.items;
using spdd.items.potions;
using spdd.levels.painters;

namespace spdd.levels.rooms.special
{
    public class StorageRoom : SpecialRoom
    {
        public override void Paint(Level level)
        {
            const int floor = Terrain.EMPTY_SP;

            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, floor);

            bool honeyPot = Rnd.Int(2) == 0;

            int n = Rnd.IntRange(3, 4);
            for (int i = 0; i < n; ++i)
            {
                int pos;
                do
                {
                    pos = level.PointToCell(Random());
                }
                while (level.map[pos] != floor);

                if (honeyPot)
                {
                    level.Drop(new Honeypot(), pos);
                    honeyPot = false;
                }
                else
                {
                    level.Drop(Prize(level), pos);
                }
            }

            Entrance().Set(Door.Type.BARRICADE);
            level.AddItemToSpawn(new PotionOfLiquidFlame());
        }

        private static Item Prize(Level level)
        {
            if (Rnd.Int(2) != 0)
            {
                Item prize = level.FindPrizeItem();
                if (prize != null)
                    return prize;
            }

            return Generator.Random(Rnd.OneOf(
                Generator.Category.POTION,
                Generator.Category.SCROLL,
                Generator.Category.FOOD,
                Generator.Category.GOLD));
        }
    }
}