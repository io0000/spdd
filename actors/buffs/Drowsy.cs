using System;
using spdd.ui;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Drowsy : Buff
    {
        public const float DURATION = 5f;

        public Drowsy()
        {
            type = BuffType.NEUTRAL;
            announced = true;
        }

        public override int Icon()
        {
            return BuffIndicator.DROWSY;
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - Visualcooldown()) / DURATION);
        }

        public override bool AttachTo(Character target)
        {
            if (!target.IsImmune(typeof(Sleep)) && base.AttachTo(target))
            {
                if (Cooldown() == 0)
                    Spend(DURATION);

                return true;
            }
            return false;
        }

        public override bool Act()
        {
            Buff.Affect<MagicalSleep>(target);

            Detach();
            return true;
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", DispTurns(Visualcooldown()));
        }
    }
}