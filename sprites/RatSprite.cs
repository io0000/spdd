using watabou.noosa;

namespace spdd.sprites
{
    public class RatSprite : MobSprite
    {
        public RatSprite()
        {
            Texture(Assets.Sprites.RAT);

            var frames = new TextureFilm(texture, 16, 15);

            idle = new Animation(2, true);
            idle.Frames(frames, 0, 0, 0, 1);

            run = new Animation(10, true);
            run.Frames(frames, 6, 7, 8, 9, 10);

            attack = new Animation(15, false);
            attack.Frames(frames, 2, 3, 4, 5, 0);

            die = new Animation(10, false);
            die.Frames(frames, 11, 12, 13, 14);

            Play(idle);
        }
    }
}