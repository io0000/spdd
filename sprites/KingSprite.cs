using watabou.noosa;

namespace spdd.sprites
{
    public class KingSprite : MobSprite
    {
        public KingSprite()
        {
            Texture(Assets.Sprites.KING);

            var frames = new TextureFilm(texture, 16, 16);

            idle = new Animation(12, true);
            idle.Frames(frames, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2);

            run = new Animation(15, true);
            run.Frames(frames, 3, 4, 5, 6, 7, 8);

            attack = new Animation(15, false);
            attack.Frames(frames, 9, 10, 11);

            die = new Animation(8, false);
            die.Frames(frames, 12, 13, 14, 15);

            Play(idle);
        }
    }
}