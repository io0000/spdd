using System;
using watabou.noosa;
using watabou.utils;
using spdd.effects;

namespace spdd.sprites
{
    public class SpawnerSprite : MobSprite
    {
        public SpawnerSprite()
        {
            Texture(Assets.Sprites.SPAWNER);

            perspectiveRaise = 8 / 16f;
            shadowOffset = 1.25f;
            shadowHeight = 0.4f;
            shadowWidth = 1f;

            var frames = new TextureFilm(texture, 16, 16);

            idle = new Animation(8, true);
            idle.Frames(frames, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);

            run = idle.Clone();

            attack = idle.Clone();

            die = idle.Clone();

            Play(idle);
        }

        private float baseY = float.NaN;

        public override void Place(int cell)
        {
            base.Place(cell);
            baseY = y;
        }

        public override void Update()
        {
            base.Update();
            if (!paused)
            {
                if (float.IsNaN(baseY))
                    baseY = y;

                y = baseY + (float)(Math.Sin(Game.timeTotal) / 3f);
                shadowOffset = 1.25f - 0.6f * (float)(Math.Sin(Game.timeTotal) / 3f);
            }
        }

        public override void Die()
        {
            Splash.At(Center(), Blood(), 100);
            KillAndErase();
        }

        public override void BloodBurstA(PointF from, int damage)
        {
            if (alive)
            {
                base.BloodBurstA(from, damage);
            }
        }
    }
}