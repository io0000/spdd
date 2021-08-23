using System;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa;
using watabou.glwrap;
using watabou.gltextures;

namespace spdd.tiles
{
    public class FogOfWar : Image
    {
        //first index is visibility type, second is brightness level
        private static readonly Color[][] FOG_COLORS = new Color[][]
        {
            //visible
            new Color[] {
                new Color(0x00, 0x00, 0x00, 0x00),  //-1 brightness
                new Color(0x00, 0x00, 0x00, 0x00),  //0  brightness
                new Color(0x00, 0x00, 0x00, 0x00)   //1  brightness
            },
            //visited
            new Color[] {
                new Color(0x00, 0x00, 0x00, 0xCC),
                new Color(0x00, 0x00, 0x00, 0x99),
                new Color(0x00, 0x00, 0x00, 0x55)
            },
            //mapped
            new Color[] {
                new Color(0x11, 0x22, 0x44, 0xCC),
                new Color(0x19, 0x33, 0x66, 0x99),
                new Color(0x22, 0x44, 0x88, 0x55)
            },
            //invisible
            new Color[] {
                new Color(0x00, 0x00, 0x00, 0xFF),
                new Color(0x00, 0x00, 0x00, 0xFF),
                new Color(0x00, 0x00, 0x00, 0xFF)
            }
        };

        private const int VISIBLE = 0;
        private const int VISITED = 1;
        private const int MAPPED = 2;
        private const int INVISIBLE = 3;

        private int mapWidth;
        private int mapHeight;
        private int mapLength;

        private int pWidth;
        private int pHeight;

        private int width2;
        private int height2;

        private List<Rect> toUpdate;
        private List<Rect> updating;

        //should be divisible by 2
        private const int PIX_PER_TILE = 2;

        /*
        TODO currently the center of each fox pixel is aligned with the inside of a cell
        might be possible to create a better fog effect by aligning them with edges of a cell,
        similar to the existing fog effect in vanilla (although probably with more precision)
        the advantage here is that it may be possible to totally eliminate the tile blocking map
        */

        public FogOfWar(int mapWidth, int mapHeight)
        {
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            mapLength = mapHeight * mapWidth;

            pWidth = mapWidth * PIX_PER_TILE;
            pHeight = mapHeight * PIX_PER_TILE;

            width2 = 1;
            while (width2 < pWidth)
            {
                width2 <<= 1;
            }

            height2 = 1;
            while (height2 < pHeight)
            {
                height2 <<= 1;
            }

            float size = DungeonTilemap.SIZE / PIX_PER_TILE;
            width = width2 * size;
            height = height2 * size;

            //TODO might be nice to compartmentalize the pixmap access and modification into texture/texturecache
            Pixmap px = new Pixmap(width2, height2);
            px.SetBlending(Pixmap.Blending.None);
            px.SetColor(new Color(0x00, 0x00, 0x00, 0xFF)); // RGBA8888 format 
            px.Fill();
            Texture tx = new Texture(px, watabou.glwrap.Texture.LINEAR, watabou.glwrap.Texture.CLAMP, false);
            TextureCache.Add(typeof(FogOfWar), tx);
            Texture(tx);

            scale.Set(size, size);

            toUpdate = new List<Rect>();
            toUpdate.Add(new Rect(0, 0, mapWidth, mapHeight));
        }

        public void UpdateFog()
        {
            toUpdate.Clear();
            toUpdate.Add(new Rect(0, 0, mapWidth, mapHeight));
        }

        public void UpdateFog(Rect update)
        {
            foreach (Rect r in toUpdate.ToArray())
            {
                if (!r.Intersect(update).IsEmpty())
                {
                    toUpdate.Remove(r);
                    toUpdate.Add(r.Union(update));
                    return;
                }
            }
            toUpdate.Add(update);
        }

        public void UpdateFog(int cell, int radius)
        {
            Rect update = new Rect((cell % mapWidth) - radius, (cell / mapWidth) - radius, (cell % mapWidth) - radius + 1 + 2 * radius, (cell / mapWidth) - radius + 1 + 2 * radius);
            update.left = Math.Max(0, update.left);
            update.top = Math.Max(0, update.top);
            update.right = Math.Min(mapWidth, update.right);
            update.bottom = Math.Min(mapHeight, update.bottom);
            if (update.IsEmpty())
                return;
            UpdateFog(update);
        }

        public void UpdateFogArea(int x, int y, int w, int h)
        {
            UpdateFog(new Rect(x, y, x + w, y + h));
        }

        private void MoveToUpdating()
        {
            updating = toUpdate;
            toUpdate = new List<Rect>();
        }

        private bool[] visibleFog;
        private bool[] visited;
        private bool[] mapped;
        private int brightness;

