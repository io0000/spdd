using System;
using Microsoft.Collections.Extensions;
using watabou.gltextures;
using watabou.glwrap;
using watabou.noosa;
using watabou.utils;
using spdd.items.keys;
using spdd.journal;

namespace spdd.ui
{
    public class KeyDisplay : Visual
    {
        //private float[] vertices = new float[16];
        //private FloatBuffer quads;
        //private Vertexbuffer buffer;
        private float[] vertices;

        private Texture tx = TextureCache.Get(Assets.Interfaces.MENU);

        private bool dirty = true;
        private int[] keys;

        //mapping of key types to slots in the array, 0 is reserved for black (missed) keys
        //this also determines the order these keys will appear (lower first)
        //and the order they will be truncated if there is no space (higher first, larger counts first)
        private static OrderedDictionary<Type, int> keyMap = new OrderedDictionary<Type, int>();

        static KeyDisplay()
        {
            keyMap.Add(typeof(SkeletonKey), 1);
            keyMap.Add(typeof(CrystalKey), 2);
            keyMap.Add(typeof(GoldenKey), 3);
            keyMap.Add(typeof(IronKey), 4);
        }

        private int totalKeys;

        public KeyDisplay()
            : base(0, 0, 0, 0)
        { }

        public void UpdateKeys()
        {
            keys = new int[keyMap.Count + 1];

            //foreach (var rec in Notes.GetRecords<Notes.KeyRecord>())
            foreach (var rec in Notes.GetKeyRecords())
            {
                if (rec.Depth() < Dungeon.depth)
                {
                    //only ever 1 black key
                    keys[0] = 1;
                }
                else if (rec.Depth() == Dungeon.depth)
                {
                    keys[keyMap[rec.Type()]] += rec.Quantity();
                }
            }

            totalKeys = 0;
            foreach (int k in keys)
            {
                totalKeys += k;
            }
            dirty = true;
        }

        public int KeyCount()
        {
            return totalKeys;
        }

        public override void Draw()
        {
            base.Draw();

            if (dirty)
            {
                UpdateVertices();
            }

            NoosaScript script = NoosaScript.Get();

            tx.Bind();

            script.Camera(GetCamera());

            script.uModel.ValueM4(matrix);
            script.Lighting(
                    rm, gm, bm, am,
                    ra, ga, ba, aa);
            script.DrawQuadSet(vertices, totalKeys);
        }

        private void UpdateVertices()
        {
            //assumes shorter key sprite
            int maxRows = (int)(height + 1) / 5;

            //1 pixel of padding between each key
            int maxPerRow = (int)(width + 1) / 4;

            int maxKeys = maxPerRow * maxRows;

            while (totalKeys > maxKeys)
            {
                Type mostType = null;
                int mostNum = 0;
                foreach (var pair in keyMap)
                {
                    Type k = pair.Key;
                    var value = pair.Value;
                    if (keys[value] >= mostNum)
                    {
                        mostType = k;
                        mostNum = keys[value];
                    }
                }

                --keys[keyMap[mostType]];
                --totalKeys;
            }

            int rows = (int)Math.Ceiling(totalKeys / (float)maxPerRow);

            bool shortKeys = (rows * 8) > height;
            float left;
            if (totalKeys > maxPerRow)
            {
                left = 0;
            }
            else
            {
                left = (width + 1 - (totalKeys * 4)) / 2;
            }

            float top = (height + 1 - (rows * (shortKeys ? 5 : 8))) / 2;
            //quads = Quad.createSet(totalKeys);
            vertices = new float[totalKeys * 16];
            int index = 0;

            for (int i = 0; i < totalKeys; ++i)
            {
                int keyIdx = 0;

                if (i == 0 && keys[0] > 0)
                {
                    //black key
                    keyIdx = 0;
                }
                else
                {
                    for (int j = 1; j < keys.Length; ++j)
                    {
                        if (keys[j] > 0)
                        {
                            --keys[j];
                            keyIdx = j;
                            break;
                        }
                    }
                }

                //texture coordinates
                RectF r = tx.UvRect(43 + 3 * keyIdx, shortKeys ? 8 : 0,
                        46 + 3 * keyIdx, shortKeys ? 12 : 7);

                vertices[index + 2] = r.left;
                vertices[index + 3] = r.top;

                vertices[index + 6] = r.right;
                vertices[index + 7] = r.top;

                vertices[index + 10] = r.right;
                vertices[index + 11] = r.bottom;

                vertices[index + 14] = r.left;
                vertices[index + 15] = r.bottom;

                //screen coordinates
                vertices[index + 0] = left;
                vertices[index + 1] = top;

                vertices[index + 4] = left + 3;
                vertices[index + 5] = top;

                vertices[index + 8] = left + 3;
                vertices[index + 9] = top + (shortKeys ? 4 : 7);

                vertices[index + 12] = left;
                vertices[index + 13] = top + (shortKeys ? 4 : 7);

                index += 16;
                //quads.put(vertices);

                //move to the right for more keys, drop down if the row is done
                left += 4;
                if (left + 3 > width)
                {
                    left = 0;
                    top += (shortKeys ? 5 : 8);
                }
            }

            dirty = false;
        }
    }
}