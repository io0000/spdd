using watabou.noosa.audio;
using spdd.actors.blobs;
using spdd.scenes;
using spdd.sprites;

namespace spdd.items.potions
{
    public class PotionOfToxicGas : Potion
    {
        public PotionOfToxicGas()
        {
            icon = ItemSpriteSheet.Icons.POTION_TOXICGAS;
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

            GameScene.Add(Blob.Seed(cell, 1000, typeof(ToxicGas)));
        }

        public override int Value()
        {
            return IsKnown() ? 30 * quantity : base.Value();
        }
    }
}