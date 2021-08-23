using watabou.noosa;
using spdd.scenes;
using spdd.ui;
using spdd.levels;
using spdd.messages;
using spdd.tiles;
using spdd.actors.blobs;

namespace spdd.windows
{
    public class WndInfoCell : Window
    {
        private const float GAP = 2;
        private const int WIDTH = 120;

        public WndInfoCell(int cell) 
            : base()
        {
            int tile = Dungeon.level.map[cell];
            if (Dungeon.level.water[cell])
                tile = Terrain.WATER;
            else if (Dungeon.level.pit[cell])
                tile = Terrain.CHASM;

            CustomTilemap customTile = null;
            Image customImage = null;
            int x = cell % Dungeon.level.Width();
            int y = cell / Dungeon.level.Width();
            foreach (CustomTilemap i in Dungeon.level.customTiles)
            {
                if ((x >= i.tileX && x < i.tileX + i.tileW) &&
                    (y >= i.tileY && y < i.tileY + i.tileH))
                {
                    if ((customImage = i.Image(x - i.tileX, y - i.tileY)) != null)
                    {
                        x -= i.tileX;
                        y -= i.tileY;
                        customTile = i;
                        break;
                    }
                }
            }

            string desc = "";

            IconTitle titlebar = new IconTitle();
            if (customTile != null)
            {
                titlebar.Icon(customImage);

                string customName = customTile.Name(x, y);
                if (!string.IsNullOrEmpty(customName))
                    titlebar.Label(customName);
                else
                    titlebar.Label(Dungeon.level.TileName(tile));

                string customDesc = customTile.Desc(x, y);
                if (!string.IsNullOrEmpty(customDesc))
                    desc += customDesc;
                else
                    desc += Dungeon.level.TileDesc(tile);
            }
            else
            {
                if (tile == Terrain.WATER)
                {
                    Image water = new Image(Dungeon.level.WaterTex());
                    water.Frame(0, 0, DungeonTilemap.SIZE, DungeonTilemap.SIZE);
                    titlebar.Icon(water);
                }
                else
                {
                    titlebar.Icon(DungeonTerrainTilemap.Tile(cell, tile));
                }
                titlebar.Label(Dungeon.level.TileName(tile));
                desc += Dungeon.level.TileDesc(tile);

            }
            titlebar.SetRect(0, 0, WIDTH, 0);
            Add(titlebar);

            RenderedTextBlock info = PixelScene.RenderTextBlock(6);
            Add(info);

            if (Dungeon.level.heroFOV[cell])
            {
                foreach (Blob blob in Dungeon.level.blobs.Values)
                {
                    if (blob.volume > 0 && 
                        blob.cur[cell] > 0 && 
                        !string.IsNullOrEmpty(blob.TileDesc()))
                    {
                        if (desc.Length > 0)
                            desc += "\n\n";

                        desc += blob.TileDesc();
                    }
                }
            }

            info.Text(desc.Length == 0 ? Messages.Get(this, "nothing") : desc);
            info.MaxWidth(WIDTH);
            info.SetPos(titlebar.Left(), titlebar.Bottom() + 2 * GAP);

            Resize(WIDTH, (int)info.Bottom() + 2);
        }
    }
}