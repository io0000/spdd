using watabou.utils;
using watabou.noosa;

namespace spdd.sprites
{
    public class SwarmSprite : MobSprite
    {
        public SwarmSprite()
        {
            Texture(Assets.Sprites.SWARM);

            var frames = new TextureFilm(texture, 16, 16);

            idle = new Animation(15, true);
            idle.Frames(frames, 0, 1, 2, 3, 4, 5);

            run = new Animation(15, true);
            run.Frames(frames, 0, 1, 2, 3, 4, 5);

            attack = new Animation(20, false);
            attack.Frames(frames, 6, 7, 8, 9);

            die = new Animation(15, false);
            die.Frames(frames, 10, 11, 12, 13, 14);

            Play(idle);
        }

        public override Color Blood()
        {
            return new Color(0x8B, 0xA0, 0x77, 0xFF);
        }
    }
}