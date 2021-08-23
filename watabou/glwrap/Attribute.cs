using System;
using OpenTK.Graphics.OpenGL4;

namespace watabou.glwrap
{
    public class Attribute
    {
        private readonly int location;

        public Attribute(int location)
        {
            this.location = location;
        }

        public int Location()
        {
            return location;
        }

        public void Enable()
        {
            GL.EnableVertexAttribArray(location);
        }

        public void Disable()
        {
            GL.DisableVertexAttribArray(location);
        }

        //public void vertexPointer(int size, int stride, FloatBuffer ptr)
        //{
        //    Gdx.gl.glVertexAttribPointer(location, size, Gdx.gl.GL_FLOAT, false, stride * 4, ptr);
        //}
        //
        //public void vertexBuffer(int size, int stride, int offset)
        //{
        //    Gdx.gl.glVertexAttribPointer(location, size, Gdx.gl.GL_FLOAT, false, stride * 4, offset * 4);
        //}

        public void VertexPointer(int size, int stride, float[] vertices, int offset)
        {
            //unsafe
            //{
            //    fixed (float* p = &vertices[offset])
            //    {
            //        IntPtr ptr = new IntPtr(p);
            //
            //        GL.VertexAttribPointer(location, 
            //            size, 
            //            VertexAttribPointerType.Float, 
            //            false, 
            //            stride * sizeof(float), 
            //            ptr);
            //    }
            //}

            unsafe
            {
                fixed (float* p = vertices)
                {
                    IntPtr ptr = new IntPtr(p);

                    GL.VertexAttribPointer(location,
                        size,
                        VertexAttribPointerType.Float,
                        false,
                        stride * sizeof(float),
                        IntPtr.Add(ptr, offset * sizeof(float)));
                }
            }
        }
    }
}