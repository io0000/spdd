using watabou.utils;
using watabou.noosa;

namespace spdd.sprites
{
    public class EarthGuardianSprite : MobSprite
    {
        public EarthGuardianSprite()
        {
            Texture(Assets.Sprites.GUARDIAN);

            var frames = new TextureFilm(texture, 12, 15);

            idle = new Animation(2, true);
            idle.Frames(frames, 0, 0, 0, 0, 0, 1, 1);

            run = new Animation(15, true);
            run.Frames(frames, 2, 3, 4, 5, 6, 7);

            attack = new Animation(12, false);
            attack.Frames(frames, 8, 9, 10);

            die = new Animation(15, false);
            die.Frames(frames, 11, 12, 13, 14, 15, 15);

            Play(idle);
        }

        public override Color Blood()
        {
            var color = new Color(0xcd, 0xcd, 0xb7, 0xFF);
            return color;
        }
    }
}