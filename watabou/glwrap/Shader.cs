using System;
using OpenTK.Graphics.OpenGL4;

namespace watabou.glwrap
{
    public class Shader
    {
        //public static final int VERTEX = Gdx.gl.GL_VERTEX_SHADER;
        //public static final int FRAGMENT = Gdx.gl.GL_FRAGMENT_SHADER;
        public const int VERTEX = (int)ShaderType.VertexShader;
        public const int FRAGMENT = (int)ShaderType.FragmentShader;

        private int handle;

        public Shader(int type)
        {
            //handle = Gdx.gl.glCreateShader( type );
            handle = GL.CreateShader((ShaderType)type);
        }

        public int Handle()
        {
            return handle;
        }

        public void Source(string src)
        {
            GL.ShaderSource(handle, src);
        }

        public void Compile()
        {
            //Gdx.gl.glCompileShader(handle);
            //
            //IntBuffer status = BufferUtils.newIntBuffer(1);
            //Gdx.gl.glGetShaderiv(handle, Gdx.gl.GL_COMPILE_STATUS, status);
            //if (status.get() == Gdx.gl.GL_FALSE)
            //{
            //    throw new Error(Gdx.gl.glGetShaderInfoLog(handle));
            //}

            GL.CompileShader(handle);

            GL.GetShader(handle, ShaderParameter.CompileStatus, out var code);
            if (code == (int)All.False)
            {
                throw new Exception(GL.GetShaderInfoLog(handle));
            }
        }

        public void Delete()
        {
            // Gdx.gl.glDeleteShader( handle );
            GL.DeleteShader(handle);
        }

        public static Shader CreateCompiled(int type, string src)
        {
            Shader shader = new Shader(type);
            shader.Source(src);
            shader.Compile();
            return shader;
        }
    }
}