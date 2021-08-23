using System;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.blobs;
using spdd.sprites;
using spdd.scenes;
using spdd.utils;

namespace spdd.items.potions.brews
{
    public class ShockingBrew : Brew
    {
        public ShockingBrew()
        {
            image = ItemSpriteSheet.BREW_SHOCKING;
        }

        public override void Shatter(int cell)
        {
            if (Dungeon.level.heroFOV[cell])
            {
                Splash(cell);
                Sample.Instance.Play(Assets.Sounds.SHATTER);
                Sample.Instance.Play(Assets.Sounds.LIGHTNING);
            }

            PathFinder.BuildDistanceMap(cell, BArray.Not(Dungeon.level.solid, null), 2);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue)
                {
                    GameScene.Add(Blob.Seed(i, 20, typeof(Electricity)));
                }
            }
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (40 + 40);
        }

        public class Recipe : spdd.items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(PotionOfParalyticGas), typeof(AlchemicalCatalyst) };
                inQuantity = new int[] { 1, 1 };

                cost = 8;

                output = typeof(ShockingBrew);
                outQuantity = 1;
            }
        }
    }
}