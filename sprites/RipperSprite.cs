using watabou.noosa;
using watabou.utils;

namespace spdd.sprites
{
    public class RipperSprite : MobSprite
    {
        private Animation stab;
        private Animation prep;
        private Animation leap;

        private bool alt = Rnd.Int(2) == 0;

        public RipperSprite()
        {
            Texture(Assets.Sprites.RIPPER);

            var frames = new TextureFilm(texture, 15, 14);

            idle = new Animation(4, true);
            idle.Frames(frames, 1, 0, 1, 2);

            run = new Animation(15, true);
            run.Frames(frames, 3, 4, 5, 6, 7, 8);

            attack = new Animation(12, false);
            attack.Frames(frames, 0, 9, 10, 9);

            stab = new Animation(12, false);
            stab.Frames(frames, 0, 9, 11, 9);

            prep = new Animation(1, true);
            prep.Frames(frames, 9);

            leap = new Animation(1, true);
            leap.Frames(frames, 12);

            die = new Animation(15, false);
            die.Frames(frames, 1, 13, 14, 15, 16);

            Play(idle);
        }

        public void LeapPrep(int cell)
        {
            TurnTo(ch.pos, cell);
            Play(prep);
        }

        public override void Jump(int from, int to, ICallback callback)
        {
            base.Jump(from, to, callback);
            Play(leap);
        }

        public override void Attack(int cell)
        {
            base.Attack(cell);
            if (alt)
            {
                Play(stab);
            }
            alt = !alt;
        }

        public override void OnComplete(Animation anim)
        {
            base.OnComplete(anim == stab ? attack : anim);
        }
    }
}