using System;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.food
{
    public class StewedMeat : Food
    {
        public StewedMeat()
        {
            image = ItemSpriteSheet.STEWED;
            energy = Hunger.HUNGRY / 2f;
        }

        public override int Value()
        {
            return 8 * quantity;
        }

        public class OneMeat : Recipe.SimpleRecipe
        {
            public OneMeat()
            {
                inputs = new Type[] { typeof(MysteryMeat) };
                inQuantity = new int[] { 1 };

                cost = 2;

                output = typeof(StewedMeat);
                outQuantity = 1;
            }
        }

        public class TwoMeat : Recipe.SimpleRecipe
        {
            public TwoMeat()
            {
                inputs = new Type[] { typeof(MysteryMeat) };
                inQuantity = new int[] { 2 };

                cost = 3;

                output = typeof(StewedMeat);
                outQuantity = 2;
            }
        }

        //red meat
        //blue meat

        public class ThreeMeat : Recipe.SimpleRecipe
        {
            public ThreeMeat()
            {
                inputs = new Type[] { typeof(MysteryMeat) };
                inQuantity = new int[] { 3 };

                cost = 4;

                output = typeof(StewedMeat);
                outQuantity = 3;
            }
        }
    }
}