using System;
using watabou.utils;
using watabou.noosa;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;

namespace spdd.effects
{
    public class EmoIcon : Image
    {
        protected float maxSize = 2.0f;
        protected float timeScale = 1.0f;

        protected bool growing = true;

        protected CharSprite owner;

        public EmoIcon(CharSprite owner)
        {
            this.owner = owner;
            GameScene.Add(this);
        }

        public override void Update()
        {
            base.Update();

            if (!visible)
                return;

            if (growing)
            {
                float size = Math.Min(scale.x + Game.elapsed * timeScale, maxSize);
                scale.Set(size);
                if (scale.x >= maxSize)
                    growing = false;
            }
            else
            {
                float size = Math.Max(scale.x - Game.elapsed * timeScale, 1.0f);
                scale.Set(size);
                if (scale.x <= 1.0f)
                    growing = true;
            }

            x = owner.x + owner.Width() - width / 2;
            y = owner.y - height;
        }

        public class Sleep : EmoIcon
        {
            public Sleep(CharSprite owner)
                : base(owner)
            {
                Copy(Icons.SLEEP.Get());

                maxSize = 1.2f;
                timeScale = 0.5f;

                origin.Set(width / 2, height / 2);
                scale.Set(Rnd.Float(1, maxSize));

                x = owner.x + owner.width - width / 2;
                y = owner.y - height;
            }
        }

        public class Alert : EmoIcon
        {
            public Alert(CharSprite owner)
                : base(owner)
            {
                Copy(Icons.ALERT.Get());

                maxSize = 1.3f;
                timeScale = 2;

                origin.Set(2.5f, height - 2.5f);
                scale.Set(Rnd.Float(1, maxSize));

                x = owner.x + owner.width - width / 2;
                y = owner.y - height;
            }
        }

        public class Lost : EmoIcon
        {
            public Lost(CharSprite owner)
                : base(owner)
            {
                Copy(Icons.LOST.Get());

                maxSize = 1.25f;
                timeScale = 1.0f;

                origin.Set(2.5f, height - 2.5f);
                scale.Set(Rnd.Float(1, maxSize));

                x = owner.x + owner.width - width / 2;
                y = owner.y - height;
            }
        }
    }
}