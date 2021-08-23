using System;
using System.Collections.Generic;
using Microsoft.Collections.Extensions;
using watabou.utils;

namespace spdd.levels.rooms
{
    public abstract class Room : Rect, INode, IBundlable
    {
        public List<Room> neighbors = new List<Room>();
        public OrderedDictionary<Room, Door> connected = new OrderedDictionary<Room, Door>();

        public int distance;
        public int price = 1;

        public Room()
        { }

        public Room(Rect other)
            : base(other)   // rect 베이스 클래스가 호출됨
        { }

        public static bool TestOverlap(Room a, Room b)
        {
            int at = a.top;
            int ab = a.bottom;

            int bt = b.top;
            int bb = b.bottom;

            int al = a.left;
            int ar = a.right;

            int bl = b.left;
            int br = b.right;

            if (bl < ar && br > al && bt < ab && bb > at)
                return true;

            return false;
        }


        //public Room Set(Room other)
        //{
        //    base.Set(other);    // Rect의 Set이 호출됨
        //    foreach (Room r in other.neighbors)
        //    {
        //        neighbors.Add(r);
        //        r.neighbors.Remove(other);
        //        r.neighbors.Add(this);
        //    }
        //
        //    foreach (var r in other.connected.Keys)
        //    {
        //        Door d = other.connected[r];
        //        r.connected.Remove(other);
        //        r.connected[this] = d;
        //        this.connected[r] = d;
        //    }
        //    return this;
        //}

        // **** Spatial logic ****

        //Note: when overriding these YOU MUST store any randomly decided values.
        //With the same room and the same parameters these should always return
        //the same value over multiple calls, even if there's some randomness initially.
        public virtual int MinWidth()
        {
            return -1;
        }

        public virtual int MaxWidth()
        {
            return -1;
        }

        public virtual int MinHeight()
        {
            return -1;
        }

        public virtual int MaxHeight()
        {
            return -1;
        }

        public bool SetSize()
        {
            return SetSize(MinWidth(), MaxWidth(), MinHeight(), MaxHeight());
        }

        //public bool ForceSize(int w, int h)
        //{
        //    return SetSize(w, w, h, h);
        //}

        public bool SetSizeWithLimit(int w, int h)
        {
            if (w < MinWidth() || h < MinHeight())
            {
                return false;
            }
            else
            {
                SetSize();

                if (Width() > w || Height() > h)
                {
                    Resize(Math.Min(Width(), w) - 1, Math.Min(Height(), h) - 1);
                }

                return true;
            }
        }

        protected bool SetSize(int minW, int maxW, int minH, int maxH)
        {
            if (minW < MinWidth() ||
                maxW > MaxWidth() ||
                minH < MinHeight() ||
                maxH > MaxHeight() ||
                minW > maxW ||
                minH > maxH)
            {
                return false;
            }
            else
            {
                //subtract one because rooms are inclusive to their right and bottom sides
                Resize(Rnd.NormalIntRange(minW, maxW) - 1,
                        Rnd.NormalIntRange(minH, maxH) - 1);
                return true;
            }
        }

        //Width and height are increased by 1 because rooms are inclusive to their right and bottom sides
        public override int Width()
        {
            return base.Width() + 1;
        }

        public override int Height()
        {
            return base.Height() + 1;
        }

        public Point Random()
        {
            return Random(1);
        }

        public Point Random(int m)
        {
            var x = Rnd.IntRange(left + m, right - m);
            var y = Rnd.IntRange(top + m, bottom - m);
            return new Point(x, y);
        }

        //a point is only considered to be inside if it is within the 1 tile perimeter
        public override bool Inside(Point p)
        {
            return p.x > left && p.y > top && p.x < right && p.y < bottom;
        }

        public override Point Center()
        {
            return new Point(
                    (left + right) / 2 + (((right - left) % 2) == 1 ? Rnd.Int(2) : 0),
                    (top + bottom) / 2 + (((bottom - top) % 2) == 1 ? Rnd.Int(2) : 0));
        }

