using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.levels.rooms;
using spdd.levels.rooms.connection;

namespace spdd.levels.builders
{
    //A builder with one core loop as its primary element
    public class LoopBuilder : RegularBuilder
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

        public LoopBuilder SetLoopShape(int exponent, float intensity, float offset)
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

        private PointF loopCenter;

        public override List<Room> Build(List<Room> rooms)
        {
            SetupRooms(rooms);

            if (entrance == null)
                return null;

            entrance.SetSize();
            entrance.SetPos(0, 0);

            float startAngle = Rnd.Float(0, 360);

            List<Room> loop = new List<Room>();
            int roomsOnLoop = (int)(multiConnections.Count * pathLength) + Rnd.Chances(pathLenJitterChances);
            roomsOnLoop = Math.Min(roomsOnLoop, multiConnections.Count);

            float[] pathTunnels = (float[])pathTunnelChances.Clone();
            for (int i = 0; i <= roomsOnLoop; ++i)
            {
                if (i == 0)
                {
                    loop.Add(entrance);
                }
                else
                {
                    var removeRoom = multiConnections[0];
                    multiConnections.RemoveAt(0);
                    loop.Add(removeRoom);
                }

                int tunnels = Rnd.Chances(pathTunnels);
                if (tunnels == -1)
                {
                    pathTunnels = (float[])pathTunnelChances.Clone();
                    tunnels = Rnd.Chances(pathTunnels);
                }
                --pathTunnels[tunnels];

                for (int j = 0; j < tunnels; ++j)
                    loop.Add(ConnectionRoom.CreateRoom());
            }

            if (exit != null)
                loop.Insert((loop.Count + 1) / 2, exit);

            Room prev = entrance;
            float targetAngle;
            for (int i = 1; i < loop.Count; ++i)
            {
                Room r = loop[i];
                targetAngle = startAngle + TargetAngle(i / (float)loop.Count);

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
            while (!prev.Connect(entrance))
            {
                ConnectionRoom c = ConnectionRoom.CreateRoom();

                if (PlaceRoom(loop, prev, c, AngleBetweenRooms(prev, entrance)) == -1)
                    return null;

                loop.Add(c);
                rooms.Add(c);
                prev = c;
            }

            if (shop != null)
            {
                float angle;
                int tries = 10;
                do
                {
                    angle = PlaceRoom(loop, entrance, shop, Rnd.Float(360f));
                    --tries;
                }
                while (angle == -1 && tries >= 0);

                if (angle == -1)
                    return null;
            }

            loopCenter = new PointF();
            foreach (Room r in loop)
            {
                loopCenter.x += (r.left + r.right) / 2f;
                loopCenter.y += (r.top + r.bottom) / 2f;
            }
            loopCenter.x /= loop.Count;
            loopCenter.y /= loop.Count;

            List<Room> branchable = new List<Room>(loop);

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
            if (loopCenter == null)
            {
                return base.RandomBranchAngle(r);
            }
            else
            {
                //generate four angles randomly and return the one which points closer to the center
                float toCenter = AngleBetweenPoints(new PointF((r.left + r.right) / 2f, (r.top + r.bottom) / 2f), loopCenter);
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