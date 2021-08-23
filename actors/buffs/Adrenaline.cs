using System;
using spdd.ui;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Adrenaline : FlavourBuff
    {
        public Adrenaline()
        {
            type = BuffType.POSITIVE;

            announced = true;
        }

        public const float DURATION = 10.0f;

        public override int Icon()
        {
            return BuffIndicator.AMOK;
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