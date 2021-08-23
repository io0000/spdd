using watabou.utils;
using spdd.levels.painters;
using spdd.levels.features;
using spdd.items;

namespace spdd.levels.rooms.secret
{
    public class SecretMazeRoom : SecretRoom
    {
        public override int MinWidth()
        {
            return 14;
        }

        public override int MinHeight()
        {
            return 14;
        }

        public override int MaxWidth()
        {
            return 18;
        }

        public override int MaxHeight()
        {
            return 18;
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            //true = space, false = wall
            Maze.allowDiagonals = false;
            bool[,] maze = Maze.Generate(this);
            bool[] passable = new bool[Width() * Height()];

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
                    passable[x + Width() * y] = maze[x, y] == Maze.EMPTY;
                }
            }

            PathFinder.SetMapSize(Width(), Height());
            Point entrance = Entrance();
            int entrancePos = (entrance.x - left) + Width() * (entrance.y - top);

            PathFinder.BuildDistanceMap(entrancePos, passable);

            int bestDist = 0;
            Point bestDistP = new Point();
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] != int.MaxValue &&
                    PathFinder.distance[i] > bestDist)
                {
                    bestDist = PathFinder.distance[i];
                    bestDistP.x = (i % Width()) + left;
                    bestDistP.y = (i / Width()) + top;
                }
            }

            Item prize;
            //1 floor set higher in probability, never cursed
            do
            {
                if (Rnd.Int(2) == 0)
                {
                    prize = Generator.RandomWeapon((Dungeon.depth / 5) + 1);
                }
                else
                {
                    prize = Generator.RandomArmor((Dungeon.depth / 5) + 1);
                }
            }
            while (prize.cursed || Challenges.IsItemBlocked(prize));


            //33% chance for an extra update.
            if (Rnd.Int(3) == 0)
            {
                prize.Upgrade();
            }

            level.Drop(prize, level.PointToCell(bestDistP)).type = Heap.Type.CHEST;

            PathFinder.SetMapSize(level.Width(), level.Height());

            Entrance().Set(Door.Type.HIDDEN);
        }
    }
}