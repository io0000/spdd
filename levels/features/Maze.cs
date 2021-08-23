using System;
using watabou.utils;
using spdd.levels.rooms;

namespace spdd.levels.features
{
    public class Maze
    {
        public const bool EMPTY = false;
        public const bool FILLED = true;

        public static bool[,] Generate(Room r)
        {
            // boolean[][] maze = new boolean[4][2];
            //maze.length = 4
            //maze[0].length = 2

            int w = r.Width();
            int h = r.Height();
            bool[,] maze = new bool[w, h];

            for (int x = 0; x < w; ++x) // maze.length
            {
                for (int y = 0; y < h; ++y)  // maze[0].length
                {
                    if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                    {
                        maze[x, y] = FILLED;
                    }
                }
            }

            //set spaces where there are doors
            foreach (var pair in r.connected)
            {
                Room.Door d = pair.Value;
                int i = d.x - r.left;
                int j = d.y - r.top;
                maze[i, j] = EMPTY;
            }

            return Generate(maze);
        }

        public static bool[,] Generate(Rect r)
        {
            return Generate(r.Width() + 1, r.Height() + 1);
        }

        public static bool[,] Generate(Rect r, int[] terrain, int width, int filledTerrainType)
        {
            int w = r.Width();
            int h = r.Height();
            bool[,] maze = new bool[w, h];

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    if (terrain[x + r.left + (y + r.top) * width] == filledTerrainType)
                    {
                        maze[x, y] = FILLED;
                    }
                }
            }

            return Generate(maze);
        }

        public static bool[,] Generate(int width, int height)
        {
            return Generate(new bool[width, height]);
        }

        public static bool[,] Generate(bool[,] maze)
        {
            int fails = 0;
            int x, y, moves;
            int[] mov;

            int width = maze.GetUpperBound(0) + 1;
            int height = maze.GetUpperBound(1) + 1;

            while (fails < 2500)
            {
                //find a random wall point
                do
                {
                    x = Rnd.Int(width);
                    y = Rnd.Int(height);
                }
                while (!maze[x, y]);

                //decide on how we're going to move
                mov = DecideDirection(maze, x, y);
                if (mov == null)
                {
                    ++fails;
                }
                else
                {
                    fails = 0;
                    moves = 0;
                    do
                    {
                        x += mov[0];
                        y += mov[1];
                        maze[x, y] = FILLED;
                        ++moves;
                    }
                    while (Rnd.Int(moves) == 0 && CheckValidMove(maze, x, y, mov));
                }
            }

            return maze;
        }

        private static int[] DecideDirection(bool[,] maze, int x, int y)
        {
            //attempts to move up
            if (Rnd.Int(4) == 0 && CheckValidMove(maze, x, y, new int[] { 0, -1 }))
            {
                return new int[] { 0, -1 };
            }

            //attempts to move right
            if (Rnd.Int(3) == 0 && CheckValidMove(maze, x, y, new int[] { 1, 0 }))
            {
                return new int[] { 1, 0 };
            }

            //attempts to move down
            if (Rnd.Int(2) == 0 && CheckValidMove(maze, x, y, new int[] { 0, 1 }))
            {
                return new int[] { 0, 1 };
            }

            //attempts to move left
            if (CheckValidMove(maze, x, y, new int[] { -1, 0 }))
            {
                return new int[] { -1, 0 };
            }

            return null;
        }

        public static bool allowDiagonals;

        private static bool CheckValidMove(bool[,] maze, int x, int y, int[] mov)
        {
            int sideX = 1 - Math.Abs(mov[0]);
            int sideY = 1 - Math.Abs(mov[1]);

            x += mov[0];
            y += mov[1];

            int width = maze.GetUpperBound(0) + 1;
            int height = maze.GetUpperBound(1) + 1;

            if (x <= 0 || x >= width - 1 || y <= 0 || y >= height - 1)
            {
                return false;
            }
            else if (maze[x, y] || maze[x + sideX, y + sideY] || maze[x - sideX, y - sideY])
            {
                return false;
            }

            x += mov[0];
            y += mov[1];

            if (x <= 0 || x >= width - 1 || y <= 0 || y >= height - 1)
            {
                return false;
            }
            else if (maze[x, y])
            {
                return false;
            }
            else if (!allowDiagonals && (maze[x + sideX, y + sideY] || maze[x - sideX, y - sideY]))
            {
                return false;
            }

            return true;
        }
    }
}