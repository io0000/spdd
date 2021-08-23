using System;
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
    public abstract class FistSprite : MobSprite
    {
        private const float SLAM_TIME = 0.33f;

        protected int boltType;

        protected abstract int TexOffset();

        private Emitter particles;
        protected abstract Emitter CreateEmitter();

        public FistSprite()
        {
            int c = TexOffset();

            Texture(Assets.Sprites.FISTS);

            var frames = new TextureFilm(texture, 24, 17);

            idle = new Animation(2, true);
            idle.Frames(frames, c + 0, c + 0, c + 1);

            run = new Animation(3, true);
            run.Frames(frames, c + 0, c + 1);

            attack = new Animation((int)Math.Round(1 / SLAM_TIME, MidpointRounding.AwayFromZero), false);
            attack.Frames(frames, c + 0);

            zap = new Animation(8, false);
            zap.Frames(frames, c + 0, c + 5, c + 6);

            die = new Animation(10, false);
            die.Frames(frames, c + 0, c + 2, c + 3, c + 4);

            Play(idle);
        }

        public override void Link(Character ch)
        {
            base.Link(ch);

            if (particles == null)
            {
                particles = CreateEmitter();
            }
        }

        public override void Update()
        {
            base.Update();

            if (particles != null)
            {
                particles.visible = visible;
            }
        }

        public override void Die()
        {
            base.Die();
            if (particles != null)
            {
                particles.on = false;
            }
        }

        public override void Kill()
        {
            base.Kill();
            if (particles != null)
            {
                particles.KillAndErase();
            }
        }

        public override void Attack(int cell)
        {
            base.Attack(cell);

            Jump(ch.pos, ch.pos, null, 9, SLAM_TIME);
        }

        public override void Zap(int cell)
        {
            TurnTo(ch.pos, cell);
            Play(zap);

            var callback = new ActionCallback();
            callback.action = () =>
            {
                //pre-0.8.0 saves
                if (ch is Yog.BurningFist)
                {
                    ((Yog.BurningFist)ch).OnZapComplete();
                }
                else
                {
                    ((YogFist)ch).OnZapComplete();
                }
            };

            MagicMissile.BoltFromChar(parent,
                    boltType,
                    this,
                    cell,
                    callback);
            Sample.Instance.Play(Assets.Sounds.ZAP);
        }

        public override void OnComplete(Animation anim)
        {
            base.OnComplete(anim);
            if (anim == attack)
            {
                Camera.main.Shake(4, 0.2f);
            }
            else if (anim == zap)
            {
                Idle();
            }
        }

        public class Burning : FistSprite
        {
            public Burning()
            {
                boltType = MagicMissile.FIRE;
            }

            protected override int TexOffset()
            {
                return 0;
            }

            protected override Emitter CreateEmitter()
            {
                Emitter emitter = Emitter();
                emitter.Pour(FlameParticle.Factory, 0.06f);
                return emitter;
            }

            public override Color Blood()
            {
                return new Color(0xFF, 0xDD, 0x34, 0xFF);
            }
        } // class Burning

        public class Soiled : FistSprite
        {
            public Soiled()
            {
                boltType = MagicMissile.FOLIAGE;
            }

            protected override int TexOffset()
            {
                return 10;
            }

            protected override Emitter CreateEmitter()
            {
                Emitter emitter = Emitter();
                emitter.Pour(LeafParticle.General, 0.06f);
                return emitter;
            }

            public override Color Blood()
            {
                return new Color(0x7F, 0x54, 0x24, 0xFF);
            }
        } // class Soiled

        public class Rotting : FistSprite
        {
            public Rotting()
            {
                boltType = MagicMissile.TOXIC_VENT;
            }

            protected override int TexOffset()
            {
                return 20;
            }

            protected override Emitter CreateEmitter()
            {
                Emitter emitter = Emitter();
                emitter.Pour(Speck.Factory(Speck.TOXIC), 0.06f);
                return emitter;
            }

            public override Color Blood()
            {
                return new Color(0xB8, 0xBB, 0xA1, 0xFF);
            }
        } // class Rotting

        public class Rusted : FistSprite
        {
            public Rusted()
            {
                boltType = MagicMissile.CORROSION;
            }

            protected override int TexOffset()
            {
                return 30;
            }

            protected override Emitter CreateEmitter()
            {
                Emitter emitter = Emitter();
                emitter.Pour(CorrosionParticle.Missile, 0.06f);
                return emitter;
            }

            public override Color Blood()
            {
                return new Color(0x7F, 0x7F, 0x7F, 0xFF);
            }
        } // class Rusted

        public class Bright : FistSprite
        {
            public Bright()
            {
                boltType = MagicMissile.RAINBOW;
            }

            protected override int TexOffset()
            {
                return 40;
            }

            protected override Emitter CreateEmitter()
            {
                Emitter emitter = Emitter();
                emitter.Pour(SparkParticle.Static, 0.06f);
                return emitter;
            }

            public override Color Blood()
            {
                return new Color(0xFF, 0xFF, 0xFF, 0xFF);
            }
        } // class Bright

        public class Dark : FistSprite
        {
            public Dark()
            {
                boltType = MagicMissile.SHADOW;
            }

            protected override int TexOffset()
            {
                return 50;
            }

            protected override Emitter CreateEmitter()
            {
                Emitter emitter = Emitter();
                emitter.Pour(ShadowParticle.Missile, 0.06f);
                return emitter;
            }

            public override Color Blood()
            {
                return new Color(0x4A, 0x2F, 0x53, 0xFF);
            }
        } // class Dark
    } // class FistSprite
}