using watabou.noosa.audio;
using spdd.sprites;
using spdd.scenes;
using spdd.actors.blobs;

namespace spdd.items.potions.exotic
{
    public class PotionOfCorrosiveGas : ExoticPotion
    {
        public PotionOfCorrosiveGas()
        {
            icon = ItemSpriteSheet.Icons.POTION_CORROGAS;
        }

        public override void Shatter(int cell)
        {
            if (Dungeon.level.heroFOV[cell])
            {
                SetKnown();

                Splash(cell);
                Sample.Instance.Play(Assets.Sounds.SHATTER);
                Sample.Instance.Play(Assets.Sounds.GAS);
            }

            var gas = (CorrosiveGas)Blob.Seed(cell, 200, typeof(CorrosiveGas));
            gas.SetStrength(1 + Dungeon.depth / 5);
            GameScene.Add(gas);
        }
    }
}
