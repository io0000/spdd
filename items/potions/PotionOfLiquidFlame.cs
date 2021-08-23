using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.blobs;
using spdd.scenes;
using spdd.sprites;

namespace spdd.items.potions
{
    public class PotionOfLiquidFlame : Potion
    {
        public PotionOfLiquidFlame()
        {
            icon = ItemSpriteSheet.Icons.POTION_LIQFLAME;
        }

        public override void Shatter(int cell)
        {
            if (Dungeon.level.heroFOV[cell])
            {
                SetKnown();

                Splash(cell);
                Sample.Instance.Play(Assets.Sounds.SHATTER);
                Sample.Instance.Play(Assets.Sounds.BURNING);
            }

            foreach (int offset in PathFinder.NEIGHBORS9)
            {
                if (!Dungeon.level.solid[cell + offset])
                {
                    var fire = Blob.Seed(cell + offset, 2, typeof(Fire));
                    GameScene.Add(fire);
                }
            }
        }

        public override int Value()
        {
            return IsKnown() ? 30 * quantity : base.Value();
        }
    }
}