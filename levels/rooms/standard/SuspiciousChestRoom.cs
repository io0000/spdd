using System;
using watabou.utils;
using spdd.levels.painters;
using spdd.items;
using spdd.actors.mobs;

namespace spdd.levels.rooms.standard
{
    public class SuspiciousChestRoom : EmptyRoom
    {
        public override int MinWidth()
        {
            return Math.Max(5, base.MinWidth());
        }

        public override int MinHeight()
        {
            return Math.Max(5, base.MinHeight());
        }

        public override void Paint(Level level)
        {
            base.Paint(level);

            Item i = level.FindPrizeItem();

            if (i == null)
            {
                i = (new Gold()).Random();
            }

            int center = level.PointToCell(this.Center());

            Painter.Set(level, center, Terrain.PEDESTAL);

            if (Rnd.Int(3) == 0)
            {
                level.mobs.Add(Mimic.SpawnAt(center, i));
            }
            else
            {
                level.Drop(i, center).type = Heap.Type.CHEST;
            }
        }
    }
}