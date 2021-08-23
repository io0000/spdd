using watabou.utils;
using watabou.noosa;

namespace spdd.sprites
{
    public class SeniorSprite : MobSprite
    {
        private Animation kick;

        public SeniorSprite()
        {
            Texture(Assets.Sprites.MONK);

            var frames = new TextureFilm(texture, 15, 14);

            idle = new Animation(6, true);
            idle.Frames(frames, 18, 17, 18, 19);

            run = new Animation(15, true);
            run.Frames(frames, 28, 29, 30, 31, 32, 33);

            attack = new Animation(12, false);
            attack.Frames(frames, 20, 21, 20, 21);

            kick = new Animation(10, false);
            kick.Frames(frames, 22, 23, 22);

            die = new Animation(15, false);
            die.Frames(frames, 18, 24, 25, 25, 26, 27);

            Play(idle);
        }

        public override void Attack(int cell)
        {
            base.Attack(cell);

            if (Rnd.Float() < 0.3f)
                Play(kick);
        }

        public override void OnComplete(Animation anim)
        {
            base.OnComplete(anim == kick ? attack : anim);
        }
    }
}