using System;
using watabou.gltextures;
using watabou.noosa;
using watabou.utils;
using watabou.glwrap;

namespace spdd.effects
{
    public class Flare : Visual
    {
        private float duration;
        private float lifespan;

        private bool lightMode = true;

        private Texture texture;

        private float[] vertices;
        private short[] indices;

        private int nRays;

        public Flare(int nRays, float radius)
            : base(0, 0, 0, 0)
        {
            Color[] gradient = new Color[2];
            gradient[0] = new Color(0xFF, 0xFF, 0xFF, 0xFF);
            gradient[1] = new Color(0xFF, 0xFF, 0xFF, 0x00);

            texture = TextureCache.CreateGradient(gradient);

            this.nRays = nRays;

            angle = 45;
            angularSpeed = 180;

            //vertices = ByteBuffer.
            //	allocateDirect( (nRays * 2 + 1) * 4 * (Float.SIZE / 8) ).
            //	order( ByteOrder.nativeOrder() ).
            //	asFloatBuffer();
            vertices = new float[(nRays * 2 + 1) * 4];
            int vi = 0;

            //indices = ByteBuffer.
            //	allocateDirect( nRays * 3 * Short.SIZE / 8 ).
            //	order( ByteOrder.nativeOrder() ).
            //	asShortBuffer();
            indices = new short[nRays * 3];
            int ii = 0;

            vertices[vi + 0] = 0;
            vertices[vi + 1] = 0;
            vertices[vi + 2] = 0.25f;
            vertices[vi + 3] = 0;
            vi += 4;

            //v[2] = 0.75f;
            //v[3] = 0;

            for (var i = 0; i < nRays; ++i)
            {
                float a = i * 3.1415926f * 2 / nRays;
                vertices[vi + 0] = (float)Math.Cos(a) * radius;
                vertices[vi + 1] = (float)Math.Sin(a) * radius;
                vertices[vi + 2] = 0.75f;
                vertices[vi + 3] = 0;
                vi += 4;

                a += 3.1415926f * 2 / nRays / 2;
                vertices[vi + 0] = (float)Math.Cos(a) * radius;
                vertices[vi + 1] = (float)Math.Sin(a) * radius;
                vertices[vi + 2] = 0.75f;
                vertices[vi + 3] = 0;
                vi += 4;

                indices[ii + 0] = 0;
                indices[ii + 1] = (short)(1 + i * 2);
                indices[ii + 2] = (short)(2 + i * 2);
                ii += 3;
            }
        }

        public Flare Color(Color color, bool lightMode)
        {
            this.lightMode = lightMode;
            Hardlight(color);

            return this;
        }

        public Flare Show(Visual visual, float duration)
        {
            Point(visual.Center());
            visual.parent.AddToBack(this);

            lifespan = this.duration = duration;
            if (lifespan > 0)
                scale.Set(0);

            return this;
        }

        public Flare Show(Group parent, PointF pos, float duration)
        {
            Point(pos);
            parent.Add(this);

            lifespan = this.duration = duration;
            if (lifespan > 0)
                scale.Set(0);

            return this;
        }

        public override void Update()
        {
            base.Update();

            if (duration <= 0)
                return;

            if ((lifespan -= Game.elapsed) > 0)
            {
                var p = 1 - lifespan / duration; // 0 -> 1
                p = p < 0.25f ? p * 4 : (1 - p) * 1.333f;
                scale.Set(p);
                Alpha(p);
            }
            else
            {
                KillAndErase();
            }
        }

        public override void Draw()
        {
            base.Draw();

            if (lightMode)
            {
                Blending.SetLightMode();
                DrawRays();
                Blending.SetNormalMode();
            }
            else
            {
                DrawRays();
            }
        }

        private void DrawRays()
        {
            var script = NoosaScript.Get();

            texture.Bind();

            script.uModel.ValueM4(matrix);
            script.Lighting(
                rm, gm, bm, am,
                ra, ga, ba, aa);

            script.Camera(camera);
            script.DrawElements(vertices, indices, nRays * 3);
        }
    }
}