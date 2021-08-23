using watabou.glwrap;
using watabou.utils;

namespace watabou.noosa.particles
{
    public class Emitter : Group
    {
        protected bool lightMode;

        public float x;
        public float y;
        public float width;
        public float height;

        protected Visual target;
        public bool fillTarget = true;

        protected float interval;
        protected int quantity;

        public bool on;

        private bool started;
        public bool autoKill = true;

        protected int count;
        protected float time;

        protected Factory factory;

        public void Pos(float x, float y)
        {
            Pos(x, y, 0, 0);
        }

        public void Pos(PointF p)
        {
            Pos(p.x, p.y, 0.0f, 0.0f);
        }

        public void Pos(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;

            target = null;
        }

        public void Pos(Visual target)
        {
            this.target = target;
        }

        public void Pos(Visual target, float x, float y, float width, float height)
        {
            Pos(x, y, width, height);
            Pos(target);
        }

        public void Burst(Factory factory, int quantity)
        {
            Start(factory, 0, quantity);
        }

        public void Pour(Factory factory, float interval)
        {
            Start(factory, interval, 0);
        }

        public void Start(Factory factory, float interval, int quantity)
        {
            started = true;

            this.factory = factory;
            this.lightMode = factory.LightMode();

            this.interval = interval;
            this.quantity = quantity;

            count = 0;
            time = Rnd.Float(interval);

            on = true;
        }

        public static bool freezeEmitters;

        protected virtual bool IsFrozen()
        {
            return Game.timeTotal > 1.0f && freezeEmitters;
        }

        public override void Update()
        {
            if (IsFrozen())
                return;

            if (on)
            {
                time += Game.elapsed;
                while (time > interval)
                {
                    time -= interval;
                    Emit(count++);
                    if (quantity > 0 && count >= quantity)
                    {
                        on = false;
                        break;
                    }
                }
            }
            else if (started && autoKill && CountLiving() == 0)
            {
                Kill();
            }

            base.Update();
        }

        public override void Revive()
        {
            started = false;
            base.Revive();
        }

        public virtual void Emit(int index)
        {
            if (target == null)
            {
                factory.Emit(
                    this,
                    index,
                    x + Rnd.Float(width),
                    y + Rnd.Float(height));
            }
            else
            {
                if (fillTarget)
                {
                    factory.Emit(
                        this,
                        index,
                        target.x + Rnd.Float(target.width),
                        target.y + Rnd.Float(target.height));
                }
                else
                {
                    factory.Emit(
                        this,
                        index,
                        target.x + x + Rnd.Float(width),
                        target.y + y + Rnd.Float(height));
                }
            }
        }

        public override void Draw()
        {
            if (lightMode)
            {
                Blending.SetLightMode();
                base.Draw();
                Blending.SetNormalMode();
            }
            else
            {
                base.Draw();
            }
        }

        public abstract class Factory
        {
            public abstract void Emit(Emitter emitter, int index, float x, float y);

            public virtual bool LightMode()
            {
                return false;
            }
        }
    }
}