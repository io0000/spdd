using System.Linq;
using System.Collections.Generic;
using watabou.utils;
using spdd.levels.rooms;
using spdd.levels.rooms.standard;
using spdd.levels.rooms.special;
using spdd.levels.rooms.secret;
using spdd.levels.rooms.connection;

namespace spdd.levels.builders
{
    public abstract class RegularBuilder : Builder
    {
        // *** Parameter values for level building logic ***
        // note that implementations do not have to use al of these variables

        protected float pathVariance = 45f;

        public RegularBuilder SetPathVariance(float var)
        {
            pathVariance = var;
            return this;
        }

        //path length is the percentage of pathable rooms that are on
        protected float pathLength = 0.5f;
        //The chance weights for extra rooms to be added to the path
        protected float[] pathLenJitterChances = new float[] { 0, 1, 0 };

        public RegularBuilder SetPathLength(float len, float[] jitter)
        {
            pathLength = len;
            pathLenJitterChances = jitter;
            return this;
        }

        protected float[] pathTunnelChances = new float[] { 1, 3, 1 };
        protected float[] branchTunnelChances = new float[] { 2, 2, 1 };

        public RegularBuilder SetTunnelLength(float[] path, float[] branch)
        {
            pathTunnelChances = path;
            branchTunnelChances = branch;
            return this;
        }

        protected float extraConnectionChance = 0.2f;

        //public RegularBuilder SetExtraConnectionChance(float chance)
        //{
        //    extraConnectionChance = chance;
        //    return this;
        //}

        // *** Room Setup ***

        protected Room entrance = null;
        protected Room exit = null;
        protected Room shop = null;

        protected List<Room> multiConnections = new List<Room>();
        protected List<Room> singleConnections = new List<Room>();

        protected void SetupRooms(List<Room> rooms)
        {
            foreach (Room r in rooms)
                r.SetEmpty();

            entrance = exit = shop = null;
            singleConnections.Clear();
            multiConnections.Clear();
            foreach (Room r in rooms)
            {
                if (r is EntranceRoom)
                {
                    entrance = r;
                }
                else if (r is ExitRoom)
                {
                    exit = r;
                }
                else if (r is ShopRoom && r.MaxConnections(Room.ALL) == 1)
                {
                    shop = r;
                }
                else if (r.MaxConnections(Room.ALL) > 1)
                {
                    multiConnections.Add(r);
                }
                else if (r.MaxConnections(Room.ALL) == 1)
                {
                    singleConnections.Add(r);
                }
            }

            //this weights larger rooms to be much more likely to appear in the main loop, by placing them earlier in the multiconnections list
            WeightRooms(multiConnections);
            Rnd.Shuffle(multiConnections);

            //multiConnections = new ArrayList<>(new LinkedHashSet<>(multiConnections));
            multiConnections = multiConnections.Distinct().ToList();
        }

        // *** Branch Placement ***

        protected void WeightRooms(List<Room> rooms)
        {
            foreach (Room r in rooms.ToArray())
            {
                if (r is StandardRoom)
                {
                    for (int i = 1; i < ((StandardRoom)r).sizeCat.ConnectionWeight(); ++i)
                        rooms.Add(r);
                }
            }
        }

        //places the rooms in roomsToBranch into branches from rooms in branchable.
        //note that the three arrays should be separate, they may contain the same rooms however
        protected void CreateBranches(List<Room> rooms, 
            List<Room> branchable,
            List<Room> roomsToBranch, 
            float[] connChances)
        {
            int i = 0;
            float angle;
            int tries;
            Room curr;
            List<Room> connectingRoomsThisBranch = new List<Room>();
            float[] connectionChances = (float[])connChances.Clone();
            
            while (i < roomsToBranch.Count)
            {
                Room r = roomsToBranch[i];

                connectingRoomsThisBranch.Clear();

                do
                {
                    curr = Rnd.Element(branchable);
                } 
                while (r is SecretRoom && curr is ConnectionRoom);

                int connectingRooms = Rnd.Chances(connectionChances);
                if (connectingRooms == -1)
                {
                    connectionChances = (float[])connChances.Clone();
                    connectingRooms = Rnd.Chances(connectionChances);
                }
                --connectionChances[connectingRooms];

                for (int j = 0; j < connectingRooms; ++j)
                {
                    ConnectionRoom t = r is SecretRoom ? new MazeConnectionRoom() : ConnectionRoom.CreateRoom();
                    tries = 3;

                    do
                    {
                        angle = PlaceRoom(rooms, curr, t, RandomBranchAngle(curr));
                        --tries;
                    } 
                    while (angle == -1 && tries > 0);

                    if (angle == -1)
                    {
                        t.ClearConnections();
                        foreach (Room c in connectingRoomsThisBranch)
                        {
                            c.ClearConnections();
                            rooms.Remove(c);
                        }
                        connectingRoomsThisBranch.Clear();
                        break;
                    }
                    else
                    {
                        connectingRoomsThisBranch.Add(t);
                        rooms.Add(t);
                    }

                    curr = t;
                }

                if (connectingRoomsThisBranch.Count != connectingRooms)
                    continue;

                tries = 10;

                do
                {
                    angle = PlaceRoom(rooms, curr, r, RandomBranchAngle(curr));
                    --tries;
                } 
                while (angle == -1 && tries > 0);

                if (angle == -1)
                {
                    r.ClearConnections();
                    foreach (Room t in connectingRoomsThisBranch)
                    {
                        t.ClearConnections();
                        rooms.Remove(t);
                    }
                    connectingRoomsThisBranch.Clear();
                    continue;
                }

                for (int j = 0; j < connectingRoomsThisBranch.Count; ++j)
                {
                    if (Rnd.Int(3) <= 1) 
                        branchable.Add(connectingRoomsThisBranch[j]);
                }

                if (r.MaxConnections(Room.ALL) > 1 && Rnd.Int(3) == 0)
                {
                    if (r is StandardRoom)
                    {
                        for (int j = 0; j < ((StandardRoom)r).sizeCat.ConnectionWeight(); ++j)
                        {
                            branchable.Add(r);
                        }
                    }
                    else
                    {
                        branchable.Add(r);
                    }
                }

                ++i;
            }
        }

        protected virtual float RandomBranchAngle(Room r)
        {
            return Rnd.Float(360f);
        }
    }
}