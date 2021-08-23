using watabou.utils;
using spdd.actors.mobs;
using spdd.items;
using spdd.items.keys;
using spdd.levels.painters;

namespace spdd.levels.rooms.special
{
    public class TreasuryRoom : SpecialRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            Painter.Set(level, Center(), Terrain.STATUE);

            Heap.Type heapType = Rnd.Int(2) == 0 ? Heap.Type.CHEST : Heap.Type.HEAP;

            int n = Rnd.IntRange(2, 3);
            for (int i = 0; i < n; ++i)
            {
                int pos;
                do
                {
                    pos = level.PointToCell(Random());
                }
                while (level.map[pos] != Terrain.EMPTY || level.heaps[pos] != null || level.FindMob(pos) != null);

                if (heapType == Heap.Type.CHEST && Dungeon.depth > 1 && Rnd.Int(5) == 0)
                {
                    level.mobs.Add(Mimic.SpawnAt(pos, (new Gold()).Random()));
                }
                else
                {
                    level.Drop((new Gold()).Random(), pos).type = heapType;
                }
            }

            if (heapType == Heap.Type.HEAP)
            {
                for (int i = 0; i < 6; ++i)
                {
                    int pos;
                    do
                    {
                        pos = level.PointToCell(Random());
                    }
                    while (level.map[pos] != Terrain.EMPTY);

                    level.Drop(new Gold(Rnd.IntRange(5, 12)), pos);
                }
            }

            Entrance().Set(Door.Type.LOCKED);
            level.AddItemToSpawn(new IronKey(Dungeon.depth));
        }
    }
}