using System;
using spdd.ui;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Roots : FlavourBuff
    {
        public const float DURATION = 5.0f;

        public Roots()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        public override bool AttachTo(Character target)
        {
            if (!target.flying && base.AttachTo(target))
            {
                target.rooted = true;
                return true;
            }

            return false;
        }

        public override void Detach()
        {
            target.rooted = false;
            base.Detach();
        }

        public override int Icon()
        {
            return BuffIndicator.ROOTS;
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