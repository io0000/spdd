using watabou.noosa;
using watabou.utils;
using spdd.items;
using spdd.actors;
using spdd.sprites;

namespace spdd.effects
{
    public class Enchanting : ItemSprite
    {
        //private const int SIZE = 16;

        private enum Phase
        {
            FADE_IN, STATIC, FADE_OUT
        }

        private const float FADE_IN_TIME = 0.2f;
        private const float STATIC_TIME = 1.0f;
        private const float FADE_OUT_TIME = 0.4f;

        private const float ALPHA = 0.6f;

        private Color color;

        private Character target;

        private Phase phase;
        private float duration;
        private float passed;

        public Enchanting(Item item)
            : base(item.Image(), null)
        {
            OriginToCenter();

            color = item.Glowing().color;

            phase = Phase.FADE_IN;
            duration = FADE_IN_TIME;
            passed = 0;
        }

        public override void Update()
        {
            base.Update();

            x = target.sprite.Center().x - SIZE / 2;
            y = target.sprite.y - SIZE;

            switch (phase)
            {
                case Phase.FADE_IN:
                    Alpha(passed / duration * ALPHA);
                    scale.Set(passed / duration);
                    break;
                case Phase.STATIC:
                    Tint(color, passed / duration * 0.8f);
                    break;
                case Phase.FADE_OUT:
                    Alpha((1 - passed / duration) * ALPHA);
                    scale.Set(1 + passed / duration);
                    break;
            }

            if ((passed += Game.elapsed) > duration)
            {
                switch (phase)
                {
                    case Phase.FADE_IN:
                        phase = Phase.STATIC;
                        duration = STATIC_TIME;
                        break;
                    case Phase.STATIC:
                        phase = Phase.FADE_OUT;
                        duration = FADE_OUT_TIME;
                        break;
                    case Phase.FADE_OUT:
                        Kill();
                        break;
                }

                passed = 0;
            }
        }

        public static void Show(Character ch, Item item)
        {
            if (!ch.sprite.visible)
                return;

            Enchanting sprite = new Enchanting(item);
            sprite.target = ch;
            ch.sprite.parent.Add(sprite);
        }
    }
}