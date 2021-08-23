using System;
using spdd.ui;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Hex : FlavourBuff
    {
        public const float DURATION = 30f;

        public Hex()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        public override int Icon()
        {
            return BuffIndicator.HEX;
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