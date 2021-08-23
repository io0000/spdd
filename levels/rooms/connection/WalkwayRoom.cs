using System;
using watabou.utils;
using spdd.levels.painters;

namespace spdd.levels.rooms.connection
{
    public class WalkwayRoom : PerimeterRoom
    {
        public override void Paint(Level level)
        {
            if (Math.Min(Width(), Height()) > 3)
            {
                Painter.Fill(level, this, 1, Terrain.CHASM);
            }

            base.Paint(level);

            foreach (Room r in neighbors)
            {
                if (r is BridgeRoom || r is RingBridgeRoom || r is WalkwayRoom)
                {
                    Rect i = Intersect(r);
                    if (i.Width() != 0)
                    {
                        ++i.left;
                        --i.right;
                    }
                    else
                    {
                        ++i.top;
                        --i.bottom;
                    }
                    Painter.Fill(level, i.left, i.top, i.Width() + 1, i.Height() + 1, Terrain.CHASM);
                }
            }
        }
    }
}