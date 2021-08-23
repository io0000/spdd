using System;
using watabou.noosa.audio;
using spdd.actors.blobs;
using spdd.scenes;
using spdd.sprites;

namespace spdd.items.potions.brews
{
    public class BlizzardBrew : Brew
    {
        public BlizzardBrew()
        {
            image = ItemSpriteSheet.BREW_BLIZZARD;
        }

        public override void Shatter(int cell)
        {
            if (Dungeon.level.heroFOV[cell])
            {
                Splash(cell);
                Sample.Instance.Play(Assets.Sounds.SHATTER);
                Sample.Instance.Play(Assets.Sounds.GAS);
            }

            GameScene.Add(Blob.Seed(cell, 1000, typeof(Blizzard)));
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (30 + 40);
        }

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(PotionOfFrost), typeof(AlchemicalCatalyst) };
                inQuantity = new int[] { 1, 1 };

                cost = 6;

                output = typeof(BlizzardBrew);
                outQuantity = 1;
            }
        }
    }
}
