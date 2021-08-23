using System.Collections.Generic;
using watabou.gltextures;
using watabou.glwrap;
using watabou.utils;

namespace watabou.noosa
{
    public class TextureFilm
    {
        private static RectF FULL = new RectF(0.0f, 0.0f, 1.0f, 1.0f);

        private int texWidth;
        private int texHeight;

        private Texture texture;

        protected Dictionary<object, RectF> frames = new Dictionary<object, RectF>();

        public TextureFilm(object tx)
        {
            texture = TextureCache.Get(tx);

            texWidth = texture.width;
            texHeight = texture.height;

            // c#은 null을 키값으로 사용할 수 없음 -> 예외처리
            //Add(null, FULL);
        }

        public TextureFilm(Texture texture, int Width)
            : this(texture, Width, texture.height)
        { }

        public TextureFilm(object tx, int width, int height)
        {
            texture = TextureCache.Get(tx);

            texWidth = texture.width;
            texHeight = texture.height;

            float uw = (float)width / texWidth;
            float vh = (float)height / texHeight;
            int cols = texWidth / width;
            int rows = texHeight / height;

            for (var i = 0; i < rows; ++i)
            {
                for (var j = 0; j < cols; ++j)
                {
                    var rect = new RectF(j * uw, i * vh, (j + 1) * uw, (i + 1) * vh);
                    Add(i * cols + j, rect);
                }
            }
        }

        public TextureFilm(TextureFilm atlas, object key, int width, int height)
        {
            texture = atlas.texture;

            texWidth = atlas.texWidth;
            texHeight = atlas.texHeight;

            RectF patch = atlas.Get(key);

            float uw = (float)width / texWidth;
            float vh = (float)height / texHeight;
            int cols = (int)Width(patch) / width;
            int rows = (int)Height(patch) / height;

            for (var i = 0; i < rows; ++i)
            {
                for (var j = 0; j < cols; ++j)
                {
                    var rect = new RectF(j * uw, i * vh, (j + 1) * uw, (i + 1) * vh);
                    rect.Shift(patch.left, patch.top);
                    Add(i * cols + j, rect);
                }
            }
        }

        public void Add(object id, RectF rect)
        {
            frames[id] = rect;
        }

        public void Add(object id, int left, int top, int right, int bottom)
        {
            frames[id] = texture.UvRect(left, top, right, bottom);
        }

        public RectF Get(object id)
        {
            // c#은 null을 키값으로 사용할 수 없음 -> 예외처리
            if (id == null)
                return FULL;

            RectF rect;
            if (frames.TryGetValue(id, out rect))
                return rect;
            else
                return null;
        }

        public float Width(object id)
        {
            return Width(Get(id));
        }

        public float Width(RectF frame)
        {
            return frame.Width() * texWidth;
        }

        public float Height(object id)
        {
            return Height(Get(id));
        }

        public float Height(RectF frame)
        {
            return frame.Height() * texHeight;
        }
    }
}