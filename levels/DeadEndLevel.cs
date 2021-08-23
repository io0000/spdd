using watabou.utils;
using spdd.actors;
using spdd.actors.mobs;

namespace spdd.levels
{
    public class DeadEndLevel : Level
    {
        private const int SIZE = 5;

        public DeadEndLevel()
        {
            color1 = new Color(0x53, 0x4f, 0x3e, 0xff);
            color2 = new Color(0xb9, 0xd6, 0x61, 0xff);
        }

        public override string TilesTex()
        {
            return Assets.Environment.TILES_CAVES;
        }

        public override string WaterTex()
        {
            return Assets.Environment.WATER_HALLS;
        }

        public override bool Build()
        {
            SetSize(7, 7);

            for (int i = 2; i < SIZE; ++i)
            {
                for (int j = 2; j < SIZE; ++j)
                {
                    map[i * Width() + j] = Terrain.EMPTY;
                }
            }

            for (int i = 1; i <= SIZE; ++i)
            {
                map[Width() + i] =
                map[Width() * SIZE + i] =
                map[Width() * i + 1] =
                map[Width() * i + SIZE] =
                    Terrain.WATER;
            }

            entrance = SIZE * Width() + SIZE / 2 + 1;
            map[entrance] = Terrain.ENTRANCE;

            exit = 0;

            return true;
        }

        public override Mob CreateMob()
        {
            return null;
        }

        public override void CreateMobs()
        { }

        public override Actor AddRespawner()
        {
            return null;
        }

        public override void CreateItems()
        { }

        public override int RandomRespawnCell(Character ch)
        {
            return entrance - Width();
        }
    }
}