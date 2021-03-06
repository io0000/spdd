using System;
using spdd.utils;

//based on: http://www.roguebasin.com/index.php?title=FOV_using_recursive_shadowcasting
namespace spdd.mechanics
{
    public class ShadowCaster
    {
        public const int MAX_DISTANCE = 12;

        //max length of rows as FOV moves out, for each FOV distance
        //This is used to make the overall FOV circular, instead of square
        public static readonly int[][] rounding;

        static ShadowCaster()
        {
            rounding = new int[MAX_DISTANCE + 1][];
            for (int i = 1; i <= MAX_DISTANCE; ++i)
            {
                rounding[i] = new int[i + 1];
                for (var j = 1; j <= i; ++j)
                {
                    //testing the middle of a cell, so we use i + 0.5
                    int value = (int)Math.Min(j,
                        Math.Round((i + 0.5) * Math.Cos(Math.Asin(j / (i + 0.5))), MidpointRounding.AwayFromZero));
                    rounding[i][j] = value;
                }
            }
        }

        public static void CastShadow(int x, int y, bool[] fieldOfView, bool[] blocking, int distance)
        {
            if (distance >= MAX_DISTANCE)
                distance = MAX_DISTANCE;

            BArray.SetFalse(fieldOfView);

            //set source cell to true
            fieldOfView[y * Dungeon.level.Width() + x] = true;

            //scans octants, clockwise
            try
            {
                ScanOctant(distance, fieldOfView, blocking, 1, x, y, 0.0, 1.0, +1, -1, false);
                ScanOctant(distance, fieldOfView, blocking, 1, x, y, 0.0, 1.0, -1, +1, true);
                ScanOctant(distance, fieldOfView, blocking, 1, x, y, 0.0, 1.0, +1, +1, true);
                ScanOctant(distance, fieldOfView, blocking, 1, x, y, 0.0, 1.0, +1, +1, false);
                ScanOctant(distance, fieldOfView, blocking, 1, x, y, 0.0, 1.0, -1, +1, false);
                ScanOctant(distance, fieldOfView, blocking, 1, x, y, 0.0, 1.0, +1, -1, true);
                ScanOctant(distance, fieldOfView, blocking, 1, x, y, 0.0, 1.0, -1, -1, true);
                ScanOctant(distance, fieldOfView, blocking, 1, x, y, 0.0, 1.0, -1, -1, false);
            }
            catch (Exception e)
            {
                ShatteredPixelDungeonDash.ReportException(e);
                BArray.SetFalse(fieldOfView);
            }
        }

        //scans a single 45 degree octant of the FOV.
        //This can add up to a whole FOV by mirroring in X(mX), Y(mY), and X=Y(mXY)
        private static void ScanOctant(int distance, bool[] fov, bool[] blocking, int row,
                                       int x, int y, double lSlope, double rSlope,
                                       int mX, int mY, bool mXY)
        {
            //if we have negative space to traverse, just quit.
            if (rSlope < lSlope)
                return;

            bool inBlocking = false;
            int start, end;
            int col;

            //calculations are offset by 0.5 because FOV is coming from the center of the source cell

            //for each row, starting with the current one
            for (; row <= distance; ++row)
            {
                //we offset by slightly less than 0.5 to account for slopes just touching a cell
                if (lSlope == 0)
                    start = 0;
                else
                    start = (int)Math.Floor((row - 0.5) * lSlope + 0.499);

                if (rSlope == 1)
                {
                    end = rounding[distance][row];
                }
                else
                {
                    end = Math.Min(
                        rounding[distance][row],
                        (int)Math.Ceiling((row + 0.5) * rSlope - 0.499));
                }

                //coordinates of source
                int cell = x + y * Dungeon.level.Width();

                //plus coordinates of current cell (including mirroring in x, y, and x=y)
                if (mXY)
                    cell += mX * start * Dungeon.level.Width() + mY * row;
                else
                    cell += mX * start + mY * row * Dungeon.level.Width();

                //for each column in this row, which
                for (col = start; col <= end; ++col)
                {
                    fov[cell] = true;

                    if (blocking[cell])
                    {
                        if (!inBlocking)
                        {
                            inBlocking = true;

                            //start a new scan, 1 row deeper, ending at the left side of current cell
                            if (col != start)
                            {
                                ScanOctant(distance, fov, blocking, row + 1, x, y, lSlope,
                                        //change in x over change in y
                                        (col - 0.5) / (row + 0.5),
                                        mX, mY, mXY);
                            }
                        }
                    }
                    else
                    {
                        if (inBlocking)
                        {
                            inBlocking = false;

                            //restrict current scan to the left side of current cell for future rows

                            //change in x over change in y
                            lSlope = (col - 0.5) / (row - 0.5);
                        }
                    }

                    if (!mXY)
                        cell += mX;
                    else
                        cell += mX * Dungeon.level.Width();
                }

                //if the row ends in a blocking cell, this scan is finished.
                if (inBlocking)
                    return;
            }
        }
    }
}