        // **** Connection logic ****

        public const int ALL = 0;
        public const int LEFT = 1;
        public const int TOP = 2;
        public const int RIGHT = 3;
        public const int BOTTOM = 4;

        public virtual int MinConnections(int direction)
        {
            if (direction == ALL)
                return 1;
            else
                return 0;
        }

        public int CurConnections(int direction)
        {
            if (direction == ALL)
            {
                return connected.Count;
            }
            else
            {
                int total = 0;
                foreach (Room r in connected.Keys)
                {
                    Rect i = Intersect(r);
                    if (direction == LEFT && i.Width() == 0 && i.left == left)
                        ++total;
                    else if (direction == TOP && i.Height() == 0 && i.top == top)
                        ++total;
                    else if (direction == RIGHT && i.Width() == 0 && i.right == right)
                        ++total;
                    else if (direction == BOTTOM && i.Height() == 0 && i.bottom == bottom)
                        ++total;
                }
                return total;
            }
        }

        public int RemConnections(int direction)
        {
            if (CurConnections(ALL) >= MaxConnections(ALL))
                return 0;
            else
                return MaxConnections(direction) - CurConnections(direction);
        }

        public virtual int MaxConnections(int direction)
        {
            if (direction == ALL)
                return 16;
            else
                return 4;
        }

        //only considers point-specific limits, not direction limits
        public virtual bool CanConnect(Point p)
        {
            //point must be along exactly one edge, no corners.
            return (p.x == left || p.x == right) != (p.y == top || p.y == bottom);
        }

        //only considers direction limits, not point-specific limits
        public bool CanConnect(int direction)
        {
            return RemConnections(direction) > 0;
        }

        //considers both direction and point limits
        public virtual bool CanConnect(Room r)
        {
            Rect i = Intersect(r);

            bool foundPoint = false;
            foreach (Point p in i.GetPoints())
            {
                if (CanConnect(p) && r.CanConnect(p))
                {
                    foundPoint = true;
                    break;
                }
            }
            if (!foundPoint)
                return false;

            if (i.Width() == 0 && i.left == left)
                return CanConnect(LEFT) && r.CanConnect(LEFT);
            else if (i.Height() == 0 && i.top == top)
                return CanConnect(TOP) && r.CanConnect(TOP);
            else if (i.Width() == 0 && i.right == right)
                return CanConnect(RIGHT) && r.CanConnect(RIGHT);
            else if (i.Height() == 0 && i.bottom == bottom)
                return CanConnect(BOTTOM) && r.CanConnect(BOTTOM);
            else
                return false;
        }

        public bool AddNeighbor(Room other)
        {
            if (neighbors.Contains(other))
                return true;

            Rect i = Intersect(other);
            if ((i.Width() == 0 && i.Height() >= 2) ||
                (i.Height() == 0 && i.Width() >= 2))
            {
                this.neighbors.Add(other);
                other.neighbors.Add(this);
                return true;
            }
            return false;
        }

        public virtual bool Connect(Room room)
        {
            if ((neighbors.Contains(room) || AddNeighbor(room)) &&
                !connected.ContainsKey(room) &&
                CanConnect(room))
            {
                this.connected[room] = null;
                room.connected[this] = null;
                return true;
            }
            return false;
        }

        public void ClearConnections()
        {
            foreach (Room r in neighbors)
            {
                r.neighbors.Remove(this);
            }
            neighbors.Clear();
            foreach (Room r in connected.Keys)
            {
                r.connected.Remove(this);
            }
            connected.Clear();
        }

        // **** Painter Logic ****

        public abstract void Paint(Level level);

        //whether or not a painter can make its own modifications to a specific point
        public virtual bool CanPlaceWater(Point p)
        {
            return Inside(p);
        }

        public List<Point> WaterPlaceablePoints()
        {
            List<Point> points = new List<Point>();
            for (int i = left; i <= right; ++i)
            {
                for (int j = top; j <= bottom; ++j)
                {
                    Point p = new Point(i, j);
                    if (CanPlaceWater(p))
                        points.Add(p);
                }
            }
            return points;
        }

