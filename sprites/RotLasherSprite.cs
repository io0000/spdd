using watabou.noosa;

namespace spdd.sprites
{
    public class RotLasherSprite : MobSprite
    {
        public RotLasherSprite()
        {
            Texture(Assets.Sprites.ROT_LASH);

            var frames = new TextureFilm(texture, 12, 16);

            idle = new Animation(0, true);
            idle.Frames(frames, 0);

            run = new Animation(0, true);
            run.Frames(frames, 0);

            attack = new Animation(24, false);
            attack.Frames(frames, 0, 1, 2, 2, 1);

            die = new Animation(12, false);
            die.Frames(frames, 3, 4, 5, 6);

            Play(idle);
        }
    }
}