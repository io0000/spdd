using System;
using watabou.noosa;
using spdd.levels;

namespace spdd.tiles
{
    public class WallBlockingTilemap : Tilemap
    {
        public const int SIZE = 16;

        private const int CLEARED = -2;
        private const int BLOCK_NONE = -1;
        //private const int BLOCK_RIGHT = 0;
        //private const int BLOCK_LEFT = 1;
        private const int BLOCK_ALL = 2;
        private const int BLOCK_BELOW = 3;

        public WallBlockingTilemap()
            : base(Assets.Environment.WALL_BLOCKING, new TextureFilm(Assets.Environment.WALL_BLOCKING, SIZE, SIZE))
        {
            Map(new int[Dungeon.level.Length()], Dungeon.level.Width());
        }

        public override void UpdateMap()
        {
            base.UpdateMap();
            data = new int[size]; //clears all values, including cleared tiles

            for (int cell = 0; cell < data.Length; ++cell)
            {
                //force all top/bottom row, and none-discoverable cells to cleared
                if (!Dungeon.level.discoverable[cell] ||
                    (cell - mapWidth) <= 0 ||
                    (cell + mapWidth) >= size)
                {
                    data[cell] = CLEARED;
                }
                else
                {
                    UpdateMapCell(cell);
                }
            }
        }

        private int curr;

        public override void UpdateMapCell(int cell)
        {
            //FIXME this is to address the wall blocking looking odd on the new yog floor.
            // The true solution is to improve the fog of war so the blockers aren't necessary.
            if (Dungeon.level is NewHallsBossLevel)
            {
                data[cell] = CLEARED;
                base.UpdateMapCell(cell);
                return;
            }

            //TODO should doors be considered? currently the blocking is a bit permissive around doors

            //non-wall tiles
            if (!Wall(cell))
            {
                //clear empty floor tiles and cells which are visible
                if (!FogHidden(cell) || !Wall(cell + mapWidth))
                {
                    curr = CLEARED;

                    //block wall overhang if:
                    //- There are cells 2x below
                    //- The cell below is a wall and visible
                    //- All of left, below-left, right, below-right is either a wall or hidden
                }
                else if (!FogHidden(cell + mapWidth) &&
                    (FogHidden(cell - 1) || Wall(cell - 1)) &&
                    (FogHidden(cell + 1) || Wall(cell + 1)) &&
                    (FogHidden(cell - 1 + mapWidth) || Wall(cell - 1 + mapWidth)) &&
                    (FogHidden(cell + 1 + mapWidth) || Wall(cell + 1 + mapWidth)))
                {
                    curr = BLOCK_BELOW;
                }
                else
                {
                    curr = BLOCK_NONE;
                }
            }
            //wall tiles
            else
            {
                //camera-facing wall tiles
                if (!Wall(cell + mapWidth))
                {
                    //Block a camera-facing wall if:
                    //- the cell above, above-left, or above-right is not a wall, visible, and has a wall below
                    //- none of the remaining 5 neighbor cells are both not a wall and visible

                    //if all 3 above are wall we can shortcut and just clear the cell
                    if (Wall(cell - 1 - mapWidth) && Wall(cell - mapWidth) && Wall(cell + 1 - mapWidth))
                    {
                        curr = CLEARED;
                    }
                    else if ((!Wall(cell - 1 - mapWidth) && !FogHidden(cell - 1 - mapWidth) && Wall(cell - 1)) ||
                             (!Wall(cell - mapWidth) && !FogHidden(cell - mapWidth)) ||
                             (!Wall(cell + 1 - mapWidth) && !FogHidden(cell + 1 - mapWidth) && Wall(cell + 1)))
                    {
                        if (!FogHidden(cell + mapWidth) || 
                            (!Wall(cell - 1) && !FogHidden(cell - 1)) || 
                            (!Wall(cell - 1 + mapWidth) && !FogHidden(cell - 1 + mapWidth)) || 
                            (!Wall(cell + 1) && !FogHidden(cell + 1)) || 
                            (!Wall(cell + 1 + mapWidth) && !FogHidden(cell + 1 + mapWidth)))
                        {
                            curr = CLEARED;
                        }
                        else
                        {
                            curr = BLOCK_ALL;
                        }
                    }
                    else
                    {
                        curr = BLOCK_NONE;
                    }
                }
                //internal wall tiles
                else
                {
                    //Block the side of an internal wall if:
                    //- the cell above, below, or the cell itself is visible
                    //and all of the following are NOT true:
                    //- the cell has no neighbors on that side
                    //- the top-side neighbor is visible and the side neighbor isn't a wall.
                    //- the side neighbor is both not a wall and visible
                    //- the bottom-side neighbor is both not a wall and visible

                    curr = BLOCK_NONE;

                    if (!FogHidden(cell - mapWidth) ||
                        !FogHidden(cell) ||
                        !FogHidden(cell + mapWidth))
                    {
                        //right side
                        if (((cell + 1) % mapWidth == 0) ||
                            (!Wall(cell + 1) && !FogHidden(cell + 1 - mapWidth)) ||
                            (!Wall(cell + 1) && !FogHidden(cell + 1)) ||
                            (!Wall(cell + 1 + mapWidth) && !FogHidden(cell + 1 + mapWidth)))
                        {
                            //do nothing
                        }
                        else
                        {
                            curr += 1;
                        }

                        //left side
                        if ((cell % mapWidth == 0) ||
                            (!Wall(cell - 1) && !FogHidden(cell - 1 - mapWidth)) ||
                            (!Wall(cell - 1) && !FogHidden(cell - 1)) ||
                            (!Wall(cell - 1 + mapWidth) && !FogHidden(cell - 1 + mapWidth)))
                        {
                            //do nothing
                        }
                        else
                        {
                            curr += 2;
                        }

                        if (curr == BLOCK_NONE)
                        {
                            curr = CLEARED;
                        }
                    }
                }
            }

            if (data[cell] != curr)
            {
                data[cell] = curr;
                base.UpdateMapCell(cell);
            }
        }

        private bool FogHidden(int cell)
        {
            if (!Dungeon.level.visited[cell] && !Dungeon.level.mapped[cell])
            {
                return true;
            }
            else if (Wall(cell) &&
                cell + mapWidth < size &&
                !Wall(cell + mapWidth) &&
                !Dungeon.level.visited[cell + mapWidth] &&
                !Dungeon.level.mapped[cell + mapWidth])
            {
                return true;
            }
            return false;
        }

        private bool Wall(int cell)
        {
            return DungeonTileSheet.WallStitcheable(Dungeon.level.map[cell]);
        }

        //private bool Door(int cell)
        //{
        //    return DungeonTileSheet.DoorTile(Dungeon.level.map[cell]);
        //}

        public void UpdateArea(int cell, int radius)
        {
            int l = cell % mapWidth - radius;
            int t = cell / mapWidth - radius;
            int r = cell % mapWidth + radius;
            int b = cell / mapWidth + radius;
            UpdateArea(Math.Max(0, l), Math.Max(0, t), Math.Min(mapWidth - 1, r - l), Math.Min(mapHeight - 1, b - t));
        }

        public void UpdateArea(int x, int y, int w, int h)
        {
            int cell;
            for (int i = x; i <= x + w; ++i)
            {
                for (int j = y; j <= y + h; ++j)
                {
                    cell = i + j * mapWidth;
                    if (cell < data.Length && data[cell] != CLEARED)
                    {
                        UpdateMapCell(cell);
                    }
                }
            }
        }
    }
}