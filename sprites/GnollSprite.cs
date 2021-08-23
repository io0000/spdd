using watabou.noosa;

namespace spdd.sprites
{
    public class GnollSprite : MobSprite
    {
        public GnollSprite()
        {
            Texture(Assets.Sprites.GNOLL);

            var frames = new TextureFilm(texture, 12, 15);

            idle = new Animation(2, true);
            idle.Frames(frames, 0, 0, 0, 1, 0, 0, 1, 1);

            run = new Animation(12, true);
            run.Frames(frames, 4, 5, 6, 7);

            attack = new Animation(12, false);
            attack.Frames(frames, 2, 3, 0);

            die = new Animation(12, false);
            die.Frames(frames, 8, 9, 10);

            Play(idle);
        }
    }
}