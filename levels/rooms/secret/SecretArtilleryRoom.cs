using spdd.levels.painters;
using spdd.items;
using spdd.items.bombs;

namespace spdd.levels.rooms.secret
{
    public class SecretArtilleryRoom : SecretRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY_SP);

            Painter.Set(level, Center(), Terrain.STATUE_SP);

            for (int i = 0; i < 3; ++i)
            {
                int itemPos;
                do
                {
                    itemPos = level.PointToCell(Random());
                }
                while (level.map[itemPos] != Terrain.EMPTY_SP ||
                       level.heaps[itemPos] != null);

                if (i == 0)
                {
                    level.Drop(new Bomb.DoubleBomb(), itemPos);
                }
                else
                {
                    level.Drop(Generator.RandomMissile(), itemPos);
                }
            }

            Entrance().Set(Door.Type.HIDDEN);
        }
    }
}