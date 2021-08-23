using System;
using System.Collections.Generic;
using spdd.actors.hero;
using spdd.sprites;
using spdd.utils;
using spdd.windows;
using spdd.messages;


namespace spdd.items.spells
{
    public class MagicalPorter : InventorySpell
    {
        public MagicalPorter()
        {
            image = ItemSpriteSheet.MAGIC_PORTER;
            mode = WndBag.Mode.NOT_EQUIPPED;
        }

        protected override void OnCast(Hero hero)
        {
            if (Dungeon.depth >= 25)
            {
                GLog.Warning(Messages.Get(this, "nowhere"));
            }
            else
            {
                base.OnCast(hero);
            }
        }

        protected override void OnItemSelected(Item item)
        {
            Item result = item.DetachAll(curUser.belongings.backpack);
            int portDepth = 5 * (1 + Dungeon.depth / 5);
            var ported = Dungeon.portedItems[portDepth];
            if (ported == null)
            {
                ported = new List<Item>();
                Dungeon.portedItems.Add(portDepth, ported);
            }
            ported.Add(result);
        }

        public override int Value()
        {
            //prices of ingredients, divided by output quantity
            return (int)Math.Round(quantity * ((5 + 40) / 8f), MidpointRounding.AwayFromZero);
        }

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(MerchantsBeacon), typeof(ArcaneCatalyst) };
                inQuantity = new int[] { 1, 1 };

                cost = 4;

                output = typeof(MagicalPorter);
                outQuantity = 8;
            }
        }
    }
}