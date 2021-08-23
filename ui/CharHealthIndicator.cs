using spdd.actors;
using spdd.scenes;
using spdd.sprites;

namespace spdd.ui
{
    public class CharHealthIndicator : HealthBar
    {
        private const float HEIGHT = 1;
        private Character target;

        public CharHealthIndicator(Character c)
        {
            target = c;
            GameScene.Add(this);
        }

        protected override void CreateChildren()
        {
            base.CreateChildren();
            height = HEIGHT;
        }

        public override void Update()
        {
            base.Update();

            if (target != null && target.IsAlive() && target.sprite.visible)
            {
                CharSprite sprite = target.sprite;
                width = sprite.Width() * (4.0f / 6.0f);
                x = sprite.x + sprite.Width() / 6.0f;
                y = sprite.y - 2.0f;
                Level(target);
                visible = target.HP < target.HT || target.Shielding() > 0;
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

        public Character Target()
        {
            return target;
        }
    }
}