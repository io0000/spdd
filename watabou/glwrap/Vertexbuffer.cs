using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace watabou.glwrap
{
    public class Vertexbuffer
    {
        private int id;
        private float[] vertices;
        private int updateStart, updateEnd;

        private static List<Vertexbuffer> buffers = new List<Vertexbuffer>();

        public Vertexbuffer(float[] vertices)
        {
            id = GL.GenBuffer();

            this.vertices = vertices;
            buffers.Add(this);

            updateStart = 0;
            updateEnd = vertices.Length;
        }

        //For flagging the buffer for a full update without changing anything
        public void UpdateVertices()
        {
            UpdateVertices(vertices);
        }

        //For flagging an update with a full set of new data
        public void UpdateVertices(float[] vertices)
        {
            UpdateVertices(vertices, 0, vertices.Length);
        }

        //For flagging an update with a subset of data changed
        public void UpdateVertices(float[] vertices, int start, int end)
        {
            this.vertices = vertices;

            if (updateStart == -1)
                updateStart = start;
            else
                updateStart = Math.Min(start, updateStart);

            if (updateEnd == -1)
                updateEnd = end;
            else
                updateEnd = Math.Max(end, updateEnd);
        }

        public void UpdateGLData()
        {
            if (updateStart == -1)
                return;

            //vertices.position(updateStart);
            Bind();

            if (updateStart == 0 && updateEnd == vertices.Length)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * 4, vertices, BufferUsageHint.StaticDraw);
            }
            else
            {
                GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(updateStart * 4), (updateEnd - updateStart) * 4, vertices);

                //unsafe
                //{
                //    fixed (float* p = &vertices[updateStart])
                //    {
                //        IntPtr ptr = new IntPtr(p);
                //    
                //        GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(updateStart * 4), (updateEnd - updateStart) * 4, ptr);
                //    }
                //}
            }

            Release();
            updateStart = updateEnd = -1;
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, id);
        }

        public void Release()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Delete()
        {
            GL.DeleteBuffer(id);
            buffers.Remove(this);
        }

        public static void RefreshAllBuffers()
        {
            foreach (var buf in buffers)
            {
                buf.UpdateVertices();
                buf.UpdateGLData();
            }
        }
    }
}