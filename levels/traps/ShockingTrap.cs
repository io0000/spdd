using watabou.utils;
using watabou.noosa.audio;
using spdd.scenes;
using spdd.actors.blobs;

namespace spdd.levels.traps
{
    public class ShockingTrap : Trap
    {
        public ShockingTrap()
        {
            color = YELLOW;
            shape = DOTS;
        }

        public override void Activate()
        {
            if (Dungeon.level.heroFOV[pos])
            {
                Sample.Instance.Play(Assets.Sounds.LIGHTNING);
            }

            foreach (int i in PathFinder.NEIGHBORS9)
            {
                if (!Dungeon.level.solid[pos + i])
                {
                    GameScene.Add(Blob.Seed(pos + i, 10, typeof(Electricity)));
                }
            }
        }
    }
}