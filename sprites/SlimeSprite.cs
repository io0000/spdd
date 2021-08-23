using watabou.noosa;

namespace spdd.sprites
{
    public class SlimeSprite : MobSprite
    {
        public SlimeSprite()
        {
            Texture(Assets.Sprites.SLIME);

            var frames = new TextureFilm(texture, 14, 12);

            idle = new Animation(3, true);
            idle.Frames(frames, 0, 1, 1, 0);

            run = new Animation(10, true);
            run.Frames(frames, 0, 2, 3, 3, 2, 0);

            attack = new Animation(15, false);
            attack.Frames(frames, 2, 3, 4, 6, 5);

            die = new Animation(10, false);
            die.Frames(frames, 0, 5, 6, 7);

            Play(idle);
        }
    }
}