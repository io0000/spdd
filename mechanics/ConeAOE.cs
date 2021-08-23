using System;
using System.Collections.Generic;
using watabou.utils;

namespace spdd.mechanics
{
    //a cone made of up several ballisticas scanning in an arc
    public class ConeAOE
    {
        public Ballistic coreRay;

        public List<Ballistic> rays = new List<Ballistic>();
        public HashSet<int> cells = new HashSet<int>();

        public ConeAOE(Ballistic core, float degrees)
            : this(core, float.PositiveInfinity, degrees, Ballistic.STOP_TARGET/* TODO */)
        { }

        public ConeAOE(Ballistic core, float maxDist, float degrees, int ballisticaParams)
        {
            coreRay = core;

            //we want to use true coordinates for our trig functions, not game cells
            // so get the center of from and to as points
            PointF fromP = new PointF(Dungeon.level.CellToPoint(core.sourcePos));
            fromP.x += 0.5f;
            fromP.y += 0.5f;

            PointF toP = new PointF(Dungeon.level.CellToPoint(core.collisionPos));
            toP.x += 0.5f;
            toP.y += 0.5f;

            //clamp distance of cone to maxDist (in true distance, not game distance)
            if (PointF.Distance(fromP, toP) > maxDist)
            {
                toP = PointF.Inter(fromP, toP, maxDist / PointF.Distance(fromP, toP));
            }

            //now we can get the circle's radius. We bump it by 0.5 as we want the cone to reach
            // The edge of the target cell, not the center.
            float circleRadius = PointF.Distance(fromP, toP);
            circleRadius += 0.5f;

            //Now we find every unique cell along the outer arc of our cone.
            PointF scan = new PointF();
            Point scanInt = new Point();
            float initalAngle = PointF.Angle(fromP, toP) / PointF.G2R;
            //want to preserve order so that our collection of rays is going clockwise
            //LinkedHashSet<Integer> targetCells = new LinkedHashSet<>();
            List<int> targetCells = new List<int>();

            //cast a ray every 0.5 degrees in a clockwise arc, to find cells along the cone's outer arc
            for (float a = initalAngle + degrees / 2f; a >= initalAngle - degrees / 2f; a -= 0.5f)
            {
                scan.Polar(a * PointF.G2R, circleRadius);
                scan.Offset(fromP);
                scan.x += (fromP.x > scan.x ? +0.5f : -0.5f);
                scan.y += (fromP.y > scan.y ? +0.5f : -0.5f);
                scanInt.Set(
                        (int)GameMath.Gate(0, (int)Math.Floor(scan.x), Dungeon.level.Width() - 1),
                        (int)GameMath.Gate(0, (int)Math.Floor(scan.y), Dungeon.level.Height() - 1));

                int addValue = Dungeon.level.PointToCell(scanInt);
                AddToTargetCells(targetCells, addValue);

                //if the cone is large enough, also cast rays to cells just inside of the outer arc
                // this helps fill in any holes when casting rays
                if (circleRadius >= 4)
                {
                    scan.Polar(a * PointF.G2R, circleRadius - 1);
                    scan.Offset(fromP);
                    scan.x += (fromP.x > scan.x ? +0.5f : -0.5f);
                    scan.y += (fromP.y > scan.y ? +0.5f : -0.5f);
                    scanInt.Set(
                            (int)GameMath.Gate(0, (int)Math.Floor(scan.x), Dungeon.level.Width() - 1),
                            (int)GameMath.Gate(0, (int)Math.Floor(scan.y), Dungeon.level.Height() - 1));

                    addValue = Dungeon.level.PointToCell(scanInt);
                    AddToTargetCells(targetCells, addValue);
                }
            }

            //cast a ray to each found cell, these make up the cone
            foreach (int c in targetCells)
            {
                Ballistic ray = new Ballistic(core.sourcePos, c, ballisticaParams);
                cells.UnionWith(ray.SubPath(1, ray.dist));
                rays.Add(ray);
            }

            //lastly add any cells in the core
            foreach (int c in core.SubPath(1, core.dist))
            {
                if (Dungeon.level.TrueDistance(core.sourcePos, c) <= maxDist)
                {
                    cells.Add(c);
                }
            }
        }

        // LinkedHashSet 대신에 List<int>을 사용해서, 중복 검사를 이 함수로 
        void AddToTargetCells(List<int> targetCells, int addValue)
        {
            if (targetCells.Contains(addValue) == false)
                targetCells.Add(addValue);
        }
    }
}