using watabou.utils;
using spdd.actors;
using spdd.actors.blobs;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.effects;
using spdd.effects.particles;
using spdd.scenes;
using spdd.sprites;

namespace spdd.plants
{
    public class Firebloom : Plant
    {
        public Firebloom()
        {
            image = 1;
            seedClass = typeof(Seed);
        }

        public override void Activate(Character ch)
        {
            if (ch is Hero && ((Hero)ch).subClass == HeroSubClass.WARDEN)
            {
                Buff.Affect<FireImbue>(ch).Set(FireImbue.DURATION * 0.3f);
            }

            GameScene.Add(Blob.Seed(pos, 2, typeof(Fire)));

            if (Dungeon.level.heroFOV[pos])
            {
                CellEmitter.Get(pos).Burst(FlameParticle.Factory, 5);
            }
        }

        [SPDStatic]
        public new class Seed : Plant.Seed
        {
            public Seed()
            {
                image = ItemSpriteSheet.SEED_FIREBLOOM;

                plantClass = typeof(Firebloom);
            }
        }
    }
}