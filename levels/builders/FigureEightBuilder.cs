using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.levels.rooms;
using spdd.levels.rooms.connection;

namespace spdd.levels.builders
{
    public class FigureEightBuilder : RegularBuilder
    {
        //These methods allow for the adjusting of the shape of the loop
        //by default the loop is a perfect circle, but it can be adjusted

        //increasing the exponent will increase the the curvature, making the loop more oval shaped.
        private int curveExponent;

        //This is a percentage (range 0-1) of the intensity of the curve function
        // 0 makes for a perfect linear curve (circle)
        // 1 means the curve is completely determined by the curve exponent
        private float curveIntensity = 1;

        //Adjusts the starting point along the loop.
        // a common example, setting to 0.25 will make for a short fat oval instead of a long one.
        private float curveOffset;

        public FigureEightBuilder SetLoopShape(int exponent, float intensity, float offset)
        {
            this.curveExponent = Math.Abs(exponent);
            curveIntensity = intensity % 1f;
            curveOffset = offset % 0.5f;
            return this;
        }

        private float TargetAngle(float percentAlong)
        {
            percentAlong += curveOffset;
            return 360f * (float)(
                    curveIntensity * CurveEquation(percentAlong)
                            + (1 - curveIntensity) * (percentAlong)
                            - curveOffset);
        }

        private double CurveEquation(double x)
        {
            return Math.Pow(4, 2 * curveExponent)
                    * (Math.Pow((x % 0.5f) - 0.25, 2 * curveExponent + 1))
                    + 0.25 + 0.5 * Math.Floor(2 * x);
        }

        private Room landmarkRoom;

        public FigureEightBuilder SetLandmarkRoom(Room room)
        {
            landmarkRoom = room;
            return this;
        }

        List<Room> firstLoop, secondLoop;
        PointF firstLoopCenter, secondLoopCenter;

