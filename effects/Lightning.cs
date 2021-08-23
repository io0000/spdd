using System;
using System.Collections.Generic;
using watabou.noosa;
using watabou.utils;
using watabou.glwrap;
using spdd.tiles;

namespace spdd.effects
{
    public class Lightning : Group
    {
        private const float DURATION = 0.3f;

        private float life;

        private List<Arc> arcs;

        private ICallback callback;

        public Lightning(int from, int to, ICallback callback)
            : this(new Arc(from, to), callback)
        { }

        public Lightning(PointF from, int to, ICallback callback)
            : this(new Arc(from, to), callback)
        { }

        public Lightning(int from, PointF to, ICallback callback)
            : this(new Arc(from, to), callback)
        { }

        public Lightning(PointF from, PointF to, ICallback callback)
            : this(new Arc(from, to), callback)
        { }

        // 필요에 의해서 추가
        public Lightning(Arc arc, ICallback callback)
            : this(new List<Arc>(new[] { arc }), callback)
        { }

        public Lightning(List<Arc> arcs, ICallback callback)
        {
            this.arcs = arcs;
            foreach (var arc in this.arcs)
                Add(arc);

            this.callback = callback;

            life = DURATION;
        }

        private const double A = 180 / Math.PI;

        public override void Update()
        {
            if ((life -= Game.elapsed) < 0)
            {
                KillAndErase();
                if (callback != null)
                    callback.Call();
            }
            else
            {
                var alpha = life / DURATION;

                foreach (var arc in arcs)
                {
                    arc.Alpha(alpha);
                }

                base.Update();
            }
        }

        public override void Draw()
        {
            Blending.SetLightMode();
            base.Draw();
            Blending.SetNormalMode();
        }

        //A lightning object is meant to be loaded up with arcs.
        //these act as a means of easily expressing lighting between two points.
        public class Arc : Group
        {
            private Image arc1, arc2;

            //starting and ending x/y values
            private PointF start, end;

            public Arc(int from, int to)
                : this(DungeonTilemap.TileCenterToWorld(from),
                        DungeonTilemap.TileCenterToWorld(to))
            { }

            public Arc(PointF from, int to)
                : this(from, DungeonTilemap.TileCenterToWorld(to))
            { }

            public Arc(int from, PointF to)
                : this(DungeonTilemap.TileCenterToWorld(from), to)
            { }

            public Arc(PointF from, PointF to)
            {
                start = from;
                end = to;

                arc1 = new Image(Effects.Get(Effects.Type.LIGHTNING));
                arc1.x = start.x - arc1.origin.x;
                arc1.y = start.y - arc1.origin.y;
                arc1.origin.Set(0, arc1.Height() / 2);
                Add(arc1);

                arc2 = new Image(Effects.Get(Effects.Type.LIGHTNING));
                arc2.origin.Set(0, arc2.Height() / 2);
                Add(arc2);

                Update();
            }

            public void Alpha(float alpha)
            {
                arc1.am = arc2.am = alpha;
            }

            public override void Update()
            {
                float x2 = (start.x + end.x) / 2 + Rnd.Float(-4, +4);
                float y2 = (start.y + end.y) / 2 + Rnd.Float(-4, +4);

                float dx = x2 - start.x;
                float dy = y2 - start.y;
                arc1.angle = (float)(Math.Atan2(dy, dx) * A);
                arc1.scale.x = (float)Math.Sqrt(dx * dx + dy * dy) / arc1.width;

                dx = end.x - x2;
                dy = end.y - y2;
                arc2.angle = (float)(Math.Atan2(dy, dx) * A);
                arc2.scale.x = (float)Math.Sqrt(dx * dx + dy * dy) / arc2.width;
                arc2.x = x2 - arc2.origin.x;
                arc2.y = y2 - arc2.origin.x;
            }
        }
    }
}