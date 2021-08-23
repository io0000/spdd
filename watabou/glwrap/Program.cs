using System;
using OpenTK.Graphics.OpenGL4;

namespace watabou.glwrap
{
    public class Program
    {
        private readonly int handle;

        public Program()
        {
            handle = GL.CreateProgram();
        }

        public int Handle()
        {
            return handle;
        }

        public void Attach(Shader shader)
        {
            GL.AttachShader(handle, shader.Handle());
        }

        public void Detach(Shader shader)
        {
            GL.DetachShader(handle, shader.Handle());
        }

        public void Link()
        {
            GL.LinkProgram(handle);

            GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out var status);
            if (status == (int)All.False)
            {
                throw new Exception(GL.GetProgramInfoLog(handle));
            }
        }

        public Attribute Attribute(string name)
        {
            return new Attribute(GL.GetAttribLocation(handle, name));
        }

        public Uniform Uniform(string name)
        {
            return new Uniform(GL.GetUniformLocation(handle, name));
        }

        public virtual void Use()
        {
            GL.UseProgram(handle);
        }

        public void Delete()
        {
            GL.DeleteProgram(handle);
        }

        //public static Program create(Shader...shaders )
        //{
        //    Program program = new Program();
        //    for (int i = 0; i < shaders.length; ++i)
        //    {
        //        program.attach(shaders[i]);
        //    }
        //    program.link();
        //    return program;
        //}
    }
}