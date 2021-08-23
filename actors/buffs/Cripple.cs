using System;
using spdd.ui;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Cripple : FlavourBuff
    {
        public const float DURATION = 10.0f;

        public Cripple()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        public override int Icon()
        {
            return BuffIndicator.CRIPPLE;
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - Visualcooldown()) / DURATION);
        }
        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string HeroMessage()
        {
            return Messages.Get(this, "heromsg");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", DispTurns());
        }
    }
}