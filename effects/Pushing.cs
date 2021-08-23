using watabou.noosa;
using watabou.utils;
using spdd.actors;
using spdd.sprites;

namespace spdd.effects
{
    public class Pushing : Actor
    {
        public readonly CharSprite sprite;
        public readonly int from;
        public readonly int to;

        private Effect effect;

        private ICallback callback;

        private void InitInstance()
        {
            actPriority = VFX_PRIO;
        }

        public Pushing(Character ch, int from, int to)
        {
            InitInstance();

            sprite = ch.sprite;
            this.from = from;
            this.to = to;
            this.callback = null;
        }

        public Pushing(Character ch, int from, int to, ICallback callback)
            : this(ch, from, to)
        {
            this.callback = callback;
        }

        public override bool Act()
        {
            if (sprite != null)
            {
                if (effect == null)
                {
                    //new Effect(this);
                    effect = new Effect(this);  // TODO»Æ¿Œ
                }
            }

            Actor.Remove(this);

            foreach (var actor in Actor.All())
            {
                if (actor is Pushing && actor.Cooldown() == 0)
                    return true;
            }

            return false;
        }

        public class Effect : Visual
        {
            private readonly Pushing pushing;
            
            private const float DELAY = 0.15f;
            
            private readonly PointF end;

            private float delay;

            public Effect(Pushing pushing)
                : base(0, 0, 0, 0)
            {
                this.pushing = pushing;

                CharSprite sprite = pushing.sprite;
                int from = pushing.from;
                int to = pushing.to;

                Point(sprite.WorldToCamera(from));
                end = sprite.WorldToCamera(to);

                speed.Set(2 * (end.x - x) / DELAY, 2 * (end.y - y) / DELAY);
                acc.Set(-speed.x / DELAY, -speed.y / DELAY);

                delay = 0;

                if (sprite.parent != null)
                    sprite.parent.Add(this);
            }

            public override void Update()
            {
                base.Update();

                CharSprite sprite = pushing.sprite;

                if ((delay += Game.elapsed) < DELAY)
                {
                    sprite.x = x;
                    sprite.y = y;
                }
                else
                {
                    sprite.Point(end);

                    KillAndErase();
                    Actor.Remove(pushing);
                    if (pushing.callback != null)
                        pushing.callback.Call();

                    pushing.Next();
                }
            }
        }
    }
}