using spdd.sprites;
using spdd.actors.blobs;
using spdd.scenes;
using watabou.noosa.audio;

namespace spdd.items.potions.exotic
{
    public class PotionOfStormClouds : ExoticPotion
    {
        public PotionOfStormClouds()
        {
            icon = ItemSpriteSheet.Icons.POTION_STRMCLOUD;
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

            GameScene.Add(Blob.Seed(cell, 1000, typeof(StormCloud)));
        }
    }
}
