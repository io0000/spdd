using watabou.noosa.particles;
using watabou.utils;
using spdd.tiles;

namespace spdd.effects.particles
{
    public class WindParticle : PixelParticle
    {
        public static Emitter.Factory Factory = new WindParticleFactory();

        public class WindParticleFactory : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<WindParticle>().Reset(x, y);
            }
        }

        private new static float angle = Rnd.Float(PointF.PI2);
        private new static PointF speed = new PointF().Polar(angle, 5);

        public WindParticle()
        {
            lifespan = Rnd.Float(1, 2);
            scale.Set(size = Rnd.Float(3));
        }

        public void Reset(float x, float y)
        {
            Revive();

            left = lifespan;

            base.speed.Set(WindParticle.speed);
            base.speed.Scale(size);

            this.x = x - base.speed.x * lifespan / 2;
            this.y = y - base.speed.y * lifespan / 2;

            angle += Rnd.Float(-0.1f, +0.1f);
            speed = new PointF().Polar(angle, 5);

            am = 0;
        }

        public override void Update()
        {
            base.Update();

            var p = left / lifespan;
            am = (p < 0.5f ? p : 1 - p) * size * 0.2f;
        }

        public class Wind : Emitter
        {
            private int pos;

            public Wind(int pos)
            {
                this.pos = pos;
                var p = DungeonTilemap.TileToWorld(pos);
                Pos(p.x, p.y, DungeonTilemap.SIZE, DungeonTilemap.SIZE);

                Pour(WindParticle.Factory, 2.5f);
            }

            public override void Update()
            {
                if (visible = (pos < Dungeon.level.heroFOV.Length && Dungeon.level.heroFOV[pos]))
                {
                    base.Update();
                }
            }
        }
    }
}