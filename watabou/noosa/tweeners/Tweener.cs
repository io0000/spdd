namespace watabou.noosa.tweeners
{
    public abstract class Tweener : Gizmo
    {
        public Gizmo target;

        public float interval;
        public float elapsed;

        public IListener listener;

        protected Tweener(Gizmo target, float interval)
        {
            this.target = target;
            this.interval = interval;

            elapsed = 0.0f;
        }

        public override void Update()
        {
            if (elapsed < 0)
            {
                OnComplete();
                Kill();
                return;
            }

            elapsed += Game.elapsed;

            //it's better to skip this frame ahead and finish one frame early
            // if doing one more frame would result in lots of overshoot
            if ((interval - elapsed) < Game.elapsed / 2.0f)
                elapsed = interval;

            if (elapsed >= interval)
            {
                UpdateValues(1.0f);
                OnComplete();
                Kill();
            }
            else
            {
                UpdateValues(elapsed / interval);
            }
        }

        public void Stop(bool complete)
        {
            elapsed = complete ? interval : -1.0f;
        }

        public virtual void OnComplete()
        {
            if (listener != null)
                listener.OnComplete(this);
        }

        public abstract void UpdateValues(float progress);

        public interface IListener
        {
            void OnComplete(Tweener tweener);
        }
    }
}