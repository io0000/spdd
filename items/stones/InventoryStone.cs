using System.Collections.Generic;
using watabou.noosa.audio;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.scenes;
using spdd.windows;
using spdd.messages;

namespace spdd.items.stones
{
    public abstract class InventoryStone : Runestone
    {
        protected string inventoryTitle;
        protected WndBag.Mode mode = WndBag.Mode.ALL;

        public InventoryStone()
        {
            defaultAction = AC_USE;
            inventoryTitle = Messages.Get(this, "inv_title");
        }

        public const string AC_USE = "USE";

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            actions.Add(AC_USE);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);
            if (action.Equals(AC_USE))
            {
                curItem = Detach(hero.belongings.backpack);
                Activate(curUser.pos);
            }
        }

        protected override void Activate(int cell)
        {
            GameScene.SelectItem(itemSelector, mode, inventoryTitle);
        }

        protected void UseAnimation()
        {
            curUser.Spend(1f);
            curUser.Busy();
            curUser.sprite.Operate(curUser.pos);

            Sample.Instance.Play(Assets.Sounds.READ);
            Invisibility.Dispel();
        }

        protected abstract void OnItemSelected(Item item);

        protected static WndBag.IListener itemSelector = new ItemSelector();

        private class ItemSelector : WndBag.IListener
        {
            public void OnSelect(Item item)
            {
                //FIXME this safety check shouldn't be necessary
                //it would be better to eliminate the curItem static variable.
                if (!(curItem is InventoryStone))
                {
                    return;
                }

                if (item != null)
                {
                    ((InventoryStone)curItem).OnItemSelected(item);
                }
                else
                {
                    curItem.Collect(curUser.belongings.backpack);
                }
            }
        }
    }
}

