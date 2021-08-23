using watabou.noosa;
using watabou.utils;

namespace spdd.sprites
{
    public class MonkSprite : MobSprite
    {
        private Animation kick;

        public MonkSprite()
        {
            Texture(Assets.Sprites.MONK);

            var frames = new TextureFilm(texture, 15, 14);

            idle = new Animation(6, true);
            idle.Frames(frames, 1, 0, 1, 2);

            run = new Animation(15, true);
            run.Frames(frames, 11, 12, 13, 14, 15, 16);

            attack = new Animation(12, false);
            attack.Frames(frames, 3, 4, 3, 4);

            kick = new Animation(10, false);
            kick.Frames(frames, 5, 6, 5);

            die = new Animation(15, false);
            die.Frames(frames, 1, 7, 8, 8, 9, 10);

            Play(idle);
        }

        public override void Attack(int cell)
        {
            base.Attack(cell);
            if (Rnd.Float() < 0.5f)
                Play(kick);
        }

        public override void OnComplete(Animation anim)
        {
            base.OnComplete(anim == kick ? attack : anim);
        }
    }
}