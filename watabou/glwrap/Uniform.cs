using OpenTK.Graphics.OpenGL4;

namespace watabou.glwrap
{
    public class Uniform
    {
        private int location;

        public Uniform(int location)
        {
            this.location = location;
        }

        public int Location()
        {
            return location;
        }

        public void Enable()
        {
            //Gdx.gl.glEnableVertexAttribArray(location);
            GL.EnableVertexAttribArray(location);
        }

        public void Disable()
        {
            // Gdx.gl.glDisableVertexAttribArray(location);
            GL.DisableVertexAttribArray(location);
        }

        //public void Value1f(float value)
        //{
        //    // Gdx.gl.glUniform1f(location, value);
        //    GL.Uniform1(location, value);
        //}

        //public void Value2f(float v1, float v2)
        //{
        //    //Gdx.gl.glUniform2f(location, v1, v2);
        //    GL.Uniform2(location, v1, v2);
        //}

        public void Value4f(float v1, float v2, float v3, float v4)
        {
            //Gdx.gl.glUniform4f(location, v1, v2, v3, v4);
            GL.Uniform4(location, v1, v2, v3, v4);
        }

        //public void ValueM3(float[] value)
        //{
        //    //Gdx.gl.glUniformMatrix3fv(location, 1, false, value, 0);
        //    GL.UniformMatrix3(location, 1, false, value);
        //}

        public void ValueM4(float[] value)
        {
            //Gdx.gl.glUniformMatrix4fv(location, 1, false, value, 0);
            GL.UniformMatrix4(location, 1, false, value);
        }
    }
}