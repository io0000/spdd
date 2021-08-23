using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors.hero;
using spdd.sprites;
using spdd.plants;
using spdd.items.potions.exotic;
using spdd.items.stones;

namespace spdd.items.potions
{
    public class AlchemicalCatalyst : Potion
    {
        public AlchemicalCatalyst()
        {
            image = ItemSpriteSheet.POTION_CATALYST;
        }

        private static Dictionary<Type, float> potionChances = new Dictionary<Type, float>();

        static AlchemicalCatalyst()
        {
            potionChances.Add(typeof(PotionOfHealing), 3f);
            potionChances.Add(typeof(PotionOfMindVision), 2f);
            potionChances.Add(typeof(PotionOfFrost), 2f);
            potionChances.Add(typeof(PotionOfLiquidFlame), 2f);
            potionChances.Add(typeof(PotionOfToxicGas), 2f);
            potionChances.Add(typeof(PotionOfHaste), 2f);
            potionChances.Add(typeof(PotionOfInvisibility), 2f);
            potionChances.Add(typeof(PotionOfLevitation), 2f);
            potionChances.Add(typeof(PotionOfParalyticGas), 2f);
            potionChances.Add(typeof(PotionOfPurity), 2f);
            potionChances.Add(typeof(PotionOfExperience), 1f);
        }

        public override void Apply(Hero hero)
        {
            Potion p = (Potion)Reflection.NewInstance(Rnd.Chances(potionChances));
            p.Anonymize();
            p.Apply(hero);
        }

        public override void Shatter(int cell)
        {
            Potion p = (Potion)Reflection.NewInstance(Rnd.Chances(potionChances));
            p.Anonymize();
            curItem = p;
            p.Shatter(cell);
        }

        public override bool IsKnown()
        {
            return true;
        }

        public override int Value()
        {
            return 40 * quantity;
        }

        public class Recipe : items.Recipe
        {
            public override bool TestIngredients(List<Item> ingredients)
            {
                bool potion = false;
                bool secondary = false;

                foreach (Item i in ingredients)
                {
                    if (i is Plant.Seed || i is Runestone)
                    {
                        secondary = true;
                    }
                    //if it is a regular or exotic potion
                    else if (ExoticPotion.regToExo.ContainsKey(i.GetType()) ||
                        ExoticPotion.regToExo.ContainsValue(i.GetType()))
                    {
                        potion = true;
                    }
                }

                return potion && secondary;
            }

            public override int Cost(List<Item> ingredients)
            {
                foreach (Item i in ingredients)
                {
                    if (i is Plant.Seed)
                        return 1;
                    else if (i is Runestone)
                        return 2;
                }
                return 1;
            }

            public override Item Brew(List<Item> ingredients)
            {
                foreach (Item i in ingredients)
                {
                    i.Quantity(i.Quantity() - 1);
                }

                return SampleOutput(null);
            }

            public override Item SampleOutput(List<Item> ingredients)
            {
                return new AlchemicalCatalyst();
            }
        }
    }
}