using watabou.glwrap;
using watabou.noosa;
using watabou.noosa.particles;
using watabou.noosa.ui;
using watabou.utils;

namespace spdd.effects
{
    public class Fireball : Component
    {
        private static readonly RectF BLIGHT = new RectF(0, 0, 0.25f, 1);
        private static readonly RectF FLIGHT = new RectF(0.25f, 0, 0.5f, 1);
        private static readonly RectF FLAME1 = new RectF(0.50f, 0, 0.75f, 1);
        private static readonly RectF FLAME2 = new RectF(0.75f, 0, 1.00f, 1);

        private static Color color = new Color(0xFF, 0x66, 0xFF, 0xFF);

        private Image bLight;
        private Image fLight;
        private Emitter emitter;
        private Group sparks;

        protected override void CreateChildren()
        {
            sparks = new Group();
            Add(sparks);

            bLight = new Image(Assets.Effects.FIREBALL);
            bLight.Frame(BLIGHT);
            bLight.origin.Set(bLight.width / 2);
            bLight.angularSpeed = -90;
            Add(bLight);

            emitter = new Emitter();
            emitter.Pour(new FireballEmitterFactory(this), 0.1f);
            Add(emitter);

            fLight = new Image(Assets.Effects.FIREBALL);
            fLight.Frame(FLIGHT);
            fLight.origin.Set(fLight.width / 2);
            fLight.angularSpeed = 360;
            Add(fLight);

            bLight.texture.Filter(Texture.LINEAR, Texture.LINEAR);
        }

        public class FireballEmitterFactory : Emitter.Factory
        {
            Fireball fireBall;

            public FireballEmitterFactory(Fireball fireBall)
            {
                this.fireBall = fireBall;
            }

            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                var p = emitter.Recycle<Fireball.Flame>();
                p.Reset();
                p.HeightLimit(fireBall.Top() - 30); // fireBall.Top() = y
                p.x = x - p.width / 2;
                p.y = y - p.height / 2;
            }
        }

        protected override void Layout()
        {
            bLight.x = x - bLight.width / 2;
            bLight.y = y - bLight.height / 2;

            emitter.Pos(
                x - bLight.width / 4,
                y - bLight.height / 4,
                bLight.width / 2,
                bLight.height / 2);

            fLight.x = x - fLight.width / 2;
            fLight.y = y - fLight.height / 2;
        }

        public override void Update()
        {
            base.Update();

            if (Rnd.Float() < Game.elapsed)
            {
                var c = new Color(0x66, 0xff, 0x66, 0xFF);

                var spark = (PixelParticle)sparks.Recycle<PixelParticle.Shrinking>();
                spark.Reset(x, y, ColorMath.Random(color, c), 2, Rnd.Float(0.5f, 1.0f));
                spark.speed.Set(
                    Rnd.Float(-40, +40),
                    Rnd.Float(-60, +20));
                spark.acc.Set(0, +80);
                sparks.Add(spark);
            }
        }

        public override void Draw()
        {
            Blending.SetLightMode();
            base.Draw();
            Blending.SetNormalMode();
        }

        public class Flame : Image
        {
            private const float LIFESPAN = 1f;

            private const float SPEED = -40f;
            private const float ACC = -20f;

            private float timeLeft;
            private float heightLimit;

            public Flame()
                : base(Assets.Effects.FIREBALL)
            {
                Frame(Rnd.Int(2) == 0 ? FLAME1 : FLAME2);
                origin.Set(width / 2, height / 2);
                acc.Set(0, ACC);
            }

            public void Reset()
            {
                Revive();
                timeLeft = LIFESPAN;
                speed.Set(0, SPEED);
            }

            public void HeightLimit(float limit)
            {
                heightLimit = limit;
            }

            public override void Update()
            {
                base.Update();

                if (y < heightLimit)
                {
                    y = heightLimit;
                    speed.Set(Rnd.Float(-20, 20), 0);
                    acc.Set(0, 0);
                }

                if ((timeLeft -= Game.elapsed) <= 0)
                {
                    Kill();
                }
                else
                {
                    var p = timeLeft / LIFESPAN;
                    scale.Set(p);
                    Alpha(p > 0.8f ? (1 - p) * 5f : p * 1.25f);
                }
            }
        }
    }
}