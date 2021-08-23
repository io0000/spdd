using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.levels.painters;


namespace spdd.levels.rooms.standard
{
    public class SewerPipeRoom : StandardRoom
    {
        public override int MinWidth()
        {
            return Math.Max(7, base.MinWidth());
        }

        public override int MinHeight()
        {
            return Math.Max(7, base.MinHeight());
        }

        public override float[] SizeCatProbs()
        {
            return new float[] { 4, 2, 1 };
        }

        public override bool CanConnect(Point p)
        {
            //refuses connections next to corners
            return base.CanConnect(p) && ((p.x > left + 1 && p.x < right - 1) || (p.y > top + 1 && p.y < bottom - 1));
        }

        //FIXME this class is a total mess, lots of copy-pasta from tunnel and perimeter rooms here
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);

            Rect c = GetConnectionSpace();

            if (connected.Count <= 2)
            {
                foreach (Door door in connected.Values)
                {
                    Point start;
                    Point mid;
                    Point end;

                    start = new Point(door);
                    if (start.x == left)
                        start.x += 2;
                    else if (start.y == top)
                        start.y += 2;
                    else if (start.x == right)
                        start.x -= 2;
                    else if (start.y == bottom)
                        start.y -= 2;

                    int rightShift;
                    int downShift;

                    if (start.x < c.left)
                        rightShift = c.left - start.x;
                    else if (start.x > c.right)
                        rightShift = c.right - start.x;
                    else
                        rightShift = 0;

                    if (start.y < c.top)
                        downShift = c.top - start.y;
                    else if (start.y > c.bottom)
                        downShift = c.bottom - start.y;
                    else
                        downShift = 0;

                    //always goes inward first
                    if (door.x == left || door.x == right)
                    {
                        mid = new Point(start.x + rightShift, start.y);
                        end = new Point(mid.x, mid.y + downShift);
                    }
                    else
                    {
                        mid = new Point(start.x, start.y + downShift);
                        end = new Point(mid.x + rightShift, mid.y);
                    }

                    Painter.DrawLine(level, start, mid, Terrain.WATER);
                    Painter.DrawLine(level, mid, end, Terrain.WATER);
                }
            }
            else
            {
                List<Point> pointsToFill = new List<Point>();
                foreach (Point door in connected.Values)
                {
                    Point p = new Point(door);
                    if (p.y == top)
                        p.y += 2;
                    else if (p.y == bottom)
                        p.y -= 2;
                    else if (p.x == left)
                        p.x += 2;
                    else
                        p.x -= 2;

                    pointsToFill.Add(p);
                }

                List<Point> pointsFilled = new List<Point>();
                Point toRemove = pointsToFill[0];
                pointsToFill.RemoveAt(0);
                pointsFilled.Add(toRemove);

                Point from = null, to = null;
                int shortestDistance;
                while (pointsToFill.Count > 0)
                {
                    shortestDistance = int.MaxValue;
                    foreach (Point f in pointsFilled)
                    {
                        foreach (Point t in pointsToFill)
                        {
                            int dist = DistanceBetweenPoints(f, t);
                            if (dist < shortestDistance)
                            {
                                from = f;
                                to = t;
                                shortestDistance = dist;
                            }
                        }
                    }
                    FillBetweenPoints(level, from, to, Terrain.WATER);
                    pointsFilled.Add(to);
                    pointsToFill.Remove(to);
                }
            }

            foreach (Point p in GetPoints())
            {
                int cell = level.PointToCell(p);
                if (level.map[cell] == Terrain.WATER)
                {
                    foreach (int i in PathFinder.NEIGHBORS8)
                    {
                        if (level.map[cell + i] == Terrain.WALL)
                        {
                            Painter.Set(level, cell + i, Terrain.EMPTY);
                        }
                    }
                }
            }

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }
        }

        //returns the space which all doors must connect to (usually 1 cell, but can be more)
        //Note that, like rooms, this space is inclusive to its right and bottom sides
        protected Rect GetConnectionSpace()
        {
            Point c = connected.Count <= 1 ? Center() : GetDoorCenter();

            return new Rect(c.x, c.y, c.x, c.y);
        }

        public override bool CanPlaceWater(Point p)
        {
            return false;
        }

        //returns a point equidistant from all doors this room has
        protected Point GetDoorCenter()
        {
            PointF doorCenter = new PointF(0, 0);

            foreach (Door door in connected.Values)
            {
                doorCenter.x += door.x;
                doorCenter.y += door.y;
            }

            Point c = new Point((int)doorCenter.x / connected.Count, (int)doorCenter.y / connected.Count);
            if (Rnd.Float() < doorCenter.x % 1)
                ++c.x;
            if (Rnd.Float() < doorCenter.y % 1)
                ++c.y;
            c.x = (int)GameMath.Gate(left + 2, c.x, right - 2);
            c.y = (int)GameMath.Gate(top + 2, c.y, bottom - 2);

            return c;
        }

        private int SpaceBetween(int a, int b)
        {
            return Math.Abs(a - b) - 1;
        }

        //gets the path distance between two points
        private int DistanceBetweenPoints(Point a, Point b)
        {
            //on the same side
            if (a.y == b.y || a.x == b.x)
            {
                return Math.Max(SpaceBetween(a.x, b.x), SpaceBetween(a.y, b.y));
            }

            //otherwise...
            //subtract 1 at the end to account for overlap
            return
                    Math.Min(SpaceBetween(left, a.x) + SpaceBetween(left, b.x),
                            SpaceBetween(right, a.x) + SpaceBetween(right, b.x))
                            +
                            Math.Min(SpaceBetween(top, a.y) + SpaceBetween(top, b.y),
                                    SpaceBetween(bottom, a.y) + SpaceBetween(bottom, b.y))
                            -
                            1;
        }

        private Point[] corners;

        //picks the smallest path to fill between two points
        private void FillBetweenPoints(Level level, Point from, Point to, int floor)
        {
            //doors are along the same side
            if (from.y == to.y || from.x == to.x)
            {
                Painter.Fill(level,
                        Math.Min(from.x, to.x),
                        Math.Min(from.y, to.y),
                        SpaceBetween(from.x, to.x) + 2,
                        SpaceBetween(from.y, to.y) + 2,
                        floor);
                return;
            }

            //set up corners
            if (corners == null)
            {
                corners = new Point[4];
                corners[0] = new Point(left + 2, top + 2);
                corners[1] = new Point(right - 2, top + 2);
                corners[2] = new Point(right - 2, bottom - 2);
                corners[3] = new Point(left + 2, bottom - 2);
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
            if (from.y == top + 2 || from.y == bottom - 2)
            {
                //connect along the left, or right side
                if (SpaceBetween(left, from.x) + SpaceBetween(left, to.x) <=
                        SpaceBetween(right, from.x) + SpaceBetween(right, to.x))
                {
                    side = new Point(left + 2, top + Height() / 2);
                }
                else
                {
                    side = new Point(right - 2, top + Height() / 2);
                }
            }
            else
            {
                //connect along the top, or bottom side
                if (SpaceBetween(top, from.y) + SpaceBetween(top, to.y) <=
                        SpaceBetween(bottom, from.y) + SpaceBetween(bottom, to.y))
                {
                    side = new Point(left + Width() / 2, top + 2);
                }
                else
                {
                    side = new Point(left + Width() / 2, bottom - 2);
                }
            }
            //treat this as two connections with adjacent sides
            FillBetweenPoints(level, from, side, floor);
            FillBetweenPoints(level, side, to, floor);
        }
    }
}