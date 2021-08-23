using System.Collections.Generic;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.effects.particles;
using spdd.messages;
using spdd.utils;

namespace spdd.items
{
    public abstract class EquipableItem : Item
    {
        public const string AC_EQUIP = "EQUIP";
        public const string AC_UNEQUIP = "UNEQUIP";

        public EquipableItem()
        {
            bones = true;
        }

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            actions.Add(IsEquipped(hero) ? AC_UNEQUIP : AC_EQUIP);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_EQUIP))
            {
                //In addition to equipping itself, item reassigns itself to the quickslot
                //This is a special case as the item is being removed from inventory, but is staying with the hero.
                int slot = Dungeon.quickslot.GetSlot(this);
                DoEquip(hero);
                if (slot != -1)
                {
                    Dungeon.quickslot.SetSlot(slot, this);
                    UpdateQuickslot();
                }
            }
            else if (action.Equals(AC_UNEQUIP))
            {
                DoUnequip(hero, true);
            }
        }

        public override void DoDrop(Hero hero)
        {
            if (!IsEquipped(hero) || DoUnequip(hero, false, false))
                base.DoDrop(hero);
        }

        public override void Cast(Hero user, int dst)
        {
            if (IsEquipped(user))
            {
                if (quantity == 1 && !DoUnequip(user, false, false))
                    return;
            }

            base.Cast(user, dst);
        }

        public static void EquipCursed(Hero hero)
        {
            hero.sprite.Emitter().Burst(ShadowParticle.Curse, 6);
            Sample.Instance.Play(Assets.Sounds.CURSED);
        }

        protected virtual float Time2Equip(Hero hero)
        {
            return 1;
        }

        public abstract bool DoEquip(Hero hero);

        public virtual bool DoUnequip(Hero hero, bool collect, bool single)
        {
            if (cursed && hero.FindBuff<MagicImmune>() == null)
            {
                GLog.Warning(Messages.Get(typeof(EquipableItem), "unequip_cursed"));
                return false;
            }

            if (single)
                hero.SpendAndNext(Time2Equip(hero));
            else
                hero.Spend(Time2Equip(hero));

            if (!collect || !Collect(hero.belongings.backpack))
            {
                OnDetach();
                Dungeon.quickslot.ClearItem(this);
                UpdateQuickslot();
                if (collect)
                    Dungeon.level.Drop(this, hero.pos);
            }

            return true;
        }

        public bool DoUnequip(Hero hero, bool collect)
        {
            return DoUnequip(hero, collect, true);
        }

        public virtual void Activate(Character ch)
        { }
    }
}