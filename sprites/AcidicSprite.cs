using watabou.noosa;
using watabou.utils;

namespace spdd.sprites
{
    public class AcidicSprite : ScorpioSprite
    {
        public AcidicSprite()
        {
            Texture(Assets.Sprites.SCORPIO);

            var frames = new TextureFilm(texture, 18, 17);

            idle = new Animation(12, true);
            idle.Frames(frames, 14, 14, 14, 14, 14, 14, 14, 14, 15, 16, 15, 16, 15, 16);

            run = new Animation(4, true);
            run.Frames(frames, 19, 20);

            attack = new Animation(15, false);
            attack.Frames(frames, 14, 17, 18);

            zap = attack.Clone();

            die = new Animation(12, false);
            die.Frames(frames, 14, 21, 22, 23, 24);

            Play(idle);
        }

        public override Color Blood()
        {
            return new Color(0x66, 0xFF, 0x22, 0xFF);
        }
    }
}