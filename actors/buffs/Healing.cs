using System;
using watabou.utils;
using spdd.ui;
using spdd.sprites;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Healing : Buff
    {
        private int healingLeft;

        private float percentHealPerTick;
        private int flatHealPerTick;

        public Healing()
        {
            //unlike other buffs, this one acts after the hero and takes priority against other effects
            //healing is much more useful if you get some of it off before taking damage
            actPriority = HERO_PRIO - 1;

            type = BuffType.POSITIVE;
        }

        public override bool Act()
        {
            target.HP = Math.Min(target.HT, target.HP + HealingThisTick());

            healingLeft -= HealingThisTick();

            if (healingLeft <= 0)
            {
                Detach();
            }

            Spend(TICK);

            return true;
        }

        private int HealingThisTick()
        {
            return (int)GameMath.Gate(1,
                    (float)Math.Round(healingLeft * percentHealPerTick, MidpointRounding.AwayFromZero) + flatHealPerTick,
                    healingLeft);
        }

        public void SetHeal(int amount, float percentPerTick, int flatPerTick)
        {
            healingLeft = amount;
            percentHealPerTick = percentPerTick;
            flatHealPerTick = flatPerTick;
        }

        public void IncreaseHeal(int amount)
        {
            healingLeft += amount;
        }

        public override void Fx(bool on)
        {
            if (on)
                target.sprite.Add(CharSprite.State.HEALING);
            else
                target.sprite.Remove(CharSprite.State.HEALING);
        }

        private const string LEFT = "left";
        private const string PERCENT = "percent";
        private const string FLAT = "flat";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(LEFT, healingLeft);
            bundle.Put(PERCENT, percentHealPerTick);
            bundle.Put(FLAT, flatHealPerTick);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            healingLeft = bundle.GetInt(LEFT);
            percentHealPerTick = bundle.GetFloat(PERCENT);
            flatHealPerTick = bundle.GetInt(FLAT);
        }

        public override int Icon()
        {
            return BuffIndicator.HEALING;
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", HealingThisTick(), healingLeft);
        }
    }
}