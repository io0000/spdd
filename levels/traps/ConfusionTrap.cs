using watabou.noosa.audio;
using spdd.actors.blobs;
using spdd.scenes;

namespace spdd.levels.traps
{
    public class ConfusionTrap : Trap
    {
        public ConfusionTrap()
        {
            color = TEAL;
            shape = GRILL;
        }

        public override void Activate()
        {
            GameScene.Add(Blob.Seed(pos, 300 + 20 * Dungeon.depth, typeof(ConfusionGas)));
            Sample.Instance.Play(Assets.Sounds.GAS);
        }
    }
}