        //whether or not a painter can make place grass at a specific point
        public virtual bool CanPlaceGrass(Point p)
        {
            return Inside(p);
        }

        public List<Point> GrassPlaceablePoints()
        {
            List<Point> points = new List<Point>();
            for (int i = left; i <= right; ++i)
            {
                for (int j = top; j <= bottom; ++j)
                {
                    Point p = new Point(i, j);
                    if (CanPlaceGrass(p))
                        points.Add(p);
                }
            }
            return points;
        }

        //whether or not a painter can place a trap at a specific point
        public virtual bool CanPlaceTrap(Point p)
        {
            return Inside(p);
        }

        public List<Point> TrapPlaceablePoints()
        {
            List<Point> points = new List<Point>();
            for (int i = left; i <= right; ++i)
            {
                for (int j = top; j <= bottom; ++j)
                {
                    Point p = new Point(i, j);
                    if (CanPlaceTrap(p))
                        points.Add(p);
                }
            }
            return points;
        }

        //whether or not a character (usually spawned) can be placed here
        public virtual bool CanPlaceCharacter(Point p, Level l)
        {
            return Inside(p);
        }

        public List<Point> CharPlaceablePoints(Level l)
        {
            List<Point> points = new List<Point>();
            for (int i = left; i <= right; ++i)
            {
                for (int j = top; j <= bottom; ++j)
                {
                    Point p = new Point(i, j);
                    if (CanPlaceCharacter(p, l))
                        points.Add(p);
                }
            }
            return points;
        }

        // **** Graph.Node interface ****

        public int Distance()
        {
            return distance;
        }

        public void Distance(int value)
        {
            distance = value;
        }

        public int Price()
        {
            return price;
        }

        public void Price(int value)
        {
            price = value;
        }

        public ICollection<INode> Edges()
        {
            List<INode> edges = new List<INode>();
            foreach (var r in connected.Keys)
            {
                Door d = connected[r];
                //for the purposes of path building, ignore all doors that are locked, blocked, or hidden
                if (d.type == Door.Type.EMPTY ||
                    d.type == Door.Type.TUNNEL ||
                    d.type == Door.Type.UNLOCKED ||
                    d.type == Door.Type.REGULAR)
                {
                    edges.Add(r);
                }
            }

            return edges;
        }

        public virtual void StoreInBundle(Bundle bundle)
        {
            bundle.Put("left", left);
            bundle.Put("top", top);
            bundle.Put("right", right);
            bundle.Put("bottom", bottom);
        }

        public virtual void RestoreFromBundle(Bundle bundle)
        {
            left = bundle.GetInt("left");
            top = bundle.GetInt("top");
            right = bundle.GetInt("right");
            bottom = bundle.GetInt("bottom");
        }

        //FIXME currently connections and neighbors are not preserved on load
        public virtual void OnLevelLoad(Level level)
        {
            //does nothing by default
        }


        [SPDStatic]
        public class Door : Point, IBundlable
        {
            public enum Type
            {
                EMPTY, TUNNEL, REGULAR, UNLOCKED, HIDDEN, BARRICADE, LOCKED
            }

            public Type type = Type.EMPTY;

            public Door()
            { }

            public Door(Point p)
                : base(p)
            { }

            public Door(int x, int y)
                : base(x, y)
            { }

            public void Set(Type type)
            {
                if (type.CompareTo(this.type) > 0)
                    this.type = type;
            }

            public void StoreInBundle(Bundle bundle)
            {
                bundle.Put("x", x);
                bundle.Put("y", y);
                bundle.Put("type", type.ToString());    // enum
            }

            public void RestoreFromBundle(Bundle bundle)
            {
                x = bundle.GetInt("x");
                y = bundle.GetInt("y");
                type = bundle.GetEnum<Type>("type");
            }
        }
    }
}