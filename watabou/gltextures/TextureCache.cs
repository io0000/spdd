using System;
using System.Collections.Generic;
using watabou.glwrap;
using watabou.noosa;
using watabou.utils;
using Texture = watabou.glwrap.Texture;

namespace watabou.gltextures
{
    public class TextureCache
    {
        private static readonly Dictionary<object, Texture> all = new Dictionary<object, Texture>();

        public static Texture CreateSolid(Color color)
        {
            string key = "1x1:" + color.i32value;

            Texture tx;
            if (all.TryGetValue(key, out tx))
            {
                return tx;
            }
            else
            {
                //Pixmap pixmap =new Pixmap( 1, 1, Pixmap.Format.RGBA8888 );
                // In the rest of the code ARGB is used
                //pixmap.setColor((color << 8) | (color >>> 24));
                //pixmap.fill();
                Pixmap pixmap = new Pixmap(1, 1);
                pixmap.SetPixel(0, 0, color);

                tx = new Texture(pixmap);
                all.Add(key, tx);

                return tx;
            }
        }

        public static Texture CreateGradient(Color[] colors)
        {
            var key = "";
            foreach (var color in colors)
                key += color.i32value;

            Texture tx;
            if (all.TryGetValue(key, out tx))
            {
                return tx;
            }
            else
            {
                //Pixmap pixmap = new Pixmap( colors.length, 1, Pixmap.Format.RGBA8888);
                //for (int i = 0; i < colors.length; ++i)
                //{
                //    // In the rest of the code ARGB is used
                //    pixmap.drawPixel(i, 0, (colors[i] << 8) | (colors[i] >>> 24));
                //}
                Pixmap pixmap = new Pixmap(colors.Length, 1);
                for (int i = 0; i < colors.Length; ++i)
                    pixmap.SetPixel(i, 0, colors[i]);

                tx = new Texture(pixmap);

                tx.Filter(Texture.LINEAR, Texture.LINEAR);
                tx.Wrap(Texture.CLAMP, Texture.CLAMP);

                all.Add(key, tx);
                return tx;
            }
        }

        public static void Add(object key, Texture tx)
        {
            //if (all.ContainsKey(key))
            //    all[key] = tx;
            //else
            //    all.Add(key, tx);
            all[key] = tx;
        }

        public static void Remove(object key)
        {
            if (all.ContainsKey(key))
            {
                Texture tx = all[key];
                all.Remove(key);

                tx.Delete();
            }
        }

        public static Texture Get(object src)
        {
            Texture tx;
            if (all.TryGetValue(src, out tx))
                return tx;

            if (src is Texture)
                return (Texture)src;

            tx = new Texture(GetBitmap(src));
            all.Add(src, tx);
            return tx;
        }

        //public static void Clear()
        //{
        //    foreach (var txt in all.Values)
        //        txt.Delete();
        //
        //    all.Clear();
        //}

        public static void Reload()
        {
            foreach (var tx in all.Values)
                tx.Reload();
        }

        private static Pixmap GetBitmap(object src)
        {
            try
            {
                if (src is int)
                {
                    //LibGDX does not support android resource integer handles, and they were
                    //never used by the game anyway, should probably remove this entirely
                    return null;
                }

                if (src is string)
                {
                    //return new Pixmap(Gdx.files.internal((string)src));
                    return new Pixmap((string)src);
                }

                if (src is Pixmap)
                    return (Pixmap)src;

                return null;
            }
            catch (Exception e)
            {
                Game.ReportException(e);
                return null;
            }
        }

        public static bool Contains(object key)
        {
            return all.ContainsKey(key);
        }
    }
}