        private void UpdateTexture(bool[] visible, bool[] visited, bool[] mapped)
        {
            this.visibleFog = visible;
            this.visited = visited;
            this.mapped = mapped;
            this.brightness = SPDSettings.Brightness() + 1;

            MoveToUpdating();

            bool fullUpdate = false;
            if (updating.Count == 1)
            {
                Rect update = updating[0];
                if (update.Height() == mapHeight && update.Width() == mapWidth)
                {
                    fullUpdate = true;
                }
            }

            Pixmap fog = texture.bitmap;

            int cell;

            foreach (Rect update in updating)
            {
                for (int i = update.top; i < update.bottom; ++i)
                {
                    cell = mapWidth * i + update.left;
                    for (int j = update.left; j < update.right; ++j)
                    {
                        if (cell >= Dungeon.level.Length())
                            continue; //do nothing

                        if (!Dungeon.level.discoverable[cell] ||
                            (!visible[cell] && !visited[cell] && !mapped[cell]))
                        {
                            //we skip filling cells here if it isn't a full update
                            // because they must already be dark
                            if (fullUpdate)
                                FillCell(fog, j, i, FOG_COLORS[INVISIBLE][brightness]);
                            ++cell;
                            continue;
                        }

                        //wall tiles
                        if (Wall(cell))
                        {
                            //always dark if nothing is beneath them
                            if (cell + mapWidth >= mapLength)
                            {
                                FillCell(fog, j, i, FOG_COLORS[INVISIBLE][brightness]);
                            }
                            //internal wall tiles, need to check both the left and right side,
                            // to account for only one half of them being seen
                            else if (Wall(cell + mapWidth))
                            {
                                //left side
                                if (cell % mapWidth != 0)
                                {
                                    //picks the darkest fog between current tile, left, and below-left(if left is a wall).
                                    if (Wall(cell - 1))
                                    {
                                        //if below-left is also a wall, then we should be dark no matter what.
                                        if (Wall(cell + mapWidth - 1))
                                        {
                                            FillLeft(fog, j, i, FOG_COLORS[INVISIBLE][brightness]);
                                        }
                                        else
                                        {
                                            FillLeft(fog, j, i, FOG_COLORS[Math.Max(GetCellFog(cell), Math.Max(GetCellFog(cell + mapWidth - 1), GetCellFog(cell - 1)))][brightness]);
                                        }
                                    }
                                    else
                                    {
                                        FillLeft(fog, j, i, FOG_COLORS[Math.Max(GetCellFog(cell), GetCellFog(cell - 1))][brightness]);
                                    }
                                }
                                else
                                {
                                    FillLeft(fog, j, i, FOG_COLORS[INVISIBLE][brightness]);
                                }

                                //right side
                                if ((cell + 1) % mapWidth != 0)
                                {
                                    //picks the darkest fog between current tile, right, and below-right(if right is a wall).
                                    if (Wall(cell + 1))
                                    {
                                        //if below-right is also a wall, then we should be dark no matter what.
                                        if (Wall(cell + mapWidth + 1))
                                        {
                                            FillRight(fog, j, i, FOG_COLORS[INVISIBLE][brightness]);
                                        }
                                        else
                                        {
                                            FillRight(fog, j, i, FOG_COLORS[Math.Max(GetCellFog(cell), Math.Max(GetCellFog(cell + mapWidth + 1), GetCellFog(cell + 1)))][brightness]);
                                        }
                                    }
                                    else
                                    {
                                        FillRight(fog, j, i, FOG_COLORS[Math.Max(GetCellFog(cell), GetCellFog(cell + 1))][brightness]);
                                    }
                                }
                                else
                                {
                                    FillRight(fog, j, i, FOG_COLORS[INVISIBLE][brightness]);
                                }
                            }
                            //camera-facing wall tiles
                            //darkest between themselves and the tile below them
                            else
                            {
                                FillCell(fog, j, i, FOG_COLORS[Math.Max(GetCellFog(cell), GetCellFog(cell + mapWidth))][brightness]);
                            }
                        }
                        //other tiles, just their direct value
                        else
                        {
                            FillCell(fog, j, i, FOG_COLORS[GetCellFog(cell)][brightness]);
                        }

                        ++cell;
                    }
                }

            }

            texture.Bitmap(fog);
        }

        private bool Wall(int cell)
        {
            return DungeonTileSheet.WallStitcheable(Dungeon.level.map[cell]);
        }

        private int GetCellFog(int cell)
        {
            if (visibleFog[cell])
            {
                return VISIBLE;
            }
            else if (visited[cell])
            {
                return VISITED;
            }
            else if (mapped[cell])
            {
                return MAPPED;
            }
            else
            {
                return INVISIBLE;
            }
        }

        private void FillLeft(Pixmap fog, int x, int y, Color color)
        {
            //fog.setColor((color << 8) | (color >>> 24));
            fog.SetColor(color);

            fog.FillRectangle(x * PIX_PER_TILE, y * PIX_PER_TILE, PIX_PER_TILE / 2, PIX_PER_TILE);
        }

        private void FillRight(Pixmap fog, int x, int y, Color color)
        {
            //fog.setColor((color << 8) | (color >>> 24));
            fog.SetColor(color);

            fog.FillRectangle(x * PIX_PER_TILE + PIX_PER_TILE / 2, y * PIX_PER_TILE, PIX_PER_TILE / 2, PIX_PER_TILE);
        }

        private void FillCell(Pixmap fog, int x, int y, Color color)
        {
            //fog.setColor((color << 8) | (color >>> 24));
            fog.SetColor(color);

            fog.FillRectangle(x * PIX_PER_TILE, y * PIX_PER_TILE, PIX_PER_TILE, PIX_PER_TILE);
        }

        protected override NoosaScript Script()
        {
            return NoosaScriptNoLighting.Get();
        }

        public override void Draw()
        {
            if (toUpdate.Count > 0)
            {
                UpdateTexture(Dungeon.level.heroFOV, Dungeon.level.visited, Dungeon.level.mapped);
            }

            base.Draw();
        }

        public override void Destroy()
        {
            base.Destroy();
            if (texture != null)
            {
                TextureCache.Remove(typeof(FogOfWar));
            }
        }
    }
}