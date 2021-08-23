using watabou.noosa.audio;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.scenes;
using spdd.windows;
using spdd.messages;

namespace spdd.items.spells
{
    public abstract class InventorySpell : Spell
    {
        protected string inventoryTitle;
        protected WndBag.Mode mode = WndBag.Mode.ALL;

        public InventorySpell()
        {
            inventoryTitle = Messages.Get(this, "inv_title");
        }

        protected override void OnCast(Hero hero)
        {
            curItem = Detach(hero.belongings.backpack);
            GameScene.SelectItem(itemSelector, mode, inventoryTitle);
        }

        protected abstract void OnItemSelected(Item item);

        protected static WndBag.IListener itemSelector = new ItemSelector();

        class ItemSelector : WndBag.IListener
        {
            public void OnSelect(Item item)
            {
                //FIXME this safety check shouldn't be necessary
                //it would be better to eliminate the curItem static variable.
                if (!(curItem is InventorySpell))
                    return;

                if (item != null)
                {
                    ((InventorySpell)curItem).OnItemSelected(item);
                    curUser.Spend(1f);
                    curUser.Busy();
                    (curUser.sprite).Operate(curUser.pos);

                    Sample.Instance.Play(Assets.Sounds.READ);
                    Invisibility.Dispel();
                }
                else
                {
                    curItem.Collect(curUser.belongings.backpack);
                }
            }
        }
    }
}
