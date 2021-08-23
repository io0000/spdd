using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.particles;
using spdd.actors;
using spdd.actors.mobs;
using spdd.effects;
using spdd.tiles;

namespace spdd.sprites
{
    public class EyeSprite : MobSprite
    {
        private int zapPos;

        private Animation charging;
        private Emitter chargeParticles;

        public EyeSprite()
        {
            Texture(Assets.Sprites.EYE);

            var frames = new TextureFilm(texture, 16, 18);

            idle = new Animation(8, true);
            idle.Frames(frames, 0, 1, 2);

            charging = new Animation(12, true);
            charging.Frames(frames, 3, 4);

            run = new Animation(12, true);
            run.Frames(frames, 5, 6);

            attack = new Animation(8, false);
            attack.Frames(frames, 4, 3);
            zap = attack.Clone();

            die = new Animation(8, false);
            die.Frames(frames, 7, 8, 9);

            Play(idle);
        }

        public override void Link(Character ch)
        {
            base.Link(ch);

            chargeParticles = CenterEmitter();
            chargeParticles.autoKill = false;
            chargeParticles.Pour(MagicMissile.MagicParticle.Attracting, 0.05f);
            chargeParticles.on = false;

            if (((Eye)ch).beamCharged)
                Play(charging);
        }

        public override void Update()
        {
            base.Update();
            if (chargeParticles != null)
            {
                chargeParticles.Pos(Center());
                chargeParticles.visible = visible;
            }
        }

        public override void Die()
        {
            base.Die();
            if (chargeParticles != null)
            {
                chargeParticles.on = false;
            }
        }

        public override void Kill()
        {
            base.Kill();
            if (chargeParticles != null)
            {
                chargeParticles.KillAndErase();
            }
        }

        public void Charge(int pos)
        {
            TurnTo(ch.pos, pos);
            Play(charging);
            if (visible)
                Sample.Instance.Play(Assets.Sounds.CHARGEUP);
        }

        public override void Play(Animation anim)
        {
            if (chargeParticles != null)
                chargeParticles.on = anim == charging;
            base.Play(anim);
        }

        public override void Zap(int pos)
        {
            zapPos = pos;
            base.Zap(pos);
        }

        public override void OnComplete(Animation anim)
        {
            base.OnComplete(anim);

            if (anim == zap)
            {
                Idle();
                if (Actor.FindChar(zapPos) != null)
                {
                    parent.Add(new Beam.DeathRay(Center(), Actor.FindChar(zapPos).sprite.Center()));
                }
                else
                {
                    parent.Add(new Beam.DeathRay(Center(), DungeonTilemap.RaisedTileCenterToWorld(zapPos)));
                }
                ((Eye)ch).DoDeathGaze();
                ch.Next();
            }
            else if (anim == die)
            {
                chargeParticles.KillAndErase();
            }
        }
    }
}