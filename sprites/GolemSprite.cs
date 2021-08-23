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
    public class GolemSprite : MobSprite
    {
        private Emitter teleParticles;

        public GolemSprite()
        {
            Texture(Assets.Sprites.GOLEM);

            var frames = new TextureFilm(texture, 17, 19);

            idle = new Animation(4, true);
            idle.Frames(frames, 0, 1);

            run = new Animation(12, true);
            run.Frames(frames, 2, 3, 4, 5);

            attack = new Animation(10, false);
            attack.Frames(frames, 6, 7, 8);

            die = new Animation(15, false);
            die.Frames(frames, 9, 10, 11, 12, 13);

            Play(idle);
        }

        public override void Link(Character ch)
        {
            base.Link(ch);

            teleParticles = Emitter();
            teleParticles.autoKill = false;
            teleParticles.Pour(ElmoParticle.Factory, 0.05f);
            teleParticles.on = false;
        }

        public override void Update()
        {
            base.Update();
            if (teleParticles != null)
            {
                teleParticles.Pos(this);
                teleParticles.visible = visible;
            }
        }

        public override void Kill()
        {
            base.Kill();

            if (teleParticles != null)
            {
                teleParticles.on = false;
            }
        }

        public void TeleParticles(bool value)
        {
            if (teleParticles != null)
                teleParticles.on = value;
        }

        public override void Play(Animation anim, bool force)
        {
            if (teleParticles != null)
                teleParticles.on = false;
            base.Play(anim, force);
        }

        public override Color Blood()
        {
            var color = new Color(0x80, 0x70, 0x6c, 0xFF);
            return color;
        }

        public override void Zap(int cell)
        {
            TurnTo(ch.pos, cell);
            Play(zap);

            var callback = new ActionCallback();
            callback.action = () =>
            {
                ((Golem)ch).OnZapComplete();
            };

            MagicMissile.BoltFromChar(parent,
                    MagicMissile.ELMO,
                    this,
                    cell,
                    callback);

            Sample.Instance.Play(Assets.Sounds.ZAP);
        }

        public override void OnComplete(Animation anim)
        {
            if (anim == die)
                Emitter().Burst(ElmoParticle.Factory, 4);
            else if (anim == zap)
                Idle();

            base.OnComplete(anim);
        }
    }
}