using System.Collections.Generic;
using watabou.noosa;
using watabou.noosa.particles;
using watabou.utils;
using spdd.actors;
using spdd.effects;
using spdd.effects.particles;
using spdd.utils;

namespace spdd.sprites
{
    public class GooSprite : MobSprite
    {
        private Animation pump;
        private Animation pumpAttack;

        private Emitter spray;
        private List<Emitter> pumpUpEmitters = new List<Emitter>();

        public GooSprite()
        {
            Texture(Assets.Sprites.GOO);

            var frames = new TextureFilm(texture, 20, 14);

            idle = new Animation(10, true);
            idle.Frames(frames, 2, 1, 0, 0, 1);

            run = new Animation(15, true);
            run.Frames(frames, 3, 2, 1, 2);

            pump = new Animation(20, true);
            pump.Frames(frames, 4, 3, 2, 1, 0);

            pumpAttack = new Animation(20, false);
            pumpAttack.Frames(frames, 4, 3, 2, 1, 0, 7);

            attack = new Animation(10, false);
            attack.Frames(frames, 8, 9, 10);

            die = new Animation(10, false);
            die.Frames(frames, 5, 6, 7);

            Play(idle);

            spray = CenterEmitter();
            spray.autoKill = false;
            spray.Pour(GooParticle.Factory, 0.04f);
            spray.on = false;
        }

        public override void Link(Character ch)
        {
            base.Link(ch);
            if (ch.HP * 2 <= ch.HT)
                Spray(true);
        }

        public void PumpUp(int warnDist)
        {
            if (warnDist == 0)
            {
                foreach (var e in pumpUpEmitters)
                {
                    e.on = false;
                }
                pumpUpEmitters.Clear();
            }
            else
            {
                Play(pump);
                PathFinder.BuildDistanceMap(ch.pos, BArray.Not(Dungeon.level.solid, null), 2);

                for (int i = 0; i < PathFinder.distance.Length; ++i)
                {
                    if (PathFinder.distance[i] <= warnDist)
                    {
                        Emitter e = CellEmitter.Get(i);
                        e.Pour(GooParticle.Factory, 0.04f);
                        pumpUpEmitters.Add(e);
                    }
                }
            }
        }

        public void PumpAttack()
        {
            Play(pumpAttack);
        }

        public override void Play(Animation anim)
        {
            if (anim != pump && anim != pumpAttack)
            {
                foreach (var e in pumpUpEmitters)
                {
                    e.on = false;
                }
                pumpUpEmitters.Clear();
            }
            base.Play(anim);
        }

        public override Color Blood()
        {
            return new Color(0x00, 0x00, 0x00, 0xFF);
        }

        public void Spray(bool on)
        {
            spray.on = on;
        }

        public override void Update()
        {
            base.Update();
            spray.Pos(Center());
            spray.visible = visible;
        }

        public class GooParticle : PixelParticle.Shrinking
        {
            public static Emitter.Factory Factory = new GooParticleFactory();

            public class GooParticleFactory : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    var particle = emitter.Recycle<GooParticle>();
                    particle.Reset(x, y);
                }
            }

            public GooParticle()
            {
                SetColor(new Color(0x00, 0x00, 0x00, 0xFF));
                lifespan = 0.3f;

                acc.Set(0, +50);
            }

            public void Reset(float x, float y)
            {
                Revive();

                this.x = x;
                this.y = y;

                left = lifespan;

                size = 4;
                speed.Polar(-Rnd.Float(PointF.PI), Rnd.Float(32, 48));
            }

            public override void Update()
            {
                base.Update();
                float p = left / lifespan;
                am = p > 0.5f ? (1 - p) * 2f : 1;
            }
        } // GooParticle


        public override void OnComplete(Animation anim)
        {
            base.OnComplete(anim);

            if (anim == pumpAttack)
            {
                foreach (Emitter e in pumpUpEmitters)
                {
                    e.Burst(ElmoParticle.Factory, 10);
                }
                pumpUpEmitters.Clear();

                Idle();
                ch.OnAttackComplete();
            }
            else if (anim == die)
            {
                spray.KillAndErase();
            }
        }
    }
}