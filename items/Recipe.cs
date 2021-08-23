using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.items.artifacts;
using spdd.items.bombs;
using spdd.items.food;
using spdd.items.potions;
using spdd.items.potions.brews;
using spdd.items.potions.elixirs;
using spdd.items.potions.exotic;
using spdd.items.scrolls;
using spdd.items.scrolls.exotic;
using spdd.items.spells;
using spdd.items.wands;

namespace spdd.items
{
    public abstract class Recipe
    {
        public abstract bool TestIngredients(List<Item> ingredients);

        public abstract int Cost(List<Item> ingredients);

        public abstract Item Brew(List<Item> ingredients);

        public abstract Item SampleOutput(List<Item> ingredients);

        //subclass for the common situation of a recipe with static inputs and outputs
        public abstract class SimpleRecipe : Recipe
        {
            //*** These elements must be filled in by subclasses
            protected Type[] inputs; //each class should be unique
            protected int[] inQuantity;

            protected int cost;

            protected Type output;
            protected int outQuantity;
            //***

            //gets a simple list of items based on inputs
            public List<Item> GetIngredients()
            {
                var result = new List<Item>();
                for (int i = 0; i < inputs.Length; ++i)
                {
                    var ingredient = (Item)Reflection.NewInstance(inputs[i]);
                    ingredient.Quantity(inQuantity[i]);
                    result.Add(ingredient);
                }
                return result;
            }

            public override bool TestIngredients(List<Item> ingredients)
            {
                int[] needed = (int[])inQuantity.Clone();

                foreach (var ingredient in ingredients)
                {
                    if (!ingredient.IsIdentified())
                        return false;
                    for (int i = 0; i < inputs.Length; ++i)
                    {
                        if (ingredient.GetType() == inputs[i])
                        {
                            needed[i] -= ingredient.Quantity();
                            break;
                        }
                    }
                }

                foreach (int i in needed)
                {
                    if (i > 0)
                        return false;
                }

                return true;
            }

            public override int Cost(List<Item> ingredients)
            {
                return cost;
            }

            public override Item Brew(List<Item> ingredients)
            {
                if (!TestIngredients(ingredients))
                    return null;

                int[] needed = (int[])inQuantity.Clone();

                foreach (Item ingredient in ingredients)
                {
                    for (int i = 0; i < inputs.Length; ++i)
                    {
                        if (ingredient.GetType() == inputs[i] && needed[i] > 0)
                        {
                            if (needed[i] <= ingredient.Quantity())
                            {
                                ingredient.Quantity(ingredient.Quantity() - needed[i]);
                                needed[i] = 0;
                            }
                            else
                            {
                                needed[i] -= ingredient.Quantity();
                                ingredient.Quantity(0);
                            }
                        }
                    }
                }

                //sample output and real output are identical in this case.
                return SampleOutput(null);
            }

            //ingredients are ignored, as output doesn't vary
            public override Item SampleOutput(List<Item> ingredients)
            {
                try
                {
                    Item result = (Item)Reflection.NewInstance(output);
                    result.Quantity(outQuantity);
                    return result;
                }
                catch (Exception e)
                {
                    ShatteredPixelDungeonDash.ReportException(e);
                    return null;
                }
            }
        }

        //*******
        // Static members
        //*******

        private static Recipe[] oneIngredientRecipes = new Recipe[]{
            new AlchemistsToolkit.UpgradeKit(),
            new Scroll.ScrollToStone(),
            new StewedMeat.OneMeat()
        };

        private static Recipe[] twoIngredientRecipes = new Recipe[]{
            new Blandfruit.CookFruit(),
            new Bomb.EnhanceBomb(),
            new AlchemicalCatalyst.Recipe(),
            new ArcaneCatalyst.Recipe(),
            new ElixirOfArcaneArmor.Recipe(),
            new ElixirOfAquaticRejuvenation.Recipe(),
            new ElixirOfDragonsBlood.Recipe(),
            new ElixirOfIcyTouch.Recipe(),
            new ElixirOfMight.Recipe(),
            new ElixirOfHoneyedHealing.Recipe(),
            new ElixirOfToxicEssence.Recipe(),
            new BlizzardBrew.Recipe(),
            new InfernalBrew.Recipe(),
            new ShockingBrew.Recipe(),
            new CausticBrew.Recipe(),
            new Alchemize.Recipe(),
            new AquaBlast.Recipe(),
            new BeaconOfReturning.Recipe(),
            new CurseInfusion.Recipe(),
            new FeatherFall.Recipe(),
            new MagicalInfusion.Recipe(),
            new MagicalPorter.Recipe(),
            new PhaseShift.Recipe(),
            new ReclaimTrap.Recipe(),
            new Recycle.Recipe(),
            new WildEnergy.Recipe(),
            new StewedMeat.TwoMeat()
        };

        private static Recipe[] threeIngredientRecipes = new Recipe[]{
            new Potion.SeedToPotion(),
            new ExoticPotion.PotionToExotic(),
            new ExoticScroll.ScrollToExotic(),
            new StewedMeat.ThreeMeat(),
            new MeatPie.Recipe()
        };

        public static Recipe FindRecipe(List<Item> ingredients)
        {
            if (ingredients.Count == 1)
            {
                foreach (Recipe recipe in oneIngredientRecipes)
                {
                    if (recipe.TestIngredients(ingredients))
                    {
                        return recipe;
                    }
                }
            }
            else if (ingredients.Count == 2)
            {
                foreach (Recipe recipe in twoIngredientRecipes)
                {
                    if (recipe.TestIngredients(ingredients))
                    {
                        return recipe;
                    }
                }
            }
            else if (ingredients.Count == 3)
            {
                foreach (Recipe recipe in threeIngredientRecipes)
                {
                    if (recipe.TestIngredients(ingredients))
                    {
                        return recipe;
                    }
                }
            }

            return null;
        }

        public static bool UsableInRecipe(Item item)
        {
            return !item.cursed &&
                (!(item is EquipableItem) || (item is AlchemistsToolkit && item.IsIdentified())) &&
                !(item is Wand);
        }
    }
}