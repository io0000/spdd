using watabou.utils;
using watabou.noosa.particles;

namespace spdd.effects.particles
{
    public class LeafParticle : PixelParticle.Shrinking
    {
        //public static int color1;
        //public static int color2;

        public static Emitter.Factory General = new LeafParticleFactory1();
        public static Emitter.Factory LevelSpecific = new LeafParticleFactory2();

        public class LeafParticleFactory1 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                var p = emitter.Recycle<LeafParticle>();
                var color1 = new Color(0x00, 0x44, 0x00, 0xFF);
                var color2 = new Color(0x88, 0xCC, 0x44, 0xFF);
                p.SetColor(ColorMath.Random(color1, color2));
                p.Reset(x, y);
            }
        }

        public class LeafParticleFactory2 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                var positive = emitter.Recycle<LeafParticle>();
                positive.SetColor(ColorMath.Random(Dungeon.level.color1, Dungeon.level.color2));
                positive.Reset(x, y);
            }
        }

        public LeafParticle()
        {
            lifespan = 1.2f;
            acc.Set(0, 25);
        }

        public void Reset(float x, float y)
        {
            Revive();

            this.x = x;
            this.y = y;

            speed.Set(Rnd.Float(-8, +8), -20);

            left = lifespan;
            size = Rnd.Float(2, 3);
        }
    }
}