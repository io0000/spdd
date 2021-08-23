using System;
using watabou.noosa.audio;
using spdd.actors.blobs;
using spdd.sprites;
using spdd.scenes;

namespace spdd.items.potions.brews
{
    public class InfernalBrew : Brew
    {
        public InfernalBrew()
        {
            image = ItemSpriteSheet.BREW_INFERNAL;
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

            GameScene.Add(Blob.Seed(cell, 1000, typeof(Inferno)));
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (30 + 40);
        }

        public class Recipe : spdd.items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(PotionOfLiquidFlame), typeof(AlchemicalCatalyst) };
                inQuantity = new int[] { 1, 1 };

                cost = 6;

                output = typeof(InfernalBrew);
                outQuantity = 1;
            }
        }
    }
}
