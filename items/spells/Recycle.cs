using System;
using watabou.utils;
using spdd.items.potions;
using spdd.items.potions.exotic;
using spdd.items.potions.elixirs;
using spdd.items.potions.brews;
using spdd.items.stones;
using spdd.items.scrolls;
using spdd.items.scrolls.exotic;
using spdd.plants;
using spdd.sprites;
using spdd.windows;
using spdd.utils;
using spdd.messages;

namespace spdd.items.spells
{
    public class Recycle : InventorySpell
    {
        public Recycle()
        {
            image = ItemSpriteSheet.RECYCLE;
            mode = WndBag.Mode.RECYCLABLE;
        }

        protected override void OnItemSelected(Item item)
        {
            Item result;
            do
            {
                if (item is Potion)
                {
                    result = Generator.Random(Generator.Category.POTION);
                    if (item is ExoticPotion)
                        result = (Potion)Reflection.NewInstance(ExoticPotion.regToExo.Get(result.GetType()));
                }
                else if (item is Scroll)
                {
                    result = Generator.Random(Generator.Category.SCROLL);
                    if (item is ExoticScroll)
                        result = (Scroll)Reflection.NewInstance(ExoticScroll.regToExo.Get(result.GetType()));
                }
                else if (item is Plant.Seed)
                {
                    result = Generator.Random(Generator.Category.SEED);
                }
                else
                {
                    result = Generator.Random(Generator.Category.STONE);
                }
            }
            while (result.GetType() == item.GetType() || Challenges.IsItemBlocked(result));

            item.Detach(curUser.belongings.backpack);
            GLog.Positive(Messages.Get(this, "recycled", result.Name()));
            if (!result.Collect())
                Dungeon.level.Drop(result, curUser.pos).sprite.Drop();

            //TODO visuals
        }

        public static bool IsRecyclable(Item item)
        {
            return (item is Potion && !(item is Elixir || item is Brew)) ||
                    item is Scroll ||
                    item is Plant.Seed ||
                    item is Runestone;
        }

        public override int Value()
        {
            //prices of ingredients, divided by output quantity
            return (int)Math.Round(quantity * ((50 + 40) / 8f), MidpointRounding.AwayFromZero);
        }

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(ScrollOfTransmutation), typeof(ArcaneCatalyst) };
                inQuantity = new int[] { 1, 1 };

                cost = 6;

                output = typeof(Recycle);
                outQuantity = 8;
            }
        }
    }
}