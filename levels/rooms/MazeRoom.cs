using spdd.levels.painters;
using spdd.levels.features;

namespace spdd.levels.rooms
{
    //TODO figure out where this should go, tunnel room type perhaps?
    public class MazeRoom : Room
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            //true = space, false = wall
            Maze.allowDiagonals = false;
            bool[,] maze = Maze.Generate(this);

            int width = maze.GetUpperBound(0) + 1;
            int height = maze.GetUpperBound(1) + 1;

            Painter.Fill(level, this, 1, Terrain.EMPTY);
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    if (maze[x, y] == Maze.FILLED)
                    {
                        Painter.Fill(level, x + left, y + top, 1, 1, Terrain.WALL);
                    }
                }
            }
        }
    }
}