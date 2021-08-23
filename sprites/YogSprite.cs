using watabou.noosa;
using spdd.effects;
using spdd.actors;

namespace spdd.sprites
{
    public class YogSprite : MobSprite
    {
        public YogSprite()
        {
            perspectiveRaise = 5 / 16f;

            Texture(Assets.Sprites.YOG);

            var frames = new TextureFilm(texture, 20, 19);

            idle = new Animation(10, true);
            idle.Frames(frames, 0, 1, 2, 2, 1, 0, 3, 4, 4, 3, 0, 5, 6, 6, 5);

            run = new Animation(12, true);
            run.Frames(frames, 0);

            attack = new Animation(12, false);
            attack.Frames(frames, 0);

            die = new Animation(10, false);
            die.Frames(frames, 0, 7, 8, 9);

            Play(idle);
        }

        public override void Link(Character ch)
        {
            base.Link(ch);
            renderShadow = false;
        }

        public override void Die()
        {
            base.Die();

            Splash.At(Center(), Blood(), 12);
        }
    }
}