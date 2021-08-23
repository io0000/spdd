using watabou.noosa;

namespace spdd.sprites
{
    public class ShieldedSprite : MobSprite
    {
        public ShieldedSprite()
        {
            Texture(Assets.Sprites.BRUTE);

            var frames = new TextureFilm(texture, 12, 16);

            idle = new Animation(2, true);
            idle.Frames(frames, 21, 21, 21, 22, 21, 21, 22, 22);

            run = new Animation(12, true);
            run.Frames(frames, 25, 26, 27, 28);

            attack = new Animation(12, false);
            attack.Frames(frames, 23, 24);

            die = new Animation(12, false);
            die.Frames(frames, 29, 30, 31);

            Play(idle);
        }
    }
}