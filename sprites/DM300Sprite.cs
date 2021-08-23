using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.particles;
using watabou.utils;
using spdd.actors;
using spdd.actors.mobs;
using spdd.effects;
using spdd.effects.particles;

namespace spdd.sprites
{
    public class DM300Sprite : MobSprite
    {
        private Animation charge;
        private Animation slam;

        private Emitter superchargeSparks;

        public DM300Sprite()
        {
            Texture(Assets.Sprites.DM300);

            SetAnimations(false);
        }

        private void SetAnimations(bool enraged)
        {
            int c = enraged ? 10 : 0;

            var frames = new TextureFilm(texture, 25, 22);

            idle = new Animation(enraged ? 15 : 10, true);
            idle.Frames(frames, c + 0, c + 1);

            run = new Animation(enraged ? 15 : 10, true);
            run.Frames(frames, c + 0, c + 2);

            attack = new Animation(15, false);
            attack.Frames(frames, c + 3, c + 4, c + 5);

            //unaffected by enrage state

            if (charge == null)
            {
                charge = new Animation(4, true);
                charge.Frames(frames, 0, 10);

                slam = attack.Clone();

                zap = new Animation(15, false);
                zap.Frames(frames, 6, 7, 7, 6);

                die = new Animation(20, false);
                die.Frames(frames, 0, 10, 0, 10, 0, 10, 0, 10, 0, 10, 0, 10, 0, 10, 0, 10, 0, 10, 0, 10);
            }

            if (curAnim != charge)
                Play(idle);
        }

        public override void Zap(int cell)
        {
            TurnTo(ch.pos, cell);
            Play(zap);

            var callback = new ActionCallback();
            callback.action = () =>
            {
                ((NewDM300)ch).OnZapComplete();
            };

            MagicMissile.BoltFromChar(parent,
                    MagicMissile.TOXIC_VENT,
                    this,
                    cell,
                    callback);
            Sample.Instance.Play(Assets.Sounds.PUFF);
        }

        public void Charge()
        {
            Play(charge);
        }

        public void Slam(int cell)
        {
            TurnTo(ch.pos, cell);
            Play(slam);
            Sample.Instance.Play(Assets.Sounds.ROCKS);
            Camera.main.Shake(3, 0.7f);
        }

        private bool exploded;

        public override void OnComplete(Animation anim)
        {
            if (anim == zap || anim == slam)
            {
                Idle();
            }

            if (anim == slam)
            {
                ((NewDM300)ch).OnSlamComplete();
            }

            base.OnComplete(anim);

            if (anim == die && !exploded)
            {
                exploded = true;
                Sample.Instance.Play(Assets.Sounds.BLAST);
                Emitter().Burst(BlastParticle.Factory, 100);
                KillAndErase();
            }
        }

        public override void Place(int cell)
        {
            if (parent != null)
                parent.BringToFront(this);

            base.Place(cell);
        }

        public override void Link(Character ch)
        {
            base.Link(ch);

            superchargeSparks = Emitter();
            superchargeSparks.autoKill = false;
            superchargeSparks.Pour(SparkParticle.Static, 0.05f);
            superchargeSparks.on = false;

            if (ch is NewDM300 && ((NewDM300)ch).IsSupercharged())
            {
                SetAnimations(true);
                superchargeSparks.on = true;
            }
        }

        public override void Update()
        {
            base.Update();

            if (superchargeSparks != null)
            {
                superchargeSparks.visible = visible;
                if (ch is NewDM300 &&
                    ((NewDM300)ch).IsSupercharged() != superchargeSparks.on)
                {
                    superchargeSparks.on = ((NewDM300)ch).IsSupercharged();
                    SetAnimations(((NewDM300)ch).IsSupercharged());
                }
            }
        }

        public override void Die()
        {
            base.Die();
            if (superchargeSparks != null)
            {
                superchargeSparks.on = false;
            }
        }

        public override void Kill()
        {
            base.Kill();
            if (superchargeSparks != null)
            {
                superchargeSparks.KillAndErase();
            }
        }

        public override Color Blood()
        {
            var color = new Color(0xFF, 0xFF, 0x88, 0xFF);
            return color;
        }
    }
}