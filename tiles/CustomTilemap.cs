using watabou.utils;
using watabou.noosa;

namespace spdd.tiles
{
    public abstract class CustomTilemap : IBundlable
    {
        protected const int SIZE = DungeonTilemap.SIZE;

        public int tileX, tileY; //x and y coords for texture within a level
        public int tileW = 1, tileH = 1; //width and height in tiles

        protected object texture;
        protected Tilemap vis;

        public void Pos(int pos)
        {
            Pos(pos % Dungeon.level.Width(), pos / Dungeon.level.Width());
        }

        public void Pos(int tileX, int tileY)
        {
            this.tileX = tileX;
            this.tileY = tileY;
        }

        public void SetRect(int topLeft, int bottomRight)
        {
            SetRect(topLeft % Dungeon.level.Width(),
                topLeft / Dungeon.level.Width(),
                bottomRight % Dungeon.level.Width() - topLeft % Dungeon.level.Width(),
                bottomRight / Dungeon.level.Width() - topLeft / Dungeon.level.Width());
        }

        public void SetRect(int tileX, int tileY, int tileW, int tileH)
        {
            this.tileX = tileX;
            this.tileY = tileY;
            this.tileW = tileW;
            this.tileH = tileH;
        }

        //utility method for getting data for a simple image
        //assumes tileW and tileH have already been set
        protected int[] MapSimpleImage(int txX, int txY, int texW)
        {
            int[] data = new int[tileW * tileH];
            int texTileWidth = texW / SIZE;
            int x = txX, y = txY;
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = x + (texTileWidth * y);

                ++x;
                if ((x - txX) == tileW)
                {
                    x = txX;
                    ++y;
                }
            }
            return data;
        }

        public virtual Tilemap Create()
        {
            if (vis != null && vis.alive)
                vis.KillAndErase();

            vis = new TilemapCustom(texture, new TextureFilm(texture, SIZE, SIZE));
            vis.x = tileX * SIZE;
            vis.y = tileY * SIZE;
            return vis;
        }

        private class TilemapCustom : Tilemap
        {
            public TilemapCustom(object tx, TextureFilm tileset)
                : base(tx, tileset)
            { }

            protected override NoosaScript Script()
            {
                //allow lighting for custom tilemaps
                return NoosaScript.Get();
            }
        }

        //x and y here are the coordinates tapped within the tile visual
        public virtual Image Image(int tileX, int tileY)
        {
            if (vis == null)
            {
                return null;
            }
            else
            {
                return vis.Image(tileX, tileY);
            }
        }

        public virtual string Name(int tileX, int tileY)
        {
            return null;
        }

        public virtual string Desc(int tileX, int tileY)
        {
            return null;
        }

        private const string TILE_X = "tileX";
        private const string TILE_Y = "tileY";

        private const string TILE_W = "tileW";
        private const string TILE_H = "tileH";

        public virtual void RestoreFromBundle(Bundle bundle)
        {
            tileX = bundle.GetInt(TILE_X);
            tileY = bundle.GetInt(TILE_Y);

            tileW = bundle.GetInt(TILE_W);
            tileH = bundle.GetInt(TILE_H);
        }

        public virtual void StoreInBundle(Bundle bundle)
        {
            bundle.Put(TILE_X, tileX);
            bundle.Put(TILE_Y, tileY);

            bundle.Put(TILE_W, tileW);
            bundle.Put(TILE_H, tileH);
        }
    }
}