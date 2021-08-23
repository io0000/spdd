using System.Collections.Generic;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.sprites;

namespace spdd.items.food
{
    public class MeatPie : Food
    {
        public MeatPie()
        {
            image = ItemSpriteSheet.MEAT_PIE;
            energy = Hunger.HUNGRY * 2f;
        }

        protected override void Satisfy(Hero hero)
        {
            base.Satisfy(hero);
            Buff.Affect<WellFed>(hero).Reset();
        }

        public override int Value()
        {
            return 40 * quantity;
        }

        // 	public static class Recipe extends com.shatteredpixel.shatteredpixeldungeon.items.Recipe
        public class Recipe : spdd.items.Recipe
        {
            public override bool TestIngredients(List<Item> ingredients)
            {
                bool pasty = false;
                bool ration = false;
                bool meat = false;

                foreach (Item ingredient in ingredients)
                {
                    if (ingredient.Quantity() > 0)
                    {
                        if (ingredient is Pasty)
                        {
                            pasty = true;
                        }
                        else if (ingredient.GetType() == typeof(Food))
                        {
                            ration = true;
                        }
                        else if (ingredient is MysteryMeat ||
                              ingredient is StewedMeat ||
                              ingredient is ChargrilledMeat ||
                              ingredient is FrozenCarpaccio)
                        {
                            meat = true;
                        }
                    }
                }

                return pasty && ration && meat;
            }

            public override int Cost(List<Item> ingredients)
            {
                return 6;
            }

            public override Item Brew(List<Item> ingredients)
            {
                if (!TestIngredients(ingredients))
                    return null;

                foreach (Item ingredient in ingredients)
                {
                    ingredient.Quantity(ingredient.Quantity() - 1);
                }

                return SampleOutput(null);
            }

            public override Item SampleOutput(List<Item> ingredients)
            {
                return new MeatPie();
            }
        }
    }
}