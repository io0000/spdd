using watabou.noosa;

namespace spdd.sprites
{
    public class BatSprite : MobSprite
    {
        public BatSprite()
        {
            Texture(Assets.Sprites.BAT);

            TextureFilm frames = new TextureFilm(texture, 15, 15);

            idle = new Animation(8, true);
            idle.Frames(frames, 0, 1);

            run = new Animation(12, true);
            run.Frames(frames, 0, 1);

            attack = new Animation(12, false);
            attack.Frames(frames, 2, 3, 0, 1);

            die = new Animation(12, false);
            die.Frames(frames, 4, 5, 6);

            Play(idle);
        }
    }
}