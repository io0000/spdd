using watabou.noosa;
using spdd.scenes;
using spdd.actors;

namespace spdd.sprites
{
    public class PiranhaSprite : MobSprite
    {
        public PiranhaSprite()
        {
            renderShadow = false;
            perspectiveRaise = 0.2f;

            Texture(Assets.Sprites.PIRANHA);

            var frames = new TextureFilm(texture, 12, 16);

            idle = new Animation(8, true);
            idle.Frames(frames, 0, 1, 2, 1);

            run = new Animation(20, true);
            run.Frames(frames, 0, 1, 2, 1);

            attack = new Animation(20, false);
            attack.Frames(frames, 3, 4, 5, 6, 7, 8, 9, 10, 11);

            die = new Animation(4, false);
            die.Frames(frames, 12, 13, 14);

            Play(idle);
        }

        public override void Link(Character ch)
        {
            base.Link(ch);
            renderShadow = false;
        }

        public override void OnComplete(Animation anim)
        {
            base.OnComplete(anim);

            if (anim == attack)
                GameScene.Ripple(ch.pos);
        }
    }
}