using watabou.noosa;
using watabou.utils;

namespace spdd.sprites
{
    public class CrabSprite : MobSprite
    {
        public CrabSprite()
        {
            Texture(Assets.Sprites.CRAB);

            var frames = new TextureFilm(texture, 16, 16);

            idle = new Animation(5, true);
            idle.Frames(frames, 0, 1, 0, 2);

            run = new Animation(15, true);
            run.Frames(frames, 3, 4, 5, 6);

            attack = new Animation(12, false);
            attack.Frames(frames, 7, 8, 9);

            die = new Animation(12, false);
            die.Frames(frames, 10, 11, 12, 13);

            Play(idle);
        }

        public override Color Blood()
        {
            var color = new Color(0xFF, 0xEA, 0x80, 0xFF);
            return color;
        }
    }
}