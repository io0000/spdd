using watabou.noosa.audio;
using spdd.actors.blobs;
using spdd.scenes;

namespace spdd.levels.traps
{
    public class CorrosionTrap : Trap
    {
        public CorrosionTrap()
        {
            color = GREY;
            shape = GRILL;
        }

        public override void Activate()
        {
            CorrosiveGas corrosiveGas = (CorrosiveGas)Blob.Seed(pos, 80 + 5 * Dungeon.depth, typeof(CorrosiveGas));
            Sample.Instance.Play(Assets.Sounds.GAS);

            corrosiveGas.SetStrength(1 + Dungeon.depth / 4);

            GameScene.Add(corrosiveGas);
        }
    }
}