        public override List<Room> Build(List<Room> rooms)
        {
            SetupRooms(rooms);

            //TODO might want to make this able to work without an exit. Probably a random room would be landmark and the landmark room would become exit
            if (landmarkRoom == null)
                landmarkRoom = Rnd.Element(multiConnections);

            if (multiConnections.Contains(landmarkRoom))
                multiConnections.Remove(landmarkRoom);

            float startAngle = Rnd.Float(0, 180);

            int roomsOnLoop = (int)(multiConnections.Count * pathLength) + Rnd.Chances(pathLenJitterChances);
            roomsOnLoop = Math.Min(roomsOnLoop, multiConnections.Count);

            int roomsOnFirstLoop = roomsOnLoop / 2;
            if (roomsOnLoop % 2 == 1)
                roomsOnFirstLoop += Rnd.Int(2);

            firstLoop = new List<Room>();
            float[] pathTunnels = (float[])pathTunnelChances.Clone();
            for (int i = 0; i <= roomsOnFirstLoop; ++i)
            {
                if (i == 0)
                {
                    firstLoop.Add(landmarkRoom);
                }
                else
                {
                    var removeRoom = multiConnections[0];
                    multiConnections.RemoveAt(0);
                    firstLoop.Add(removeRoom);
                }

                int tunnels = Rnd.Chances(pathTunnels);
                if (tunnels == -1)
                {
                    pathTunnels = (float[])pathTunnelChances.Clone();
                    tunnels = Rnd.Chances(pathTunnels);
                }
                --pathTunnels[tunnels];

                for (int j = 0; j < tunnels; ++j)
                {
                    firstLoop.Add(ConnectionRoom.CreateRoom());
                }
            }
            if (entrance != null)
                firstLoop.Insert((firstLoop.Count + 1) / 2, entrance);

            int roomsOnSecondLoop = roomsOnLoop - roomsOnFirstLoop;

            secondLoop = new List<Room>();
            for (int i = 0; i <= roomsOnSecondLoop; ++i)
            {
                if (i == 0)
                {
                    secondLoop.Add(landmarkRoom);
                }
                else
                {
                    var removeRoom = multiConnections[0];
                    multiConnections.RemoveAt(0);
                    secondLoop.Add(removeRoom);
                }

                int tunnels = Rnd.Chances(pathTunnels);
                if (tunnels == -1)
                {
                    pathTunnels = (float[])pathTunnelChances.Clone();
                    tunnels = Rnd.Chances(pathTunnels);
                }
                --pathTunnels[tunnels];

                for (int j = 0; j < tunnels; ++j)
                {
                    secondLoop.Add(ConnectionRoom.CreateRoom());
                }
            }
            if (exit != null)
                secondLoop.Insert((secondLoop.Count + 1) / 2, exit);

            landmarkRoom.SetSize();
            landmarkRoom.SetPos(0, 0);

            Room prev = landmarkRoom;
            float targetAngle;
            for (int i = 1; i < firstLoop.Count; ++i)
            {
                Room r = firstLoop[i];
                targetAngle = startAngle + TargetAngle(i / (float)firstLoop.Count);
                if (PlaceRoom(rooms, prev, r, targetAngle) != -1)
                {
                    prev = r;
                    if (!rooms.Contains(prev))
                        rooms.Add(prev);
                }
                else
                {
                    //FIXME this is lazy, there are ways to do this without relying on chance
                    return null;
                }
            }

            //FIXME this is still fairly chance reliant
            // should just write a general function for stitching two rooms together in builder
            while (!prev.Connect(landmarkRoom))
            {
                ConnectionRoom c = ConnectionRoom.CreateRoom();
                if (PlaceRoom(rooms, prev, c, AngleBetweenRooms(prev, landmarkRoom)) == -1)
                    return null;

                firstLoop.Add(c);
                rooms.Add(c);
                prev = c;
            }

            prev = landmarkRoom;
            startAngle += 180f;
            for (int i = 1; i < secondLoop.Count; ++i)
            {
                Room r = secondLoop[i];
                targetAngle = startAngle + TargetAngle(i / (float)secondLoop.Count);
                if (PlaceRoom(rooms, prev, r, targetAngle) != -1)
                {
                    prev = r;
                    if (!rooms.Contains(prev))
                        rooms.Add(prev);
                }
                else
                {
                    //FIXME this is lazy, there are ways to do this without relying on chance
                    return null;
                }
            }

            //FIXME this is still fairly chance reliant
            // should just write a general function for stitching two rooms together in builder
            while (!prev.Connect(landmarkRoom))
            {
                ConnectionRoom c = ConnectionRoom.CreateRoom();
                if (PlaceRoom(rooms, prev, c, AngleBetweenRooms(prev, landmarkRoom)) == -1)
                    return null;

                secondLoop.Add(c);
                rooms.Add(c);
                prev = c;
            }

            if (shop != null)
            {
                float angle;
                int tries = 10;
                do
                {
                    angle = PlaceRoom(firstLoop, entrance, shop, Rnd.Float(360f));
                    --tries;
                }
                while (angle == -1 && tries >= 0);

                if (angle == -1)
                    return null;
            }

            firstLoopCenter = new PointF();
            foreach (Room r in firstLoop)
            {
                firstLoopCenter.x += (r.left + r.right) / 2f;
                firstLoopCenter.y += (r.top + r.bottom) / 2f;
            }
            firstLoopCenter.x /= firstLoop.Count;
            firstLoopCenter.y /= firstLoop.Count;

            secondLoopCenter = new PointF();
            foreach (Room r in secondLoop)
            {
                secondLoopCenter.x += (r.left + r.right) / 2f;
                secondLoopCenter.y += (r.top + r.bottom) / 2f;
            }
            secondLoopCenter.x /= secondLoop.Count;
            secondLoopCenter.y /= secondLoop.Count;

            List<Room> branchable = new List<Room>(firstLoop);
            branchable.AddRange(secondLoop);
            branchable.Remove(landmarkRoom); //remove once so it isn't present twice

            List<Room> roomsToBranch = new List<Room>();
            roomsToBranch.AddRange(multiConnections);
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

        protected override float RandomBranchAngle(Room r)
        {
            PointF center;
            if (firstLoop.Contains(r))
                center = firstLoopCenter;
            else
                center = secondLoopCenter;

            if (center == null)
            {
                return base.RandomBranchAngle(r);
            }
            else
            {
                //generate four angles randomly and return the one which points closer to the center
                float toCenter = AngleBetweenPoints(new PointF((r.left + r.right) / 2f, (r.top + r.bottom) / 2f), center);
                if (toCenter < 0)
                    toCenter += 360f;

                float currAngle = Rnd.Float(360f);
                for (int i = 0; i < 4; ++i)
                {
                    float newAngle = Rnd.Float(360f);
                    if (Math.Abs(toCenter - newAngle) < Math.Abs(toCenter - currAngle))
                    {
                        currAngle = newAngle;
                    }
                }
                return currAngle;
            }
        }
    }
}