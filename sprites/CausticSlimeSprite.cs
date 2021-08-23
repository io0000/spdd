using watabou.noosa;

namespace spdd.sprites
{
    public class CausticSlimeSprite : MobSprite
    {
        public CausticSlimeSprite()
        {
            Texture(Assets.Sprites.SLIME);

            TextureFilm frames = new TextureFilm(texture, 14, 12);

            int c = 9;

            idle = new Animation(3, true);
            idle.Frames(frames, c + 0, c + 1, c + 1, c + 0);

            run = new Animation(10, true);
            run.Frames(frames, c + 0, c + 2, c + 3, c + 3, c + 2, c + 0);

            attack = new Animation(15, false);
            attack.Frames(frames, c + 2, c + 3, c + 4, c + 6, c + 5);

            die = new Animation(10, false);
            die.Frames(frames, c + 0, c + 5, c + 6, c + 7);

            Play(idle);
        }
    }
}