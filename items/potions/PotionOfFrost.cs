using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.blobs;
using spdd.sprites;
using spdd.scenes;

namespace spdd.items.potions
{
    public class PotionOfFrost : Potion
    {
        public PotionOfFrost()
        {
            icon = ItemSpriteSheet.Icons.POTION_FROST;
        }

        public override void Shatter(int cell)
        {
            if (Dungeon.level.heroFOV[cell])
            {
                SetKnown();

                Splash(cell);
                Sample.Instance.Play(Assets.Sounds.SHATTER);
            }

            foreach (int offset in PathFinder.NEIGHBORS9)
            {
                if (!Dungeon.level.solid[cell + offset])
                {
                    GameScene.Add(Blob.Seed(cell + offset, 10, typeof(Freezing)));
                }
            }
        }

        public override int Value()
        {
            return IsKnown() ? 30 * quantity : base.Value();
        }
    }
}