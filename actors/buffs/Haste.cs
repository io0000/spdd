using System;
using spdd.ui;
using watabou.noosa;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Haste : FlavourBuff
    {
        public Haste()
        {
            type = BuffType.POSITIVE;
        }

        public const float DURATION = 20f;

        public override int Icon()
        {
            return BuffIndicator.MOMENTUM;
        }

        public override void TintIcon(Image icon)
        {
            icon.Hardlight(1.0f, 0.8f, 0.0f);
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