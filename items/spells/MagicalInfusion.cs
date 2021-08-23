using System;
using spdd.sprites;
using spdd.effects;
using spdd.utils;
using spdd.items.scrolls;
using spdd.items.weapon;
using spdd.items.armor;
using spdd.windows;
using spdd.messages;

namespace spdd.items.spells
{
    public class MagicalInfusion : InventorySpell
    {
        public MagicalInfusion()
        {
            mode = WndBag.Mode.UPGRADEABLE;
            image = ItemSpriteSheet.MAGIC_INFUSE;

            unique = true;
        }

        protected override void OnItemSelected(Item item)
        {
            if (item is Weapon && ((Weapon)item).enchantment != null && !((Weapon)item).HasCurseEnchant())
            {
                ((Weapon)item).Upgrade(true);
            }
            else if (item is Armor && ((Armor)item).glyph != null && !((Armor)item).HasCurseGlyph())
            {
                ((Armor)item).Upgrade(true);
            }
            else
            {
                item.Upgrade();
            }

            GLog.Positive(Messages.Get(this, "infuse", item.Name()));

            BadgesExtensions.ValidateItemLevelAquired(item);

            curUser.sprite.Emitter().Start(Speck.Factory(Speck.UP), 0.2f, 3);
            ++Statistics.upgradesUsed;
        }

        public override int Value()
        {
            //prices of ingredients, divided by output quantity
            return (int)Math.Round(quantity * ((50 + 40) / 1f), MidpointRounding.AwayFromZero);
        }

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(ScrollOfUpgrade), typeof(ArcaneCatalyst) };
                inQuantity = new int[] { 1, 1 };

                cost = 4;

                output = typeof(MagicalInfusion);
                outQuantity = 1;
            }
        }
    }
}