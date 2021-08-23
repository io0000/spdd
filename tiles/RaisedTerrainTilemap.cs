using spdd.levels;

namespace spdd.tiles
{
    public class RaisedTerrainTilemap : DungeonTilemap
    {
        public RaisedTerrainTilemap()
            : base(Dungeon.level.TilesTex())
        {
            Map(Dungeon.level.map, Dungeon.level.Width());
        }

        protected override int GetTileVisual(int pos, int tile, bool flat)
        {
            if (flat)
                return -1;

            if (tile == Terrain.HIGH_GRASS)
            {
                return DungeonTileSheet.GetVisualWithAlts(DungeonTileSheet.RAISED_HIGH_GRASS, pos) + 2;
            }
            else if (tile == Terrain.FURROWED_GRASS)
            {
                return DungeonTileSheet.GetVisualWithAlts(DungeonTileSheet.RAISED_FURROWED_GRASS, pos) + 2;
            }

            return -1;
        }
    }
}