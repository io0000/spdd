using System;
using watabou.utils;
using spdd.levels.painters;

namespace spdd.levels.rooms.connection
{
    public class RingTunnelRoom : TunnelRoom
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

            int floor = level.TunnelTile();

            Rect ring = GetConnectionSpace();

            Painter.Fill(level, ring.left, ring.top, 3, 3, floor);
            Painter.Fill(level, ring.left + 1, ring.top + 1, 1, 1, Terrain.WALL);
        }

        //caches the value so multiple calls will always return the same.
        private Rect connSpace;

        protected override Rect GetConnectionSpace()
        {
            if (connSpace == null)
            {
                Point c = GetDoorCenter();

                c.x = (int)GameMath.Gate(left + 2, c.x, right - 2);
                c.y = (int)GameMath.Gate(top + 2, c.y, bottom - 2);

                connSpace = new Rect(c.x - 1, c.y - 1, c.x + 1, c.y + 1);
            }

            return connSpace;
        }
    }
}