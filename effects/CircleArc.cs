using System;
using watabou.gltextures;
using watabou.noosa;
using watabou.utils;
using watabou.glwrap;

namespace spdd.effects
{
    public class CircleArc : Visual
    {
        private float duration;
        private float lifespan;

        //1f is an entire 360 degree sweep
        private float sweep;
        private bool dirty;

        private bool lightMode = true;

        private Texture texture;

        //private FloatBuffer vertices;
        //private ShortBuffer indices;
        private float[] vertices;
        private short[] indices;

        private int nTris;
        private float rad;

        public CircleArc(int triangles, float radius)
            : base(0, 0, 0, 0)
        {
            texture = TextureCache.CreateSolid(new Color(0xFF, 0xFF, 0xFF, 0xFF));

            this.nTris = triangles;
            this.rad = radius;

            //vertices = ByteBuffer.
            //    allocateDirect((nTris * 2 + 1) * 4 * (Float.SIZE / 8)).
            //    order(ByteOrder.nativeOrder()).
            //    asFloatBuffer();
            vertices = new float[(nTris * 2 + 1) * 4];

            //indices = ByteBuffer.
            //        allocateDirect(nTris * 3 * Short.SIZE / 8).
            //        order(ByteOrder.nativeOrder()).
            //        asShortBuffer();
            indices = new short[nTris * 3];

            sweep = 1f;
            UpdateTriangles();
        }

        public CircleArc Color(Color color, bool lightMode)
        {
            this.lightMode = lightMode;
            Hardlight(color);

            return this;
        }

        public CircleArc Show(Visual visual, float duration)
        {
            Point(visual.Center());
            visual.parent.AddToBack(this);

            lifespan = this.duration = duration;

            return this;
        }

        public CircleArc Show(Group parent, PointF pos, float duration)
        {
            Point(pos);
            parent.Add(this);

            lifespan = this.duration = duration;

            return this;
        }

        public void SetSweep(float sweep)
        {
            this.sweep = sweep;
            dirty = true;
        }

        private void UpdateTriangles()
        {
            dirty = false;

            int vi = 0;
            int ii = 0;

            vertices[vi + 0] = 0;
            vertices[vi + 1] = 0;
            vertices[vi + 2] = 0.25f;
            vertices[vi + 3] = 0;
            vi += 4;

            //starting position is very top by default, use angle to adjust this.
            double start = 2 * (Math.PI - Math.PI * sweep) - Math.PI / 2.0;

            for (int i = 0; i < nTris; ++i)
            {
                double a = start + i * Math.PI * 2 / nTris * sweep;
                vertices[vi + 0] = (float)Math.Cos(a) * rad;
                vertices[vi + 1] = (float)Math.Sin(a) * rad;
                vertices[vi + 2] = 0.75f;
                vertices[vi + 3] = 0.0f;
                vi += 4;

                a += 3.1415926f * 2 / nTris * sweep;
                vertices[vi + 0] = (float)Math.Cos(a) * rad;
                vertices[vi + 1] = (float)Math.Sin(a) * rad;
                vertices[vi + 2] = 0.75f;
                vertices[vi + 3] = 0.0f;
                vi += 4;

                indices[ii + 0] = 0;
                indices[ii + 1] = (short)(1 + i * 2);
                indices[ii + 2] = (short)(2 + i * 2);
                ii += 3;
            }
        }


        public override void Update()
        {
            base.Update();

            if (duration > 0)
            {
                if ((lifespan -= Game.elapsed) > 0)
                {
                    sweep = lifespan / duration;
                    dirty = true;
                }
                else
                {
                    KillAndErase();
                }
            }
        }

        public override void Draw()
        {
            base.Draw();

            if (dirty)
                UpdateTriangles();

            if (lightMode)
                Blending.SetLightMode();

            var script = NoosaScript.Get();

            texture.Bind();

            script.uModel.ValueM4(matrix);
            script.Lighting(
                rm, gm, bm, am,
                ra, ga, ba, aa);

            script.Camera(camera);
            script.DrawElements(vertices, indices, nTris * 3);

            if (lightMode)
                Blending.SetNormalMode();
        }
    }
}