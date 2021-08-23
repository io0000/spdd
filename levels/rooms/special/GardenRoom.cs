using watabou.utils;
using spdd.levels.painters;
using spdd.plants;
using spdd.actors.blobs;

namespace spdd.levels.rooms.special
{
    public class GardenRoom : SpecialRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.HIGH_GRASS);
            Painter.Fill(level, this, 2, Terrain.GRASS);

            Entrance().Set(Door.Type.REGULAR);

            if (Dungeon.IsChallenged(Challenges.NO_FOOD))
            {
                if (Rnd.Int(2) == 0)
                {
                    level.Plant(new Sungrass.Seed(), PlantPos(level));
                }
            }
            else
            {
                int bushes = Rnd.Int(3);
                if (bushes == 0)
                {
                    level.Plant(new Sungrass.Seed(), PlantPos(level));
                }
                else if (bushes == 1)
                {
                    level.Plant(new BlandfruitBush.Seed(), PlantPos(level));
                }
                else if (Rnd.Int(5) == 0)
                {
                    level.Plant(new Sungrass.Seed(), PlantPos(level));
                    level.Plant(new BlandfruitBush.Seed(), PlantPos(level));
                }
            }

            Foliage light = (Foliage)level.GetBlob(typeof(Foliage));
            if (light == null)
            {
                light = new Foliage();
            }
            for (int i = top + 1; i < bottom; ++i)
            {
                for (int j = left + 1; j < right; ++j)
                {
                    light.Seed(level, j + level.Width() * i, 1);
                }
            }
            level.blobs[typeof(Foliage)] = light;
        }

        private int PlantPos(Level level)
        {
            int pos;
            do
            {
                pos = level.PointToCell(Random());
            }
            while (level.plants[pos] != null);

            return pos;
        }
    }
}