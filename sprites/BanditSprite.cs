using watabou.noosa;

namespace spdd.sprites
{
    public class BanditSprite : MobSprite
    {
        public BanditSprite()
        {
            Texture(Assets.Sprites.THIEF);

            var film = new TextureFilm(texture, 12, 13);

            idle = new Animation(1, true);
            idle.Frames(film, 21, 21, 21, 22, 21, 21, 21, 21, 22);

            run = new Animation(15, true);
            run.Frames(film, 21, 21, 23, 24, 24, 25);

            die = new Animation(10, false);
            die.Frames(film, 25, 27, 28, 29, 30);

            attack = new Animation(12, false);
            attack.Frames(film, 31, 32, 33);

            Idle();
        }
    }
}