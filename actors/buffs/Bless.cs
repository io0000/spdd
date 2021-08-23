using System;
using spdd.ui;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Bless : FlavourBuff
    {
        public const float DURATION = 30f;

        public Bless()
        {
            type = BuffType.POSITIVE;
            announced = true;
        }

        public override int Icon()
        {
            return BuffIndicator.BLESS;
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