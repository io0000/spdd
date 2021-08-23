using System;
using System.Collections.Generic;
using watabou.glwrap;
using watabou.utils;

namespace watabou.glscripts
{
    public class Script : Program
    {
        private static List<Script> all = new List<Script>();

        private static Script curScript;
        private static Type curScriptClass;

        public static T Use<T>() where T : Script
        {
            Type c = typeof(T);

            if (c == curScriptClass)
                return (T)curScript;

            // Script script = all.get( c );
            var script = all.Find(sc => sc.GetType() == c);
            if (script == null)
            {
                script = (T)Reflection.NewInstance(typeof(T));
                all.Add(script);
            }

            if (curScript != null)
                curScript.Unuse();

            curScript = script;
            curScriptClass = c;
            curScript.Use();

            return (T)curScript;
        }

        public static void Reset()
        {
            foreach (var script in all)
                script.Delete();

            all.Clear();

            curScript = null;
            curScriptClass = null;
        }

        public void Compile(string srcVert, string srcFrag)
        {
            //string[] srcShaders = src.split("//\n");

            //Attach(Shader.CreateCompiled(Shader.VERTEX, srcVert));
            //Attach(Shader.CreateCompiled(Shader.FRAGMENT, srcFrag));
            //Link();

            Shader vert = Shader.CreateCompiled(Shader.VERTEX, srcVert);
            Shader frag = Shader.CreateCompiled(Shader.FRAGMENT, srcFrag);

            Attach(vert);
            Attach(frag);

            Link();

            Detach(vert);
            Detach(frag);

            vert.Delete();
            frag.Delete();
        }

        public void Unuse()
        { }
    }
}