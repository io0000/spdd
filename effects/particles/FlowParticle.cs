using watabou.noosa.particles;
using watabou.utils;
using spdd.tiles;

namespace spdd.effects.particles
{
    public class FlowParticle : PixelParticle
    {
        public static Emitter.Factory Factory = new FlowParticleFactory();

        public class FlowParticleFactory : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<FlowParticle>().Reset(x, y);
            }
        }

        public FlowParticle()
        {
            lifespan = 0.6f;
            acc.Set(0, 32);
            angularSpeed = Rnd.Float(-360, +360);
        }

        public void Reset(float x, float y)
        {
            Revive();

            left = lifespan;

            this.x = x;
            this.y = y;

            am = 0;
            Size(0);
            speed.Set(0);
        }

        public override void Update()
        {
            base.Update();

            var p = left / lifespan;
            am = (p < 0.5f ? p : 1 - p) * 0.6f;
            Size((1 - p) * 4);
        }

        public class Flow : Emitter
        {
            private int pos;

            public Flow(int pos)
            {
                this.pos = pos;

                var p = DungeonTilemap.TileToWorld(pos);
                Pos(p.x, p.y + DungeonTilemap.SIZE - 1, DungeonTilemap.SIZE, 0);

                Pour(FlowParticle.Factory, 0.05f);
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