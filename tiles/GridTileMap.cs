using spdd.levels;

namespace spdd.tiles
{
    public class GridTileMap : DungeonTilemap
    {
        public GridTileMap()
            : base(Assets.Environment.VISUAL_GRID)
        {
            Map(Dungeon.level.map, Dungeon.level.Width());
        }

        private int gridSetting = -1;

        public override void UpdateMap()
        {
            gridSetting = SPDSettings.VisualGrid();
            base.UpdateMap();
        }

        protected override int GetTileVisual(int pos, int tile, bool flat)
        {
            if (gridSetting == -1 || (pos % mapWidth) % 2 != (pos / mapWidth) % 2)
            {
                return -1;
            }
            else if (DungeonTileSheet.FloorTile(tile) || tile == Terrain.HIGH_GRASS || tile == Terrain.FURROWED_GRASS)
            {
                return gridSetting;
            }
            else if (DungeonTileSheet.DoorTile(tile))
            {
                if (DungeonTileSheet.WallStitcheable(map[pos - mapWidth]))
                {
                    return 12 + gridSetting;
                }
                else if (tile == Terrain.OPEN_DOOR)
                {
                    return 8 + gridSetting;
                }
                else
                {
                    return 4 + gridSetting;
                }
            }
            else
            {
                return -1;
            }
        }
    }
}