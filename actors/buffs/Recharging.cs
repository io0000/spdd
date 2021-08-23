using System;
using watabou.noosa;
using spdd.ui;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Recharging : FlavourBuff
    {
        public const float DURATION = 30f;

        public Recharging()
        {
            type = BuffType.POSITIVE;
        }

        public override int Icon()
        {
            return BuffIndicator.RECHARGING;
        }

        public override void TintIcon(Image icon)
        {
            icon.Hardlight(1, 1, 0);
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - Visualcooldown()) / DURATION);
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        //want to process partial turns for this buff, and not count it when it's expiring.
        //firstly, if this buff has half a turn left, should give out half the benefit.
        //secondly, recall that buffs execute in random order, so this can cause a problem where we can't simply check
        //if this buff is still attached, must instead directly check its remaining time, and act accordingly.
        //otherwise this causes inconsistent behaviour where this may detach before, or after, a wand charger acts.
        public float Remainder()
        {
            return Math.Min(1f, this.Cooldown());
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", DispTurns());
        }
    }
}