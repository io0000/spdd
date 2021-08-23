using spdd.sprites;
using spdd.actors.blobs;
using spdd.scenes;
using watabou.noosa.audio;

namespace spdd.items.potions.exotic
{
    public class PotionOfShroudingFog : ExoticPotion
    {
        public PotionOfShroudingFog()
        {
            icon = ItemSpriteSheet.Icons.POTION_SHROUDFOG;
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

            GameScene.Add(Blob.Seed(cell, 1000, typeof(SmokeScreen)));
        }
    }
}
