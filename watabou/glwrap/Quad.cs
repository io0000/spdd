namespace watabou.glwrap
{
    public class Quad
    {
        // 0---1
        // | \ |
        // 3---2
        public static readonly short[] VALUES = { 0, 1, 2, 0, 2, 3 };

        public static readonly int SIZE = VALUES.Length;

        //	private static ShortBuffer indices;
        private static short[] indices;
        private static int indexSize;
        //private static int bufferIndex = -1;

        //public static FloatBuffer create()
        //{
        //    return ByteBuffer.
        //        allocateDirect(16 * Float.SIZE / 8).
        //        order(ByteOrder.nativeOrder()).
        //        asFloatBuffer();
        //}
        //
        //public static FloatBuffer createSet(int size)
        //{
        //    return ByteBuffer.
        //        allocateDirect(size * 16 * Float.SIZE / 8).
        //        order(ByteOrder.nativeOrder()).
        //        asFloatBuffer();
        //}
        //
        ////sets up for drawing up to 32k quads in one command, shouldn't ever need to exceed this
        //public static void setupIndices()
        //{
        //    ShortBuffer indices = getIndices(Short.MAX_VALUE);
        //    if (bufferIndex == -1)
        //    {
        //        bufferIndex = Gdx.gl.glGenBuffer();
        //    }
        //    Gdx.gl.glBindBuffer(Gdx.gl.GL_ELEMENT_ARRAY_BUFFER, bufferIndex);
        //    Gdx.gl.glBufferData(Gdx.gl.GL_ELEMENT_ARRAY_BUFFER, (indices.capacity() * 2), indices, Gdx.gl.GL_STATIC_DRAW);
        //    Gdx.gl.glBindBuffer(Gdx.gl.GL_ELEMENT_ARRAY_BUFFER, 0);
        //}
        //
        //public static void bindIndices()
        //{
        //    Gdx.gl.glBindBuffer(Gdx.gl.GL_ELEMENT_ARRAY_BUFFER, bufferIndex);
        //}
        //
        //public static void releaseIndices()
        //{
        //    Gdx.gl.glBindBuffer(Gdx.gl.GL_ELEMENT_ARRAY_BUFFER, 0);
        //}

        public static short[] GetIndices(int size)
        {
            if (size > indexSize)
            {
                indexSize = size;
                //indices = ByteBuffer.
                //    allocateDirect(size * SIZE * Short.SIZE / 8).
                //    order(ByteOrder.nativeOrder()).
                //    asShortBuffer();

                indices = new short[size * 6];
                int pos = 0;
                int limit = size * 4;

                for (int ofs = 0; ofs < limit; ofs += 4)
                {
                    // 0, 1, 2
                    indices[pos++] = (short)(ofs + 0);
                    indices[pos++] = (short)(ofs + 1);
                    indices[pos++] = (short)(ofs + 2);

                    // 0, 2, 3
                    indices[pos++] = (short)(ofs + 0);
                    indices[pos++] = (short)(ofs + 2);
                    indices[pos++] = (short)(ofs + 3);
                }

                //indices.put(values);
                //indices.position(0);
            }

            return indices;
        }

        public static void Fill(float[] v, int index,
            float x1, float x2, float y1, float y2,
            float u1, float u2, float v1, float v2)
        {
            v[index + 0] = x1;
            v[index + 1] = y1;
            v[index + 2] = u1;
            v[index + 3] = v1;

            v[index + 4] = x2;
            v[index + 5] = y1;
            v[index + 6] = u2;
            v[index + 7] = v1;

            v[index + 8] = x2;
            v[index + 9] = y2;
            v[index + 10] = u2;
            v[index + 11] = v2;

            v[index + 12] = x1;
            v[index + 13] = y2;
            v[index + 14] = u1;
            v[index + 15] = v2;
        }

        //public static void FillXY(float[] v, int index, float x1, float x2, float y1, float y2)
        //{
        //    v[index + 0] = x1;
        //    v[index + 1] = y1;
        //
        //    v[index + 4] = x2;
        //    v[index + 5] = y1;
        //
        //    v[index + 8] = x2;
        //    v[index + 9] = y2;
        //
        //    v[index + 12] = x1;
        //    v[index + 13] = y2;
        //}

        //public static void FillUV(float[] v, int index, float u1, float u2, float v1, float v2)
        //{
        //    v[index + 2] = u1;
        //    v[index + 3] = v1;
        //
        //    v[index + 6] = u2;
        //    v[index + 7] = v1;
        //
        //    v[index + 10] = u2;
        //    v[index + 11] = v2;
        //
        //    v[index + 14] = u1;
        //    v[index + 15] = v2;
        //}
    }
}