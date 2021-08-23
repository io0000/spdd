using System;
using spdd.ui;
using spdd.items;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Degrade : FlavourBuff
    {
        public const float DURATION = 30f;

        public Degrade()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        public override bool AttachTo(Character target)
        {
            if (base.AttachTo(target))
            {
                Item.UpdateQuickslot();
                return true;
            }
            return false;
        }

        public override void Detach()
        {
            base.Detach();
            Item.UpdateQuickslot();
        }

        public static int ReduceLevel(int level)
        {
            if (level <= 0)
            {
                //zero or negative levels are unaffected
                return level;
            }
            else
            {
                //Otherwise returns the rounded result of sqrt(2*(lvl-1)) + 1
                // This means that levels 1/2/3/4/5/6/7/8/9/10/11/12/...
                // Are now instead:       1/2/3/3/4/4/4/5/5/ 5/ 5/ 6/...
                // Basically every level starting with 3 sticks around for 1 level longer than the last
                return (int)Math.Round(Math.Sqrt(2 * (level - 1)) + 1, MidpointRounding.AwayFromZero);
            }
        }

        public override int Icon()
        {
            return BuffIndicator.DEGRADE;
        }

        public override float IconFadePercent()
        {
            return (DURATION - Visualcooldown()) / DURATION;
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