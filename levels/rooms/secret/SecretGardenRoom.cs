using watabou.utils;
using spdd.actors.blobs;
using spdd.levels.painters;
using spdd.plants;
using spdd.items.wands;

namespace spdd.levels.rooms.secret
{
    public class SecretGardenRoom : SecretRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.GRASS);

            bool[] grass = Patch.Generate(Width() - 2, Height() - 2, 0.5f, 0, true);
            for (int i = top + 1; i < bottom; ++i)
            {
                for (int j = left + 1; j < right; ++j)
                {
                    if (grass[XYToPatchCoords(j, i)])
                    {
                        level.map[i * level.Width() + j] = Terrain.HIGH_GRASS;
                    }
                }
            }

            Entrance().Set(Door.Type.HIDDEN);

            level.Plant(new Starflower.Seed(), PlantPos(level));
            level.Plant(new WandOfRegrowth.Seedpod.Seed(), PlantPos(level));
            level.Plant(new WandOfRegrowth.Dewcatcher.Seed(), PlantPos(level));

            if (Rnd.Int(2) == 0)
            {
                level.Plant(new WandOfRegrowth.Seedpod.Seed(), PlantPos(level));
            }
            else
            {
                level.Plant(new WandOfRegrowth.Dewcatcher.Seed(), PlantPos(level));
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

        protected int XYToPatchCoords(int x, int y)
        {
            return (x - left - 1) + ((y - top - 1) * (Width() - 2));
        }
    }
}