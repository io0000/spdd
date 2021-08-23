using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.blobs;
using spdd.effects;
using spdd.scenes;

namespace spdd.levels.traps
{
    public class ChillingTrap : Trap
    {
        public ChillingTrap()
        {
            color = WHITE;
            shape = DOTS;
        }

        public override void Activate()
        {
            if (Dungeon.level.heroFOV[pos])
            {
                Splash.At(pos, new Color(0xB2, 0xD6, 0xFF, 0xFF), 5);
                Sample.Instance.Play(Assets.Sounds.SHATTER);
            }

            foreach (int i in PathFinder.NEIGHBORS9)
            {
                if (!Dungeon.level.solid[pos + i])
                {
                    GameScene.Add(Blob.Seed(pos + i, 10, typeof(Freezing)));
                }
            }
        }
    }
}