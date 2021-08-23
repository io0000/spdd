using watabou.utils;
using watabou.noosa;

namespace spdd.sprites
{
    public class GreatCrabSprite : MobSprite
    {
        public GreatCrabSprite()
        {
            Texture(Assets.Sprites.CRAB);

            var frames = new TextureFilm(texture, 16, 16);

            idle = new Animation(5, true);
            idle.Frames(frames, 16, 17, 16, 18);

            run = new Animation(10, true);
            run.Frames(frames, 19, 20, 21, 22);

            attack = new Animation(12, false);
            attack.Frames(frames, 23, 24, 25);

            die = new Animation(12, false);
            die.Frames(frames, 26, 27, 28, 29);

            Play(idle);
        }

        public override Color Blood()
        {
            var color = new Color(0xFF, 0xEA, 0x80, 0xFF);
            return color;
        }
    }
}