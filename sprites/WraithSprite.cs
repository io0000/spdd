using watabou.utils;
using watabou.noosa;

namespace spdd.sprites
{
    public class WraithSprite : MobSprite
    {
        public WraithSprite()
        {
            Texture(Assets.Sprites.WRAITH);

            var frames = new TextureFilm(texture, 14, 15);

            idle = new Animation(5, true);
            idle.Frames(frames, 0, 1);

            run = new Animation(10, true);
            run.Frames(frames, 0, 1);

            attack = new Animation(10, false);
            attack.Frames(frames, 0, 2, 3);

            die = new Animation(8, false);
            die.Frames(frames, 0, 4, 5, 6, 7);

            Play(idle);
        }

        public override Color Blood()
        {
            return new Color(0x00, 0x00, 0x00, 0x88);
        }
    }
}