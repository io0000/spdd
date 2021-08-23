using watabou.noosa;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.mobs;
using spdd.effects.particles;

namespace spdd.sprites
{
    public class PylonSprite : MobSprite
    {
        private Animation activeIdle;

        public PylonSprite()
        {
            perspectiveRaise = 5 / 16f; //1 pixel less
            renderShadow = false;

            Texture(Assets.Sprites.PYLON);

            var frames = new TextureFilm(texture, 10, 20);

            idle = new Animation(1, false);
            idle.Frames(frames, 0);

            activeIdle = new Animation(1, false);
            activeIdle.Frames(frames, 1);

            run = idle.Clone();

            attack = idle.Clone();

            die = new Animation(1, false);
            die.Frames(frames, 2);

            Play(idle);
        }

        public override void Link(Character ch)
        {
            base.Link(ch);

            if (ch is Pylon && ch.alignment == Character.Alignment.ENEMY)
                Activate();

            renderShadow = false;
        }

        public override void Place(int cell)
        {
            if (parent != null)
                parent.BringToFront(this);
            base.Place(cell);
        }

        public void Activate()
        {
            idle = activeIdle.Clone();
            Idle();
        }

        public override void Play(Animation anim)
        {
            if (anim == die)
            {
                TurnTo(ch.pos, ch.pos + 1); //always face right to merge with custom tiles
                Emitter().Burst(BlastParticle.Factory, 20);
                Sample.Instance.Play(Assets.Sounds.BLAST);
            }
            base.Play(anim);
        }

        public override void OnComplete(Animation anim)
        {
            if (anim == attack)
                Flash();

            base.OnComplete(anim);
        }
    }
}