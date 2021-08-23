using watabou.noosa;

namespace spdd.sprites
{
    public class ThiefSprite : MobSprite
    {
        public ThiefSprite()
        {
            Texture(Assets.Sprites.THIEF);

            var film = new TextureFilm(texture, 12, 13);

            idle = new Animation(1, true);
            idle.Frames(film, 0, 0, 0, 1, 0, 0, 0, 0, 1);

            run = new Animation(15, true);
            run.Frames(film, 0, 0, 2, 3, 3, 4);

            die = new Animation(10, false);
            die.Frames(film, 5, 6, 7, 8, 9);

            attack = new Animation(12, false);
            attack.Frames(film, 10, 11, 12, 0);

            Idle();
        }
    }
}