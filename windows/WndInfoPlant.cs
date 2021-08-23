using spdd.plants;
using spdd.tiles;

namespace spdd.windows
{
    public class WndInfoPlant : WndTitledMessage
    {
        public WndInfoPlant(Plant plant)
            : base(TerrainFeaturesTilemap.Tile(plant.pos, Dungeon.level.map[plant.pos]),
                  plant.plantName, plant.Desc())
        { }
    }
}