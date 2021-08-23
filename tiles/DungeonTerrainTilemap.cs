using watabou.utils;
using watabou.noosa;
using spdd.levels;

namespace spdd.tiles
{
    public class DungeonTerrainTilemap : DungeonTilemap
    {
        static DungeonTerrainTilemap instance;

        public DungeonTerrainTilemap()
            : base(Dungeon.level.TilesTex())
        {
            Map(Dungeon.level.map, Dungeon.level.Width());

            instance = this;
        }

        protected override int GetTileVisual(int pos, int tile, bool flat)
        {
            int visual = DungeonTileSheet.directVisuals.Get(tile, -1);
            if (visual != -1)
                return DungeonTileSheet.GetVisualWithAlts(visual, pos);

            if (tile == Terrain.WATER)
            {
                return DungeonTileSheet.StitchWaterTile(
                    map[pos + PathFinder.CIRCLE4[0]],
                    map[pos + PathFinder.CIRCLE4[1]],
                    map[pos + PathFinder.CIRCLE4[2]],
                    map[pos + PathFinder.CIRCLE4[3]]);
            }
            else if (tile == Terrain.CHASM)
            {
                return DungeonTileSheet.StitchChasmTile(pos > mapWidth ? map[pos - mapWidth] : -1);
            }

            if (!flat)
            {
                if ((DungeonTileSheet.DoorTile(tile)))
                {
                    return DungeonTileSheet.GetRaisedDoorTile(tile, map[pos - mapWidth]);
                }
                else if (DungeonTileSheet.WallStitcheable(tile))
                {
                    return DungeonTileSheet.GetRaisedWallTile(
                        tile,
                        pos,
                        (pos + 1) % mapWidth != 0 ? map[pos + 1] : -1,
                        pos + mapWidth < size ? map[pos + mapWidth] : -1,
                        pos % mapWidth != 0 ? map[pos - 1] : -1);
                }
                else if (tile == Terrain.SIGN)
                {
                    return DungeonTileSheet.RAISED_SIGN;
                }
                else if (tile == Terrain.STATUE)
                {
                    return DungeonTileSheet.RAISED_STATUE;
                }
                else if (tile == Terrain.STATUE_SP)
                {
                    return DungeonTileSheet.RAISED_STATUE_SP;
                }
                else if (tile == Terrain.ALCHEMY)
                {
                    return DungeonTileSheet.RAISED_ALCHEMY_POT;
                }
                else if (tile == Terrain.BARRICADE)
                {
                    return DungeonTileSheet.RAISED_BARRICADE;
                }
                else if (tile == Terrain.HIGH_GRASS)
                {
                    return DungeonTileSheet.GetVisualWithAlts(
                        DungeonTileSheet.RAISED_HIGH_GRASS,
                        pos);
                }
                else if (tile == Terrain.FURROWED_GRASS)
                {
                    return DungeonTileSheet.GetVisualWithAlts(
                        DungeonTileSheet.RAISED_FURROWED_GRASS,
                        pos);
                }
                else
                {
                    return DungeonTileSheet.NULL_TILE;
                }
            }
            else
            {
                return DungeonTileSheet.GetVisualWithAlts(
                    DungeonTileSheet.directFlatVisuals[tile],
                    pos);
            }
        }

        public static Image Tile(int pos, int tile)
        {
            Image img = new Image(instance.texture);
            img.Frame(instance.tileset.Get(instance.GetTileVisual(pos, tile, true)));
            return img;
        }

        protected override bool NeedsRender(int pos)
        {
            return base.NeedsRender(pos) && data[pos] != DungeonTileSheet.WATER;
        }
    }
}