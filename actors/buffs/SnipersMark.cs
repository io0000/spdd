using System;
using watabou.noosa;
using watabou.utils;
using spdd.actors.hero;
using spdd.items.weapon;
using spdd.messages;
using spdd.sprites;
using spdd.ui;

namespace spdd.actors.buffs
{
    public class SnipersMark : FlavourBuff, ActionIndicator.IAction
    {
        public int obj;

        private const string OBJECT = "object";

        public const float DURATION = 4f;

        public SnipersMark()
        {
            type = BuffType.POSITIVE;
        }

        public override bool AttachTo(Character target)
        {
            ActionIndicator.SetAction(this);
            return base.AttachTo(target);
        }

        public override void Detach()
        {
            base.Detach();
            ActionIndicator.ClearAction(this);
        }

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(OBJECT, obj);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            obj = bundle.GetInt(OBJECT);
        }

        public override int Icon()
        {
            return BuffIndicator.MARK;
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - Visualcooldown()) / DURATION);
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc");
        }

        // ActionIndicator.IAction
        public Image GetIcon()
        {
            return new ItemSprite(ItemSpriteSheet.SPIRIT_BOW, null);
        }

        // ActionIndicator.IAction
        public void DoAction()
        {
            Hero hero = Dungeon.hero;
            if (hero == null)
                return;

            SpiritBow bow = hero.belongings.GetItem<SpiritBow>();
            if (bow == null)
                return;

            SpiritBow.SpiritArrow arrow = bow.KnockArrow();
            if (arrow == null)
                return;

            var ch = (Character)Actor.FindById(obj);
            if (ch == null)
                return;

            int cell = QuickSlotButton.AutoAim(ch, arrow);
            if (cell == -1)
                return;

            bow.sniperSpecial = true;

            arrow.Cast(hero, cell);
            Detach();
        }
    }
}