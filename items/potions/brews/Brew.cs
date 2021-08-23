using System.Collections.Generic;
using spdd.actors.hero;
using spdd.scenes;

namespace spdd.items.potions.brews
{
    public class Brew : Potion
    {
        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            actions.Remove(AC_DRINK);
            return actions;
        }

        public override void SetAction()
        {
            defaultAction = AC_THROW;
        }

        public override void DoThrow(Hero hero)
        {
            GameScene.SelectCell(thrower);
        }

        //public override void Shatter(int cell);

        public override bool IsKnown()
        {
            return true;
        }
    }
}
