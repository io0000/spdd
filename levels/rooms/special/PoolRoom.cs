using watabou.utils;
using spdd.actors.mobs;
using spdd.items;
using spdd.items.potions;
using spdd.levels.painters;

namespace spdd.levels.rooms.special
{
    public class PoolRoom : SpecialRoom
    {
        private const int NPIRANHAS = 3;

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
            Painter.Fill(level, this, 1, Terrain.WATER);

            Door door = Entrance();
            door.Set(Door.Type.REGULAR);

            int x = -1;
            int y = -1;
            if (door.x == left)
            {
                x = right - 1;
                y = top + Height() / 2;
                Painter.Fill(level, left + 1, top + 1, 1, Height() - 2, Terrain.EMPTY_SP);
            }
            else if (door.x == right)
            {
                x = left + 1;
                y = top + Height() / 2;
                Painter.Fill(level, right - 1, top + 1, 1, Height() - 2, Terrain.EMPTY_SP);
            }
            else if (door.y == top)
            {
                x = left + Width() / 2;
                y = bottom - 1;
                Painter.Fill(level, left + 1, top + 1, Width() - 2, 1, Terrain.EMPTY_SP);
            }
            else if (door.y == bottom)
            {
                x = left + Width() / 2;
                y = top + 1;
                Painter.Fill(level, left + 1, bottom - 1, Width() - 2, 1, Terrain.EMPTY_SP);
            }

            int pos = x + y * level.Width();
            level.Drop(Prize(level), pos).type = Rnd.Int(3) == 0 ? Heap.Type.CHEST : Heap.Type.HEAP;
            Painter.Set(level, pos, Terrain.PEDESTAL);

            level.AddItemToSpawn(new PotionOfInvisibility());

            for (int i = 0; i < NPIRANHAS; ++i)
            {
                Piranha piranha = new Piranha();
                do
                {
                    piranha.pos = level.PointToCell(Random());
                }
                while (level.map[piranha.pos] != Terrain.WATER || level.FindMob(piranha.pos) != null);

                level.mobs.Add(piranha);
            }
        }

        private static Item Prize(Level level)
        {
            Item prize;

            if (Rnd.Int(3) == 0)
            {
                prize = level.FindPrizeItem();
                if (prize != null)
                    return prize;
            }

            //1 floor set higher in probability, never cursed
            do
            {
                if (Rnd.Int(2) == 0)
                {
                    prize = Generator.RandomWeapon((Dungeon.depth / 5) + 1);
                }
                else
                {
                    prize = Generator.RandomArmor((Dungeon.depth / 5) + 1);
                }
            }
            while (prize.cursed || Challenges.IsItemBlocked(prize));

            prize.cursedKnown = true;

            //33% chance for an extra update.
            if (Rnd.Int(3) == 0)
                prize.Upgrade();

            return prize;
        }
    }
}