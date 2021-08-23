using OpenTK.Graphics.OpenGL4;
using watabou.glwrap;
using watabou.glscripts;

using Attribute = watabou.glwrap.Attribute;

namespace watabou.noosa
{
    public class NoosaScript : Script
    {
        public Uniform uCamera;
        public Uniform uModel;
        public Uniform uTex;
        public Uniform uColorM;
        public Uniform uColorA;
        public Attribute aXY;
        public Attribute aUV;

        private Camera lastCamera;

        public NoosaScript()
        {
            Compile(ShaderVert(), ShaderFrag());

            uCamera = Uniform("uCamera");
            uModel = Uniform("uModel");
            uTex = Uniform("uTex");
            uColorM = Uniform("uColorM");
            uColorA = Uniform("uColorA");
            aXY = Attribute("aXYZW");
            aUV = Attribute("aUV");

            //Quad.setupIndices();
            //Quad.bindIndices();
        }

        public override void Use()
        {
            base.Use();

            aXY.Enable();
            aUV.Enable();
        }

        public void DrawElements(float[] vertices, short[] indices, int size)
        {
            aXY.VertexPointer(2, 4, vertices, 0);

            aUV.VertexPointer(2, 4, vertices, 2);

            GL.DrawElements(PrimitiveType.Triangles, size, DrawElementsType.UnsignedShort, indices);
        }

        public void DrawQuad(float[] vertices)
        {
            aXY.VertexPointer(2, 4, vertices, 0);

            aUV.VertexPointer(2, 4, vertices, 2);

            short[] indices = { 0, 1, 2, 0, 2, 3 };

            GL.DrawElements(PrimitiveType.Triangles, Quad.SIZE, DrawElementsType.UnsignedShort, indices);
        }

        // size : quadÀÇ °¹¼ö
        public void DrawQuadSet(float[] vertices, int size)
        {
            if (size == 0)
                return;

            aXY.VertexPointer(2, 4, vertices, 0);

            aUV.VertexPointer(2, 4, vertices, 2);

            GL.DrawElements(PrimitiveType.Triangles, Quad.SIZE * size, DrawElementsType.UnsignedShort, Quad.GetIndices(size));
        }

        public virtual void Lighting(float rm, float gm, float bm, float am, float ra, float ga, float ba, float aa)
        {
            uColorM.Value4f(rm, gm, bm, am);
            uColorA.Value4f(ra, ga, ba, aa);
        }

        public void ResetCamera()
        {
            lastCamera = null;
        }

        public void Camera(Camera camera)
        {
            if (camera == null)
                camera = noosa.Camera.main;

            if (camera != lastCamera && camera.matrix != null)
            {
                lastCamera = camera;
                uCamera.ValueM4(camera.matrix);

                if (!camera.fullScreen)
                {
                    GL.Enable(EnableCap.ScissorTest);

                    /*
                    //This fixes pixel scaling issues on some hidpi displays (mainly on macOS)
                    // because for some reason all other openGL operations work on virtual pixels
                    // but glScissor operations work on real pixels
                    float xScale = (Gdx.graphics.getBackBufferWidth() / (float)Game.width);
                    float yScale = (Gdx.graphics.getBackBufferHeight() / (float)Game.height);

                    Gdx.gl20.glScissor(
                            Math.round(camera.x * xScale),
                            Math.round((Game.height - camera.screenHeight - camera.y) * yScale),
                            Math.round(camera.screenWidth * xScale),
                            Math.round(camera.screenHeight * yScale));
                    */
                    GL.Scissor(
                        camera.x,
                        Game.height - (int)camera.screenHeight - camera.y,
                        camera.screenWidth,
                        camera.screenHeight);
                }
                else
                {
                    GL.Disable(EnableCap.ScissorTest);
                }
            }
        }

        public static NoosaScript Get()
        {
            return Script.Use<NoosaScript>();
        }

        protected virtual string ShaderVert()
        {
            return SHADER_VERT;
        }

        protected virtual string ShaderFrag()
        {
            return SHADER_FRAG;
        }

        private const string SHADER_VERT =
            //"uniform mat4 uCamera;" +
            //"uniform mat4 uModel;" +
            //"attribute vec4 aXYZW;" +
            //"attribute vec2 aUV;" +
            //"varying vec2 vUV;" +
            //"void main() {" +
            //"  gl_Position = uCamera * uModel * aXYZW;" +
            //"  vUV = aUV;" +
            //"}";
            "uniform mat4 uCamera;\n" +
            "uniform mat4 uModel;\n" +
            "attribute vec4 aXYZW;\n" +
            "attribute vec2 aUV;\n" +
            "varying vec2 vUV;\n" +
            "void main() {\n" +
            "  gl_Position = uCamera * uModel * aXYZW;\n" +
            "  vUV = aUV;\n" +
            "}\n";

        // diff
        private const string SHADER_FRAG =
            //"#ifdef GL_ES\n" +
            //"precision mediump float;\n" +
            //"#endif\n" +
            //"varying vec2 vUV;\n" +
            //"uniform sampler2D uTex;\n" +
            //"uniform vec4 uColorM;\n" +
            //"uniform vec4 uColorA;\n" +
            //"void main() {\n" +
            //"  gl_FragColor = texture2D( uTex, vUV ) * uColorM + uColorA;\n" +
            //"}\n";
            "#ifdef GL_ES\n" +
            "  #define LOW lowp\n" +
            "  #define MED mediump\n" +
            "#else\n" +
            "  #define LOW\n" +
            "  #define MED\n" +
            "#endif\n" +
            "varying MED vec2 vUV;\n" +
            "uniform LOW sampler2D uTex;\n" +
            "uniform LOW vec4 uColorM;\n" +
            "uniform LOW vec4 uColorA;\n" +
            "void main() {\n" +
            "  gl_FragColor = texture2D( uTex, vUV ) * uColorM + uColorA;\n" +
            "}\n";
    }
}