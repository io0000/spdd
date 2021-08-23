using watabou.noosa;

namespace spdd.sprites
{
    public class AlbinoSprite : MobSprite
    {
        public AlbinoSprite()
        {
            Texture(Assets.Sprites.RAT);

            var frames = new TextureFilm(texture, 16, 15);

            idle = new Animation(2, true);
            idle.Frames(frames, 16, 16, 16, 17);

            run = new Animation(10, true);
            run.Frames(frames, 22, 23, 24, 25, 26);

            attack = new Animation(15, false);
            attack.Frames(frames, 18, 19, 20, 21);

            die = new Animation(10, false);
            die.Frames(frames, 27, 28, 29, 30);

            Play(idle);
        }
    }
}