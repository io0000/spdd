using System.Collections.Generic;
using watabou.noosa.audio;
using spdd.actors.hero;
using spdd.effects;
using spdd.items.armor;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.utils;
using spdd.windows;

namespace spdd.items
{
    public class ArmorKit : Item
    {
        private const float TIME_TO_UPGRADE = 2;

        private const string AC_APPLY = "APPLY";

        public ArmorKit()
        {
            image = ItemSpriteSheet.KIT;

            unique = true;

            itemSelector = new ArmorKitListener(this);
        }

        public override List<string> Actions(Hero hero)
        {
            var actions = base.Actions(hero);
            actions.Add(AC_APPLY);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action == AC_APPLY)
            {
                curUser = hero;
                GameScene.SelectItem(itemSelector, WndBag.Mode.ARMOR, Messages.Get(this, "prompt"));
            }
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public void Upgrade(Armor armor)
        {
            Detach(curUser.belongings.backpack);

            curUser.sprite.CenterEmitter().Start(Speck.Factory(Speck.KIT), 0.05f, 10);
            curUser.Spend(TIME_TO_UPGRADE);
            curUser.Busy();

            GLog.Warning(Messages.Get(this, "upgraded", armor.Name()));

            var classArmor = ClassArmor.Upgrade(curUser, armor);
            if (curUser.belongings.armor == armor)
            {
                curUser.belongings.armor = classArmor;
                ((HeroSprite)curUser.sprite).UpdateArmor();
                classArmor.Activate(curUser);
            }
            else
            {
                armor.Detach(curUser.belongings.backpack);
                classArmor.Collect(curUser.belongings.backpack);
            }

            curUser.sprite.Operate(curUser.pos);
            Sample.Instance.Play(Assets.Sounds.EVOKE);
        }

        private WndBag.IListener itemSelector;
    }

    class ArmorKitListener : WndBag.IListener
    {
        private ArmorKit armorKit;

        public ArmorKitListener(ArmorKit armorKit)
        {
            this.armorKit = armorKit;
        }

        public void OnSelect(Item item)
        {
            if (item != null)
                armorKit.Upgrade((Armor)item);
        }
    }
}