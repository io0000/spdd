using watabou.glscripts;

namespace watabou.noosa
{
    public class NoosaScriptNoLighting : NoosaScript
    {
        public override void Lighting(float rm, float gm, float bm, float am, float ra, float ga, float ba, float aa)
        {
            //Does nothing
        }

        // warning CS0108: 'NoosaScriptNoLighting.Get()'은(는) 상속된 'NoosaScript.Get()' 멤버를 숨깁니다.숨기려면 new 키워드를 사용하세요.
        //public static NoosaScriptNoLighting Get()
        public new static NoosaScriptNoLighting Get()
        {
            return Script.Use<NoosaScriptNoLighting>();
        }

        protected override string ShaderVert()
        {
            return SHADER_VERT;
        }

        protected override string ShaderFrag()
        {
            return SHADER_FRAG;
        }

        private const string SHADER_VERT =
            "uniform mat4 uCamera;" +
            "uniform mat4 uModel;" +
            "attribute vec4 aXYZW;" +
            "attribute vec2 aUV;" +
            "varying vec2 vUV;" +
            "void main() {" +
            "  gl_Position = uCamera * uModel * aXYZW;" +
            "  vUV = aUV;" +
            "}";

        private const string SHADER_FRAG =
            "#ifdef GL_ES\n" +
            "precision mediump float;\n" +
            "#endif\n" +
            "varying vec2 vUV;" +
            "uniform sampler2D uTex;" +
            "void main() {" +
            "  gl_FragColor = texture2D( uTex, vUV );" +
            "}";
    }
}