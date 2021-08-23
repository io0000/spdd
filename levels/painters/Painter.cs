using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.levels.rooms;

namespace spdd.levels.painters
{
    public abstract class Painter
    {
        //If painters require additional parameters, they should
        // request them in their constructor or other methods

        //Painters take a level and its collection of rooms, and paint all the specific tile values
        public abstract bool Paint(Level level, List<Room> rooms);

        // Static methods

        public static void Set(Level level, int cell, int value)
        {
            level.map[cell] = value;
        }

        public static void Set(Level level, int x, int y, int value)
        {
            Set(level, x + y * level.Width(), value);
        }

        public static void Set(Level level, Point p, int value)
        {
            Set(level, p.x, p.y, value);
        }

        public static void Fill(Level level, int x, int y, int w, int h, int value)
        {
            int width = level.Width();

            int pos = y * width + x;
            for (int i = y; i < y + h; ++i, pos += width)
            {
                //Arrays.fill(level.map, pos, pos + w, value);
                //for (int j = pos; j < pos + w; ++j)
                //    level.map[j] = value;
                Array.Fill(level.map, value, pos, w);
            }
        }

        public static void Fill(Level level, Rect rect, int value)
        {
            Fill(level, rect.left, rect.top, rect.Width(), rect.Height(), value);
        }

        public static void Fill(Level level, Rect rect, int m, int value)
        {
            Fill(level, rect.left + m, rect.top + m, rect.Width() - m * 2, rect.Height() - m * 2, value);
        }

        //public static void Fill(Level level, Rect rect, int l, int t, int r, int b, int value)
        //{
        //    Fill(level, rect.left + l, rect.top + t, rect.Width() - (l + r), rect.Height() - (t + b), value);
        //}

        public static void DrawLine(Level level, Point from, Point to, int value)
        {
            float x = from.x;
            float y = from.y;
            float dx = to.x - from.x;
            float dy = to.y - from.y;

            bool movingbyX = Math.Abs(dx) >= Math.Abs(dy);
            //normalize
            if (movingbyX)
            {
                dy /= Math.Abs(dx);
                dx /= Math.Abs(dx);
            }
            else
            {
                dx /= Math.Abs(dy);
                dy /= Math.Abs(dy);
            }

            Set(level, (int)Math.Round(x, MidpointRounding.AwayFromZero), (int)Math.Round(y, MidpointRounding.AwayFromZero), value);
            while ((movingbyX && to.x != x) || (!movingbyX && to.y != y))
            {
                x += dx;
                y += dy;
                Set(level, (int)Math.Round(x, MidpointRounding.AwayFromZero), (int)Math.Round(y, MidpointRounding.AwayFromZero), value);
            }
        }

        public static void FillEllipse(Level level, Rect rect, int value)
        {
            FillEllipse(level, rect.left, rect.top, rect.Width(), rect.Height(), value);
        }

        public static void FillEllipse(Level level, Rect rect, int m, int value)
        {
            FillEllipse(level, rect.left + m, rect.top + m, rect.Width() - m * 2, rect.Height() - m * 2, value);
        }

        public static void FillEllipse(Level level, int x, int y, int w, int h, int value)
        {
            //radii
            double radH = h / 2f;
            double radW = w / 2f;

            //fills each row of the ellipse from top to bottom
            for (int i = 0; i < h; ++i)
            {
                //y coordinate of the row for determining ellipsis width
                //always want to test the middle of a tile, hence the 0.5 shift
                double rowY = -radH + 0.5 + i;

                //equation is derived from ellipsis formula: y^2/radH^2 + x^2/radW^2 = 1
                //solves for x and then doubles to get the width
                double rowW = 2.0 * Math.Sqrt((radW * radW) * (1.0 - (rowY * rowY) / (radH * radH)));

                //need to round to nearest even or odd number, depending on width
                if (w % 2 == 0)
                {
                    rowW = Math.Round(rowW / 2.0, MidpointRounding.AwayFromZero) * 2.0;
                }
                else
                {
                    rowW = Math.Floor(rowW / 2.0) * 2.0;
                    ++rowW;
                }

                int cell = x + (w - (int)rowW) / 2 + ((y + i) * level.Width());
                //Arrays.fill(level.map, cell, cell + (int)rowW, value);
                //for (int j = cell; j < cell + (int)rowW; ++j)
                //    level.map[j] = value;
                Array.Fill(level.map, value, cell, (int)rowW);
            }
        }

        //public static void FillDiamond(Level level, Rect rect, int value)
        //{
        //    FillDiamond(level, rect.left, rect.top, rect.Width(), rect.Height(), value);
        //}

        public static void FillDiamond(Level level, Rect rect, int m, int value)
        {
            FillDiamond(level, rect.left + m, rect.top + m, rect.Width() - m * 2, rect.Height() - m * 2, value);
        }

        public static void FillDiamond(Level level, int x, int y, int w, int h, int value)
        {
            //we want the end width to be w, and the width will grow by a total of (h-2 - h%2)
            int diamondWidth = w - (h - 2 - h % 2);
            //but starting width cannot be smaller than 2 on even width, 3 on odd width.
            diamondWidth = Math.Max(diamondWidth, w % 2 == 0 ? 2 : 3);

            for (int i = 0; i <= h; ++i)
            {
                Painter.Fill(level, x + (w - diamondWidth) / 2, y + i, diamondWidth, h - 2 * i, value);
                diamondWidth += 2;
                if (diamondWidth > w)
                    break;
            }
        }

        public static Point DrawInside(Level level, Room room, Point from, int n, int value)
        {
            Point step = new Point();
            if (from.x == room.left)
            {
                step.Set(+1, 0);
            }
            else if (from.x == room.right)
            {
                step.Set(-1, 0);
            }
            else if (from.y == room.top)
            {
                step.Set(0, +1);
            }
            else if (from.y == room.bottom)
            {
                step.Set(0, -1);
            }

            Point p = new Point(from).Offset(step);
            for (int i = 0; i < n; ++i)
            {
                if (value != -1)
                {
                    Set(level, p, value);
                }
                p.Offset(step);
            }

            return p;
        }
    }
}