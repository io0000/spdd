using System;
using System.Collections.Generic;
using watabou.glwrap;
using watabou.noosa;

namespace watabou.utils
{
    public class BitmapCache
    {
        private const string DEFAULT = "__default";

        private static Dictionary<string, Layer> layers = new Dictionary<string, Layer>();

        public static Pixmap Get(string assetName)
        {
            return Get(DEFAULT, assetName);
        }

        public static Pixmap Get(string layerName, string assetName)
        {
            Layer layer;
            if (!layers.ContainsKey(layerName))
            {
                layer = new Layer();
                layers.Add(layerName, layer);
            }
            else
            {
                layer = layers[layerName];
            }

            if (layer.ContainsKey(assetName))
            {
                return layer[assetName];
            }
            else
            {
                try
                {
                    //Pixmap bmp = new Pixmap( Gdx.files.internal(assetName) );
                    Pixmap bmp = new Pixmap(assetName);
                    layer.Add(assetName, bmp);
                    return bmp;
                }
                catch (Exception e)
                {
                    Game.ReportException(e);
                    return null;
                }
            }
        }

        //Unused, LibGDX does not support resource Ids
        /*
        public static Pixmap get( int resID ) {
            return get( DEFAULT, resID );
        }
    
        public static Pixmap get( string layerName, int resID ) {
        
            Layer layer;
            if (!layers.containsKey( layerName )) {
                layer = new Layer();
                layers.put( layerName, layer );
            } else {
                layer = layers.get( layerName );
            }
        
            if (layer.containsKey( resID )) {
                return layer.get( resID );
            } else {
            
                Bitmap bmp = BitmapFactory.decodeResource( context.getResources(), resID );
                layer.put( resID, bmp );
                return bmp;
            
            }
        }*/

        public static void Clear(string layerName)
        {
            if (layers.ContainsKey(layerName))
            {
                layers[layerName].Clear();
                layers.Remove(layerName);
            }
        }

        public static void Clear()
        {
            foreach (Layer layer in layers.Values)
            {
                layer.Clear();
            }
            layers.Clear();
        }

        private class Layer : Dictionary<object, Pixmap>
        {
            //public void Clear() {
            //	foreach (Pixmap bmp in Values) {
            //		bmp.Dispose();
            //	}
            //	base.Clear();
            //}
        }
    }
}