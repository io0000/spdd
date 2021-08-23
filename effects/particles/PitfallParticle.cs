using watabou.utils;
using watabou.noosa.particles;

namespace spdd.effects.particles
{
    public class PitfallParticle : PixelParticle.Shrinking
    {
        public static Emitter.Factory Factory4 = new PitfallParticleFactory1();
        public static Emitter.Factory Factory8 = new PitfallParticleFactory2();

        public class PitfallParticleFactory1 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<PitfallParticle>().Reset(x, y, 4);
            }
        }

        public class PitfallParticleFactory2 : Emitter.Factory
        {
            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                emitter.Recycle<PitfallParticle>().Reset(x, y, 8);
            }
        }

        public PitfallParticle()
        {
            SetColor(new Color(0x00, 0x00, 0x00, 0xFF));
            angle = Rnd.Float(-30, 30);
        }

        public void Reset(float x, float y, int size)
        {
            Revive();

            this.x = x;
            this.y = y;

            left = lifespan = 1.0f;

            this.size = size;
        }
    }
}