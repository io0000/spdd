using spdd.actors;

namespace spdd.ui
{
    public class TargetHealthIndicator : HealthBar
    {
        public static TargetHealthIndicator instance;

        private Character target;

        public TargetHealthIndicator()
        {
            instance = this;
        }

        public override void Update()
        {
            base.Update();

            if (target != null && target.IsAlive() && target.sprite.visible)
            {
                var sprite = target.sprite;
                width = sprite.Width();
                x = sprite.x;
                y = sprite.y - 3;
                Level(target);
                visible = true;
            }
            else
            {
                visible = false;
            }
        }

        public void Target(Character ch)
        {
            if (ch != null && ch.IsAlive())
                target = ch;
            else
                target = null;
        }

        //public Character Target()
        //{
        //    return target;
        //}
    }
}