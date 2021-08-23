using watabou.utils;
using spdd.items;
using spdd.levels.painters;
using spdd.levels.rooms.sewerboss;
using spdd.actors.mobs.npcs;

namespace spdd.levels.rooms.secret
{
    public class RatKingRoom : SecretRoom
    {
        public override bool CanConnect(Room r)
        {
            //never connects at the entrance
            return !(r is SewerBossEntranceRoom) && base.CanConnect(r);
        }

        //reduced max size to limit chest numbers.
        // normally would gen with 8-28, this limits it to 8-16
        public override int MaxHeight()
        {
            return 7;
        }

        public override int MaxWidth()
        {
            return 7;
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY_SP);

            Door entrance = Entrance();
            entrance.Set(Door.Type.HIDDEN);
            int door = entrance.x + entrance.y * level.Width();

            for (int i = left + 1; i < right; ++i)
            {
                AddChest(level, (top + 1) * level.Width() + i, door);
                AddChest(level, (bottom - 1) * level.Width() + i, door);
            }

            for (int i = top + 2; i < bottom - 1; ++i)
            {
                AddChest(level, i * level.Width() + left + 1, door);
                AddChest(level, i * level.Width() + right - 1, door);
            }

            RatKing king = new RatKing();
            king.pos = level.PointToCell(Random(2));
            level.mobs.Add(king);
        }

        private static void AddChest(Level level, int pos, int door)
        {
            if (pos == door - 1 ||
                pos == door + 1 ||
                pos == door - level.Width() ||
                pos == door + level.Width())
            {
                return;
            }

            Item prize = new Gold(Rnd.IntRange(10, 25));

            level.Drop(prize, pos).type = Heap.Type.CHEST;
        }
    }
}