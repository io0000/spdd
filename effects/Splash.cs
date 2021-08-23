using watabou.utils;
using watabou.noosa.particles;
using spdd.scenes;
using spdd.tiles;

namespace spdd.effects
{
    public class Splash
    {
        public static void At(int cell, Color color, int n)
        {
            At(DungeonTilemap.TileCenterToWorld(cell), color, n);
        }

        public static void At(PointF p, Color color, int n)
        {
            if (n <= 0)
                return;

            var emitter = GameScene.GetEmitter();
            emitter.Pos(p);

            FACTORY.color = color;
            FACTORY.dir = -3.1415926f / 2;
            FACTORY.cone = 3.1415926f;
            emitter.Burst(FACTORY, n);
        }

        public static void At(PointF p, float dir, float cone, Color color, int n)
        {
            if (n <= 0)
                return;

            var emitter = GameScene.GetEmitter();
            emitter.Pos(p);

            FACTORY.color = color;
            FACTORY.dir = dir;
            FACTORY.cone = cone;
            emitter.Burst(FACTORY, n);
        }

        private static SplashFactory FACTORY = new SplashFactory();

        private class SplashFactory : Emitter.Factory
        {
            public Color color;
            public float dir;
            public float cone;

            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                var p = emitter.Recycle<PixelParticle.Shrinking>();

                p.Reset(x, y, color, 4, Rnd.Float(0.5f, 1.0f));
                p.speed.Polar(Rnd.Float(dir - cone / 2, dir + cone / 2), Rnd.Float(40, 80));
                p.acc.Set(0, +100);
            }
        }
    }
}