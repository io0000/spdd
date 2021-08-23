using System;
using watabou.noosa;
using spdd.ui;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Stamina : FlavourBuff
    {
        public const float DURATION = 100f;

        public Stamina()
        {
            type = BuffType.POSITIVE;
        }

        public override int Icon()
        {
            return BuffIndicator.MOMENTUM;
        }

        public override void TintIcon(Image icon)
        {
            icon.Hardlight(0.5f, 1f, 0.5f);
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
            return Messages.Get(this, "desc", DispTurns());
        }
    }
}