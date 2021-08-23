using spdd.effects;
using spdd.sprites;
using spdd.utils;
using spdd.windows;
using spdd.items.weapon;
using spdd.items.armor;
using spdd.messages;

namespace spdd.items.stones
{
    public class StoneOfEnchantment : InventoryStone
    {
        public StoneOfEnchantment()
        {
            mode = WndBag.Mode.ENCHANTABLE;
            image = ItemSpriteSheet.STONE_ENCHANT;

            unique = true;
        }

        protected override void OnItemSelected(Item item)
        {
            if (item is Weapon)
            {
                ((Weapon)item).Enchant();
            }
            else
            {
                ((Armor)item).Inscribe();
            }

            curUser.sprite.Emitter().Start(Speck.Factory(Speck.LIGHT), 0.1f, 5);
            Enchanting.Show(curUser, item);

            if (item is Weapon)
            {
                GLog.Positive(Messages.Get(this, "weapon"));
            }
            else
            {
                GLog.Positive(Messages.Get(this, "armor"));
            }

            UseAnimation();
        }

        public override int Value()
        {
            return 30 * quantity;
        }
    }
}