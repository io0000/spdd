using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.particles;
using watabou.utils;
using spdd.actors;
using spdd.actors.mobs;
using spdd.effects;
using spdd.effects.particles;
using spdd.tiles;

namespace spdd.sprites
{
    public abstract class ElementalSprite : MobSprite
    {
        protected int boltType;
        protected abstract int TexOffset();

        private Emitter particles;
        protected abstract Emitter CreateEmitter();

        public ElementalSprite()
        {
            int c = TexOffset();

            Texture(Assets.Sprites.ELEMENTAL);

            var frames = new TextureFilm(texture, 12, 14);

            idle = new Animation(10, true);
            idle.Frames(frames, c + 0, c + 1, c + 2);

            run = new Animation(12, true);
            run.Frames(frames, c + 0, c + 1, c + 3);

            attack = new Animation(15, false);
            attack.Frames(frames, c + 4, c + 5, c + 6);

            zap = attack.Clone();

            die = new Animation(15, false);
            die.Frames(frames, c + 7, c + 8, c + 9, c + 10, c + 11, c + 12, c + 13, c + 12);

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

        public override void Zap(int cell)
        {
            TurnTo(ch.pos, cell);
            Play(zap);

            var callback = new ActionCallback();
            callback.action = () =>
            {
                ((Elemental)ch).OnZapComplete();
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
            if (anim == zap)
            {
                Idle();
            }
            base.OnComplete(anim);
        }

        public class Fire : ElementalSprite
        {
            public Fire()
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
                var color = new Color(0xFF, 0xBB, 0x33, 0xFF);
                return color;
            }
        }

        public class NewbornFire : ElementalSprite
        {
            public NewbornFire()
            {
                boltType = MagicMissile.FIRE;
            }

            protected override int TexOffset()
            {
                return 14;
            }

            protected override Emitter CreateEmitter()
            {
                Emitter emitter = Emitter();
                emitter.Pour(ElmoParticle.Factory, 0.06f);
                return emitter;
            }

            public override Color Blood()
            {
                var color = new Color(0x85, 0xFF, 0xC8, 0xFF);
                return color;
            }
        }

        public class Frost : ElementalSprite
        {
            public Frost()
            {
                boltType = MagicMissile.FROST;
            }

            protected override int TexOffset()
            {
                return 28;
            }

            protected override Emitter CreateEmitter()
            {
                Emitter emitter = Emitter();
                emitter.Pour(MagicMissile.MagicParticle.Factory, 0.06f);
                return emitter;
            }

            public override Color Blood()
            {
                // 0xFF8EE3FF
                var color = new Color(0x8E, 0xE3, 0xFF, 0xFF);
                return color;
            }
        }

        public class Shock : ElementalSprite
        {
            //different bolt, so overrides zap
            public override void Zap(int cell)
            {
                TurnTo(ch.pos, cell);
                Play(zap);

                ((Elemental)ch).OnZapComplete();
                parent.Add(new Beam.LightRay(Center(), DungeonTilemap.RaisedTileCenterToWorld(cell)));
            }

            protected override int TexOffset()
            {
                return 42;
            }

            protected override Emitter CreateEmitter()
            {
                Emitter emitter = Emitter();
                emitter.Pour(SparkParticle.Factory, 0.06f);
                return emitter;
            }

            public override Color Blood()
            {
                // 0xFFFFFF85
                var color = new Color(0xFF, 0xFF, 0x85, 0xFF);
                return color;
            }
        }

        public class Chaos : ElementalSprite
        {
            public Chaos()
            {
                boltType = MagicMissile.RAINBOW;
            }

            protected override int TexOffset()
            {
                return 56;
            }

            protected override Emitter CreateEmitter()
            {
                Emitter emitter = Emitter();
                emitter.Pour(RainbowParticle.Burst, 0.025f);
                return emitter;
            }

            public override Color Blood()
            {
                // 0xFFE3E3E3
                var color = new Color(0xE3, 0xE3, 0xE3, 0xFF);
                return color;
            }
        }
    }
}