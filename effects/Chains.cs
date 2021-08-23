using System;
using watabou.noosa;
using watabou.utils;
using spdd.tiles;

namespace spdd.effects
{
    public class Chains : Group
    {
        private const double A = 180 / Math.PI;

        private float spent;
        private float duration;

        private ICallback callback;

        private Image[] chains;
        private int numChains;
        private float distance;
        private float rotation;

        private PointF from, to;

        public Chains(int from, int to, ICallback callback)
            : this(DungeonTilemap.TileCenterToWorld(from),
                DungeonTilemap.TileCenterToWorld(to),
                callback)
        {
        }

        public Chains(PointF from, PointF to, ICallback callback)
        {
            this.callback = callback;

            this.from = from;
            this.to = to;

            float dx = to.x - from.x;
            float dy = to.y - from.y;
            //distance = (float)Math.hypot(dx, dy);
            distance = (float)Math.Sqrt(dx * dx + dy * dy);

            duration = distance / 300f + 0.1f;

            rotation = (float)(Math.Atan2(dy, dx) * A) + 90f;

            numChains = (int)Math.Round(distance / 6f, MidpointRounding.AwayFromZero) + 1;

            chains = new Image[numChains];
            for (int i = 0; i < chains.Length; ++i)
            {
                chains[i] = new Image(Effects.Get(Effects.Type.CHAIN));
                chains[i].angle = rotation;
                chains[i].origin.Set(chains[i].Width() / 2, chains[i].Height());
                Add(chains[i]);
            }
        }

        public override void Update()
        {
            if ((spent += Game.elapsed) > duration)
            {
                KillAndErase();
                if (callback != null)
                {
                    callback.Call();
                }
            }
            else
            {
                float dx = to.x - from.x;
                float dy = to.y - from.y;
                for (int i = 0; i < chains.Length; ++i)
                {
                    chains[i].Center(new PointF(
                            from.x + ((dx * (i / (float)chains.Length)) * (spent / duration)),
                            from.y + ((dy * (i / (float)chains.Length)) * (spent / duration))
                    ));
                }
            }
        }
    }
}