using watabou.utils;
using watabou.noosa;

namespace spdd.sprites
{
    public class SheepSprite : MobSprite
    {
        public SheepSprite()
        {
            Texture(Assets.Sprites.SHEEP);

            var frames = new TextureFilm(texture, 16, 15);

            idle = new Animation(8, true);
            idle.Frames(frames, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 0);

            run = idle.Clone();
            attack = idle.Clone();

            die = new Animation(20, false);
            die.Frames(frames, 0);

            Play(idle);
            curFrame = Rnd.Int(curAnim.frames.Length);
        }
    }
}