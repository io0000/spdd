using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.levels.rooms;
using spdd.levels.rooms.connection;

namespace spdd.levels.builders
{
    //A simple builder which utilizes a line as its core feature.
    public class LineBuilder : RegularBuilder
    {
        public override List<Room> Build(List<Room> rooms)
        {
            SetupRooms(rooms);

            if (entrance == null)
                return null;

            float direction = Rnd.Float(0, 360);
            List<Room> branchable = new List<Room>();

            entrance.SetSize();
            entrance.SetPos(0, 0);
            branchable.Add(entrance);

            if (shop != null)
                PlaceRoom(rooms, entrance, shop, direction + 180f);

            int roomsOnPath = (int)(multiConnections.Count * pathLength) + Rnd.Chances(pathLenJitterChances);
            roomsOnPath = Math.Min(roomsOnPath, multiConnections.Count);

            Room curr = entrance;

            float[] pathTunnels = (float[])pathTunnelChances.Clone();
            for (int i = 0; i <= roomsOnPath; ++i)
            {
                if (i == roomsOnPath && exit == null)
                    continue;

                int tunnels = Rnd.Chances(pathTunnels);
                if (tunnels == -1)
                {
                    pathTunnels = (float[])pathTunnelChances.Clone();
                    tunnels = Rnd.Chances(pathTunnels);
                }
                --pathTunnels[tunnels];

                for (int j = 0; j < tunnels; ++j)
                {
                    ConnectionRoom t = ConnectionRoom.CreateRoom();
                    PlaceRoom(rooms, curr, t, direction + Rnd.Float(-pathVariance, pathVariance));
                    branchable.Add(t);
                    rooms.Add(t);
                    curr = t;
                }

                Room r = (i == roomsOnPath ? exit : multiConnections[i]);
                PlaceRoom(rooms, curr, r, direction + Rnd.Float(-pathVariance, pathVariance));
                branchable.Add(r);
                curr = r;
            }

            List<Room> roomsToBranch = new List<Room>();
            for (int i = roomsOnPath; i < multiConnections.Count; ++i)
                roomsToBranch.Add(multiConnections[i]);

            roomsToBranch.AddRange(singleConnections);
            WeightRooms(branchable);
            CreateBranches(rooms, branchable, roomsToBranch, branchTunnelChances);

            FindNeighbors(rooms);

            foreach (Room r in rooms)
            {
                foreach (Room n in r.neighbors)
                {
                    if (!n.connected.ContainsKey(r) &&
                        Rnd.Float() < extraConnectionChance)
                    {
                        r.Connect(n);
                    }
                }
            }

            return rooms;
        }
    }
}