using watabou.noosa.audio;
using spdd.scenes;
using spdd.actors.blobs;

namespace spdd.levels.traps
{
    public class ToxicTrap : Trap
    {
        public ToxicTrap()
        {
            color = GREEN;
            shape = GRILL;
        }

        public override void Activate()
        {
            GameScene.Add(Blob.Seed(pos, 300 + 20 * Dungeon.depth, typeof(ToxicGas)));
            Sample.Instance.Play(Assets.Sounds.GAS);
        }
    }
}