using watabou.utils;
using watabou.noosa;
using spdd.effects;

namespace spdd.sprites
{
    public class LarvaSprite : MobSprite
    {
        public LarvaSprite()
        {
            Texture(Assets.Sprites.LARVA);

            var frames = new TextureFilm(texture, 12, 8);

            idle = new Animation(5, true);
            idle.Frames(frames, 4, 4, 4, 4, 4, 5, 5);

            run = new Animation(12, true);
            run.Frames(frames, 0, 1, 2, 3);

            attack = new Animation(15, false);
            attack.Frames(frames, 6, 5, 7);

            die = new Animation(10, false);
            die.Frames(frames, 8);

            Play(idle);
        }

        public override Color Blood()
        {
            return new Color(0xbb, 0xcc, 0x66, 0xFF);
        }

        public override void Die()
        {
            Splash.At(Center(), Blood(), 10);
            base.Die();
        }
    }
}