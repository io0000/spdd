using watabou.noosa;
using watabou.utils;

namespace spdd.sprites
{
    public class BeeSprite : MobSprite
    {
        public BeeSprite()
        {
            Texture(Assets.Sprites.BEE);

            TextureFilm frames = new TextureFilm(texture, 16, 16);

            idle = new Animation(12, true);
            idle.Frames(frames, 0, 1, 1, 0, 2, 2);

            run = new Animation(15, true);
            run.Frames(frames, 0, 1, 1, 0, 2, 2);

            attack = new Animation(20, false);
            attack.Frames(frames, 3, 4, 5, 6);

            die = new Animation(20, false);
            die.Frames(frames, 7, 8, 9, 10);

            Play(idle);
        }

        public override Color Blood()
        {
            return new Color(0xff, 0xd5, 0x00, 0xff);
        }
    }
}