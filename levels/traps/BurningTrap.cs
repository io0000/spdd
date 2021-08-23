using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.blobs;
using spdd.effects;
using spdd.effects.particles;
using spdd.scenes;

namespace spdd.levels.traps
{
    public class BurningTrap : Trap
    {
        public BurningTrap()
        {
            color = ORANGE;
            shape = DOTS;
        }

        public override void Activate()
        {
            foreach (int i in PathFinder.NEIGHBORS9)
            {
                if (!Dungeon.level.solid[pos + i])
                {
                    GameScene.Add(Blob.Seed(pos + i, 2, typeof(Fire)));
                    CellEmitter.Get(pos + i).Burst(FlameParticle.Factory, 5);
                }
            }
            Sample.Instance.Play(Assets.Sounds.BURNING);
        }
    }
}