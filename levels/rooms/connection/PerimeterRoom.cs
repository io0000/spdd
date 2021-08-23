using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.levels.painters;

namespace spdd.levels.rooms.connection
{
    //tunnels along the room's perimeter
    public class PerimeterRoom : ConnectionRoom
    {
        public override void Paint(Level level)
        {
            int floor = level.TunnelTile();

            FillPerimeterPaths(level, this, floor);

            foreach (Door door in connected.Values)
                door.Set(Door.Type.TUNNEL);
        }

        public static void FillPerimeterPaths(Level l, Room r, int floor)
        {
            corners = null;

            List<Point> pointsToFill = new List<Point>();
            foreach (Point door in r.connected.Values)
            {
                Point p = new Point(door);
                if (p.y == r.top)
                {
                    ++p.y;
                }
                else if (p.y == r.bottom)
                {
                    --p.y;
                }
                else if (p.x == r.left)
                {
                    ++p.x;
                }
                else
                {
                    --p.x;
                }
                pointsToFill.Add(p);
            }

            List<Point> pointsFilled = new List<Point>();
            var pointRemove = pointsToFill[0];
            pointsToFill.RemoveAt(0);
            pointsFilled.Add(pointRemove);

            Point from = null, to = null;
            int shortestDistance;
            while (pointsToFill.Count > 0)
            {
                shortestDistance = int.MaxValue;
                foreach (Point f in pointsFilled)
                {
                    foreach (Point t in pointsToFill)
                    {
                        int dist = DistanceBetweenPoints(r, f, t);
                        if (dist < shortestDistance)
                        {
                            from = f;
                            to = t;
                            shortestDistance = dist;
                        }
                    }
                }
                FillBetweenPoints(l, r, from, to, floor);
                pointsFilled.Add(to);
                pointsToFill.Remove(to);
            }
        }

        private static int SpaceBetween(int a, int b)
        {
            return Math.Abs(a - b) - 1;
        }

        //gets the path distance between two points
        private static int DistanceBetweenPoints(Room r, Point a, Point b)
        {
            //on the same side
            if (((a.x == r.left || a.x == r.right) && a.y == b.y) ||
                ((a.y == r.top || a.y == r.bottom) && a.x == b.x))
            {
                return Math.Max(SpaceBetween(a.x, b.x), SpaceBetween(a.y, b.y));
            }

            //otherwise...
            //subtract 1 at the end to account for overlap
            return Math.Min(SpaceBetween(r.left, a.x) + SpaceBetween(r.left, b.x),
                SpaceBetween(r.right, a.x) +
                SpaceBetween(r.right, b.x))
                +
                Math.Min(SpaceBetween(r.top, a.y) +
                SpaceBetween(r.top, b.y),
                SpaceBetween(r.bottom, a.y) + SpaceBetween(r.bottom, b.y))
                -
                1;
        }

        private static Point[] corners;

        //picks the smallest path to fill between two points
        private static void FillBetweenPoints(Level level, Room r, Point from, Point to, int floor)
        {
            //doors are along the same side
            if (((from.x == r.left + 1 || from.x == r.right - 1) && from.x == to.x) || ((from.y == r.top + 1 || from.y == r.bottom - 1) && from.y == to.y))
            {
                Painter.Fill(level, Math.Min(from.x, to.x), Math.Min(from.y, to.y), SpaceBetween(from.x, to.x) + 2, SpaceBetween(from.y, to.y) + 2, floor);
                return;
            }

            //set up corners
            if (corners == null)
            {
                corners = new Point[4];
                corners[0] = new Point(r.left + 1, r.top + 1);
                corners[1] = new Point(r.right - 1, r.top + 1);
                corners[2] = new Point(r.right - 1, r.bottom - 1);
                corners[3] = new Point(r.left + 1, r.bottom - 1);
            }

            //doors on adjacent sides
            foreach (Point c in corners)
            {
                if ((c.x == from.x || c.y == from.y) && (c.x == to.x || c.y == to.y))
                {
                    Painter.DrawLine(level, from, c, floor);
                    Painter.DrawLine(level, c, to, floor);
                    return;
                }
            }

            //doors on opposite sides
            Point side;
            if (from.y == r.top + 1 || from.y == r.bottom - 1)
            {
                //connect along the left, or right side
                if (SpaceBetween(r.left, from.x) + SpaceBetween(r.left, to.x) <=
                    SpaceBetween(r.right, from.x) + SpaceBetween(r.right, to.x))
                {
                    side = new Point(r.left + 1, r.top + r.Height() / 2);
                }
                else
                {
                    side = new Point(r.right - 1, r.top + r.Height() / 2);
                }
            }
            else
            {
                //connect along the top, or bottom side
                if (SpaceBetween(r.top, from.y) + SpaceBetween(r.top, to.y) <=
                    SpaceBetween(r.bottom, from.y) + SpaceBetween(r.bottom, to.y))
                {
                    side = new Point(r.left + r.Width() / 2, r.top + 1);
                }
                else
                {
                    side = new Point(r.left + r.Width() / 2, r.bottom - 1);
                }
            }
            //treat this as two connections with adjacent sides
            FillBetweenPoints(level, r, from, side, floor);
            FillBetweenPoints(level, r, side, to, floor);
        }
    }
}