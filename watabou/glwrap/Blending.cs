using OpenTK.Graphics.OpenGL4;

namespace watabou.glwrap
{
    public class Blending
    {
        public static void UseDefault()
        {
            Enable();
            SetNormalMode();
        }

        public static void Enable()
        {
            //Gdx.gl.glEnable(Gdx.gl.GL_BLEND);
            GL.Enable(EnableCap.Blend);
        }

        public static void Disable()
        {
            //Gdx.gl.glDisable(Gdx.gl.GL_BLEND);
            GL.Disable(EnableCap.Blend);
        }

        //in this mode colors overwrite eachother, based on alpha value
        public static void SetNormalMode()
        {
            //Gdx.gl.glBlendFunc(Gdx.gl.GL_SRC_ALPHA, Gdx.gl.GL_ONE_MINUS_SRC_ALPHA);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        //in this mode colors add to eachother, eventually reaching pure white
        public static void SetLightMode()
        {
            //Gdx.gl.glBlendFunc(Gdx.gl.GL_SRC_ALPHA, Gdx.gl.GL_ONE);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
        }
    }
}