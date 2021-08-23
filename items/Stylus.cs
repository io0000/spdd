using System.Collections.Generic;
using watabou.noosa.audio;
using spdd.actors.hero;
using spdd.effects;
using spdd.effects.particles;
using spdd.items.armor;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.utils;
using spdd.windows;

namespace spdd.items
{
    public class Stylus : Item
    {
        private const float TIME_TO_INSCRIBE = 2;

        private const string AC_INSCRIBE = "INSCRIBE";

        public Stylus()
        {
            image = ItemSpriteSheet.STYLUS;

            stackable = true;

            bones = true;
            itemSelector = new StylusListener(this);
        }

        public override List<string> Actions(Hero hero)
        {
            var actions = base.Actions(hero);

            actions.Add(AC_INSCRIBE);

            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_INSCRIBE))
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

        public void Inscribe(Armor armor)
        {
            if (!armor.IsIdentified())
            {
                GLog.Warning(Messages.Get(this, "identify"));
                return;
            }
            else if (armor.cursed || armor.HasCurseGlyph())
            {
                GLog.Warning(Messages.Get(this, "cursed"));
                return;
            }

            Detach(curUser.belongings.backpack);

            GLog.Warning(Messages.Get(this, "inscribed"));

            armor.Inscribe();

            curUser.sprite.Operate(curUser.pos);
            curUser.sprite.CenterEmitter().Start(PurpleParticle.Burst, 0.05f, 10);
            Enchanting.Show(curUser, armor);
            Sample.Instance.Play(Assets.Sounds.BURNING);

            curUser.Spend(TIME_TO_INSCRIBE);
            curUser.Busy();
        }

        public override int Value()
        {
            return 30 * quantity;
        }

        private readonly WndBag.IListener itemSelector;
    }

    class StylusListener : WndBag.IListener
    {
        private Stylus stylus;

        public StylusListener(Stylus stylus)
        {
            this.stylus = stylus;
        }

        public void OnSelect(Item item)
        {
            if (item != null)
                stylus.Inscribe((Armor)item);
        }
    }
}