using watabou.gltextures;
using watabou.glwrap;
using watabou.utils;

namespace watabou.noosa
{
    public class Tilemap : Visual
    {
        protected Texture texture;
        protected TextureFilm tileset;

        protected int[] data;  // Level::map
        protected int mapWidth;
        protected int mapHeight;
        protected int size;

        private float cellW;
        private float cellH;

        //protected float[] vertices;
        //protected FloatBuffer quads;
        //protected Vertexbuffer buffer;
        private float[] quads;

        private Rect updated;
        //private bool fullUpdate;
        private Rect updating;
        private int topLeftUpdating;
        //private int bottomRightUpdating;

        public Tilemap(object tx, TextureFilm tileset)
            : base(0.0f, 0.0f, 0.0f, 0.0f)
        {
            texture = TextureCache.Get(tx);
            this.tileset = tileset;

            RectF r = tileset.Get(0);
            cellW = tileset.Width(r);
            cellH = tileset.Height(r);

            //vertices = new float[16];

            updated = new Rect();
        }

        public virtual void Map(int[] data, int cols)
        {
            this.data = data;

            mapWidth = cols;
            mapHeight = data.Length / cols;
            size = mapWidth * mapHeight;

            width = cellW * mapWidth;
            height = cellH * mapHeight;

            //quads = Quad.CreateSet(size);
            //  return ByteBuffer.AllocateDirect(size * 16 * sizeof(float)).Order(ByteOrder.NativeOrder()).AsFloatBuffer();
            quads = new float[size * 16];

            UpdateMap();
        }

        public Image Image(int x, int y)
        {
            int pos = x + mapWidth * y;

            if (!NeedsRender(pos))
            {
                return null;
            }
            else
            {
                Image img = new Image(texture);
                img.Frame(tileset.Get(data[pos]));
                return img;
            }
        }

        //forces a full update, including new buffer
        public virtual void UpdateMap()
        {
            updated.Set(0, 0, mapWidth, mapHeight);
            //fullUpdate = true;
        }

        public virtual void UpdateMapCell(int cell)
        {
            updated.Union(cell % mapWidth, cell / mapWidth);
        }

        private void MoveToUpdating()
        {
            updating = new Rect(updated);
            updated.SetEmpty();
        }

        public void UpdateVertices()
        {
            MoveToUpdating();

            float x1, y1, x2, y2;
            int pos;
            RectF uv;

            y1 = cellH * updating.top;
            y2 = y1 + cellH;
            int index = 0;

            for (int i = updating.top; i < updating.bottom; ++i)
            {
                x1 = cellW * updating.left;
                x2 = x1 + cellW;

                pos = i * mapWidth + updating.left;
                //quads.Position(16 * pos);
                index = 16 * pos;

                for (int j = updating.left; j < updating.right; ++j)
                {
                    if (topLeftUpdating == -1)
                        topLeftUpdating = pos;

                    //bottomRightUpdating = pos + 1;

                    // quads.position(pos*16);

                    uv = tileset.Get(data[pos]);

                    if (NeedsRender(pos) && uv != null)
                    {
                        quads[index + 0] = x1;
                        quads[index + 1] = y1;

                        quads[index + 2] = uv.left;
                        quads[index + 3] = uv.top;

                        quads[index + 4] = x2;
                        quads[index + 5] = y1;

                        quads[index + 6] = uv.right;
                        quads[index + 7] = uv.top;

                        quads[index + 8] = x2;
                        quads[index + 9] = y2;

                        quads[index + 10] = uv.right;
                        quads[index + 11] = uv.bottom;

                        quads[index + 12] = x1;
                        quads[index + 13] = y2;

                        quads[index + 14] = uv.left;
                        quads[index + 15] = uv.bottom;
                    }
                    else
                    {
                        //If we don't need to draw this tile simply set the quad to size 0 at 0, 0.
                        // This does result in the quad being drawn, but we are skipping all
                        // pixel-filling. This is better than fully skipping rendering as we
                        // don't need to manage a buffer of drawable tiles with insertions/deletions.
                        for (int k = 0; k < 16; ++k)
                            quads[index + k] = 0.0f;
                    }

                    index += 16;
                    //quads.Put(vertices);

                    ++pos;
                    x1 = x2;
                    x2 += cellW;
                }

                y1 = y2;
                y2 += cellH;
            }
        }

        public override void Draw()
        {
            base.Draw();

            if (!updated.IsEmpty())
            {
                UpdateVertices();

                //if (buffer == null)
                //    buffer = new Vertexbuffer(quads);
                //else
                //{
                //    if (fullUpdate)
                //    {
                //        buffer.updateVertices(quads);
                //        fullUpdate = false;
                //    }
                //    else
                //    {
                //        buffer.updateVertices(quads,
                //                topLeftUpdating * 16,
                //                bottomRightUpdating * 16);
                //    }
                //}

                topLeftUpdating = -1;
                updating.SetEmpty();
            }

            //FIXME temporarily disabled this optimization as it is suspected to cause crashes
            /*Camera c = Camera.main;
            //we treat the position of the tilemap as (0,0) here
            camX = (int)(c.scroll.x/cellW - x/cellW);
            camY = (int)(c.scroll.y/cellH - y/cellH);
            camW = (int)Math.ceil(c.width/cellW);
            camH = (int)Math.ceil(c.height/cellH);

            if (camX >= mapWidth
                    || camY >= mapHeight
                    || camW + camW <= 0
                    || camH + camH <= 0)
                return;

            //determines the top-left visible tile, the bottom-right one, and the buffer length
            //between them, this culls a good number of none-visible tiles while keeping to 1 draw
            topLeft = Math.max(camX, 0)
                    + Math.max(camY*mapWidth, 0);

            bottomRight = Math.min(camX+camW, mapWidth-1)
                    + Math.min((camY+camH)*mapWidth, (mapHeight-1)*mapWidth);

            if (topLeft >= size || bottomRight < 0)
                length = 0;
            else
                length = bottomRight - topLeft + 1;

            if (length <= 0)
                return;*/

            NoosaScript script = Script();

            texture.Bind();

            script.uModel.ValueM4(matrix);
            script.Lighting(
                rm, gm, bm, am,
                ra, ga, ba, aa);

            script.Camera(camera);
            script.DrawQuadSet(quads, size);
        }

        protected virtual NoosaScript Script()
        {
            return NoosaScriptNoLighting.Get();
        }

        public override void Destroy()
        {
            base.Destroy();
            //if (buffer != null)
            //    buffer.delete();
        }

        protected virtual bool NeedsRender(int pos)
        {
            return data[pos] >= 0;
        }
    }
}