using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.journal;
using spdd.levels.traps;
using spdd.levels.rooms;
using spdd.levels.rooms.standard;

namespace spdd.levels.painters
{
    public abstract class RegularPainter : Painter
    {
        private float waterFill;
        private int waterSmoothness;

        public RegularPainter SetWater(float fill, int smoothness)
        {
            waterFill = fill;
            waterSmoothness = smoothness;
            return this;
        }

        private float grassFill;
        private int grassSmoothness;

        public RegularPainter SetGrass(float fill, int smoothness)
        {
            grassFill = fill;
            grassSmoothness = smoothness;
            return this;
        }

        private int nTraps;
        private Type[] trapClasses; // Class<? extends Trap>
        private float[] trapChances;

        public RegularPainter SetTraps(int num, Type[] classes, float[] chances)
        {
            nTraps = num;
            trapClasses = classes;
            trapChances = chances;
            return this;
        }

        public override bool Paint(Level level, List<Room> rooms)
        {
            //painter can be used without rooms
            if (rooms != null)
            {
                int padding = level.feeling == Level.Feeling.CHASM ? 2 : 1;

                int leftMost = int.MaxValue, topMost = int.MaxValue;

                foreach (Room r in rooms)
                {
                    if (r.left < leftMost)
                        leftMost = r.left;

                    if (r.top < topMost)
                        topMost = r.top;
                }

                leftMost -= padding;
                topMost -= padding;

                int rightMost = 0, bottomMost = 0;

                foreach (Room r in rooms)
                {
                    r.Shift(-leftMost, -topMost);
                    if (r.right > rightMost)
                        rightMost = r.right;

                    if (r.bottom > bottomMost)
                        bottomMost = r.bottom;
                }

                rightMost += padding;
                bottomMost += padding;

                //add 1 to account for 0 values
                level.SetSize(rightMost + 1, bottomMost + 1);
            }
            else
            {
                //check if the level's size was already initialized by something else
                if (level.Length() == 0)
                    return false;

                //easier than checking for null everywhere
                rooms = new List<Room>();
            }

            Rnd.Shuffle(rooms);

            foreach (Room r in rooms)
            {
                PlaceDoors(r);
                r.Paint(level);
            }

            PaintDoors(level, rooms);

            if (waterFill > 0f)
                PaintWater(level, rooms);

            if (grassFill > 0f)
                PaintGrass(level, rooms);

            if (nTraps > 0)
                PaintTraps(level, rooms);

            Decorate(level, rooms);

            return true;
        }

        protected abstract void Decorate(Level level, List<Room> rooms);

        private void PlaceDoors(Room r)
        {
            foreach (Room n in r.connected.Keys)
            {
                Room.Door door = r.connected[n];
                if (door == null)
                {
                    Rect i = r.Intersect(n);
                    List<Point> doorSpots = new List<Point>();
                    foreach (Point p in i.GetPoints())
                    {
                        if (r.CanConnect(p) && n.CanConnect(p))
                            doorSpots.Add(p);
                    }
                    if (doorSpots.Count == 0)
                    {
                        ShatteredPixelDungeonDash.ReportException(new Exception("Could not place a door! " +
                            "r=" + r.GetType().Name +
                            " n=" + n.GetType().Name));
                        continue;
                    }
                    door = new Room.Door(Rnd.Element(doorSpots));

                    r.connected[n] = door;
                    n.connected[r] = door;
                }
            }
        }

        protected virtual void PaintDoors(Level l, List<Room> rooms)
        {
            foreach (Room r in rooms)
            {
                foreach (var n in r.connected.Keys)
                {
                    if (JoinRooms(l, r, n))
                        continue;

                    Room.Door d = r.connected[n];
                    int door = d.x + d.y * l.Width();

                    if (d.type == Room.Door.Type.REGULAR)
                    {
                        //chance for a hidden door scales from 3/21 on floor 2 to 3/3 on floor 20
                        if (Dungeon.depth > 1 &&
                                (Dungeon.depth >= 20 || Rnd.Int(23 - Dungeon.depth) < Dungeon.depth))
                        {
                            d.type = Room.Door.Type.HIDDEN;
                            Graph.BuildDistanceMap(rooms, r);
                            //don't hide if it would make this room only accessible by hidden doors
                            if (n.distance == int.MaxValue)
                                d.type = Room.Door.Type.UNLOCKED;
                        }
                        else
                        {
                            d.type = Room.Door.Type.UNLOCKED;
                        }

                        //entrance doors on floor 2 are hidden if the player hasn't picked up 2nd guidebook page
                        if (Dungeon.depth == 2 &&
                            !Document.ADVENTURERS_GUIDE.HasPage(Document.GUIDE_SEARCH_PAGE) &&
                            r is EntranceRoom)
                        {
                            d.type = Room.Door.Type.HIDDEN;
                        }
                    }

                    switch (d.type)
                    {
                        case Room.Door.Type.EMPTY:
                            l.map[door] = Terrain.EMPTY;
                            break;
                        case Room.Door.Type.TUNNEL:
                            l.map[door] = l.TunnelTile();
                            break;
                        case Room.Door.Type.UNLOCKED:
                            l.map[door] = Terrain.DOOR;
                            break;
                        case Room.Door.Type.HIDDEN:
                            l.map[door] = Terrain.SECRET_DOOR;
                            break;
                        case Room.Door.Type.BARRICADE:
                            l.map[door] = Terrain.BARRICADE;
                            break;
                        case Room.Door.Type.LOCKED:
                            l.map[door] = Terrain.LOCKED_DOOR;
                            break;
                    }
                }
            }
        }

