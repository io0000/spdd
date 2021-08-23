using watabou.utils;
using watabou.noosa;
using watabou.noosa.tweeners;

namespace spdd.tiles
{
    public abstract class DungeonTilemap : Tilemap
    {
        public const int SIZE = 16;

        protected int[] map;

        public DungeonTilemap(string tex)
            : base(tex, new TextureFilm(tex, SIZE, SIZE))
        { }

        public override void Map(int[] data, int cols)
        {
            map = data;
            base.Map(new int[data.Length], cols);
        }

        public override void UpdateMap()
        {
            base.UpdateMap();
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = GetTileVisual(i, map[i], false);
            }
        }

        public override void UpdateMapCell(int cell)
        {
            //update in a 3x3 grid to account for neighbors which might also be affected
            if (Dungeon.level.InsideMap(cell))
            {
                base.UpdateMapCell(cell - mapWidth - 1);
                base.UpdateMapCell(cell + mapWidth + 1);
                foreach (int i in PathFinder.NEIGHBORS9)
                {
                    data[cell + i] = GetTileVisual(cell + i, map[cell + i], false);
                }
            }
            //unless we're at the level's edge, then just do the one tile.
            else
            {
                base.UpdateMapCell(cell);
                data[cell] = GetTileVisual(cell, map[cell], false);
            }
        }

        protected abstract int GetTileVisual(int pos, int tile, bool flat);

        //public int ScreenToTile(int x, int y)
        //{
        //    return ScreenToTile(x, y, false);
        //}

        //wall assist is used to make raised perspective tapping a bit easier.
        // If the pressed tile is a wall tile, the tap can be 'bumped' down into a none-wall tile.
        // currently this happens if the bottom 1/4 of the wall tile is pressed.
        public int ScreenToTile(int x, int y, bool wallAssist)
        {
            PointF p = GetCamera().ScreenToCamera(x, y).
                Offset(this.Point().Negate()).
                InvScale(SIZE);

            //snap to the edges of the tilemap
            p.x = GameMath.Gate(0, p.x, Dungeon.level.Width() - 0.001f);
            p.y = GameMath.Gate(0, p.y, Dungeon.level.Height() - 0.001f);

            int cell = (int)p.x + (int)p.y * Dungeon.level.Width();

            if (wallAssist &&
                map != null &&
                DungeonTileSheet.WallStitcheable(map[cell]))
            {
                if (cell + mapWidth < size &&
                    p.y % 1 >= 0.75f &&
                    !DungeonTileSheet.WallStitcheable(map[cell + mapWidth]))
                {
                    cell += mapWidth;
                }
            }

            return cell;
        }

        public override bool OverlapsPoint(float x, float y)
        {
            return true;
        }

        public void Discover(int pos, int oldValue)
        {
            int visual = GetTileVisual(pos, oldValue, false);
            if (visual < 0)
                return;

            Image tile = new Image(texture);
            tile.Frame(tileset.Get(GetTileVisual(pos, oldValue, false)));
            tile.Point(TileToWorld(pos));

            parent.Add(tile);

            parent.Add(new DiscoverAlphaTweener(tile));
        }

        private class DiscoverAlphaTweener : AlphaTweener
        {
            private Image tile;

            public DiscoverAlphaTweener(Image tile)
                : base(tile, 0, 0.6f)
            {
                this.tile = tile;
            }

            public override void OnComplete()
            {
                tile.KillAndErase();
                KillAndErase();
            }
        }

        public static PointF TileToWorld(int pos)
        {
            return (new PointF(pos % Dungeon.level.Width(), pos / Dungeon.level.Width())).Scale(SIZE);
        }

        public static PointF TileCenterToWorld(int pos)
        {
            return new PointF((pos % Dungeon.level.Width() + 0.5f) * SIZE, (pos / Dungeon.level.Width() + 0.5f) * SIZE);
        }

        public static PointF RaisedTileCenterToWorld(int pos)
        {
            return new PointF((pos % Dungeon.level.Width() + 0.5f) * SIZE, (pos / Dungeon.level.Width() + 0.1f) * SIZE);
        }

        public override bool OverlapsScreenPoint(int x, int y)
        {
            return true;
        }
    }
}