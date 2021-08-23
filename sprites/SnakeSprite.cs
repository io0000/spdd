using watabou.noosa;

namespace spdd.sprites
{
    public class SnakeSprite : MobSprite
    {
        public SnakeSprite()
        {
            Texture(Assets.Sprites.SNAKE);

            var frames = new TextureFilm(texture, 12, 11);

            idle = new Animation(10, true);
            idle.Frames(frames, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                             1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 3, 2, 1, 1);

            run = new Animation(8, true);
            run.Frames(frames, 4, 5, 6, 7);

            attack = new Animation(15, false);
            attack.Frames(frames, 8, 9, 10, 9, 0);

            die = new Animation(10, false);
            die.Frames(frames, 11, 12, 13);

            Play(idle);
        }
    }
}