        protected bool JoinRooms(Level l, Room r, Room n)
        {
            if (!(r is EmptyRoom && n is EmptyRoom))
                return false;

            //TODO decide on good probabilities and dimension restrictions
            Rect w = r.Intersect(n);
            if (w.left == w.right)
            {
                if (w.bottom - w.top < 3)
                    return false;

                if (w.Height() + 1 == Math.Max(r.Height(), n.Height()))
                    return false;

                if (r.Width() + n.Width() > 10)
                    return false;

                w.top += 1;
                w.bottom -= 0;

                ++w.right;

                Painter.Fill(l, w.left, w.top, 1, w.Height(), Terrain.EMPTY);
            }
            else
            {
                if (w.right - w.left < 3)
                    return false;

                if (w.Width() + 1 == Math.Max(r.Width(), n.Width()))
                    return false;

                if (r.Height() + n.Height() > 10)
                    return false;

                w.left += 1;
                w.right -= 0;

                ++w.bottom;

                Painter.Fill(l, w.left, w.top, w.Width(), 1, Terrain.EMPTY);
            }

            return true;
        }

        protected void PaintWater(Level l, List<Room> rooms)
        {
            bool[] lake = Patch.Generate(l.Width(), l.Height(), waterFill, waterSmoothness, true);

            if (rooms.Count > 0)
            {
                foreach (Room r in rooms)
                {
                    foreach (Point p in r.WaterPlaceablePoints())
                    {
                        int i = l.PointToCell(p);
                        if (lake[i] && l.map[i] == Terrain.EMPTY)
                        {
                            l.map[i] = Terrain.WATER;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < l.Length(); ++i)
                {
                    if (lake[i] && l.map[i] == Terrain.EMPTY)
                    {
                        l.map[i] = Terrain.WATER;
                    }
                }
            }
        }

        protected void PaintGrass(Level l, List<Room> rooms)
        {
            bool[] grass = Patch.Generate(l.Width(), l.Height(), grassFill, grassSmoothness, true);

            List<int> grassCells = new List<int>();

            if (rooms.Count > 0)
            {
                foreach (Room r in rooms)
                {
                    foreach (Point p in r.GrassPlaceablePoints())
                    {
                        int i = l.PointToCell(p);
                        if (grass[i] && l.map[i] == Terrain.EMPTY)
                        {
                            grassCells.Add(i);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < l.Length(); ++i)
                {
                    if (grass[i] && l.map[i] == Terrain.EMPTY)
                    {
                        grassCells.Add(i);
                    }
                }
            }

            //Adds chaos to grass height distribution. Ratio of high grass depends on fill and smoothing
            //Full range is 8.3% to 75%, but most commonly (20% fill with 3 smoothing) is around 60%
            //low smoothing, or very low fill, will begin to push the ratio down, normally to 50-30%
            foreach (int i in grassCells)
            {
                if (l.heaps[i] != null || l.FindMob(i) != null)
                {
                    l.map[i] = Terrain.GRASS;
                    continue;
                }

                int count = 1;
                foreach (int n in PathFinder.NEIGHBORS8)
                {
                    if (grass[i + n])
                    {
                        ++count;
                    }
                }
                l.map[i] = (Rnd.Float() < count / 12f) ? Terrain.HIGH_GRASS : Terrain.GRASS;
            }
        }

        protected void PaintTraps(Level l, List<Room> rooms)
        {
            List<int> validCells = new List<int>();

            if (rooms.Count > 0)
            {
                foreach (Room r in rooms)
                {
                    foreach (Point p in r.TrapPlaceablePoints())
                    {
                        int i = l.PointToCell(p);
                        if (l.map[i] == Terrain.EMPTY)
                            validCells.Add(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < l.Length(); ++i)
                {
                    if (l.map[i] == Terrain.EMPTY)
                        validCells.Add(i);
                }
            }

            //no more than one trap every 5 valid tiles.
            nTraps = Math.Min(nTraps, validCells.Count / 5);

            for (int i = 0; i < nTraps; ++i)
            {
                int trapPos = Rnd.Element(validCells);
                validCells.Remove(trapPos); //removes the integer object, not at the index

                Trap trap = (Trap)Reflection.NewInstance(trapClasses[Rnd.Chances(trapChances)]);
                trap = trap.Hide();
                l.SetTrap(trap, trapPos);
                //some traps will not be hidden
                l.map[trapPos] = trap.visible ? Terrain.TRAP : Terrain.SECRET_TRAP;
            }
        }
    }
}