using System;
using watabou.utils;
using spdd.ui;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class WellFed : Buff
    {
        public WellFed()
        {
            type = BuffType.POSITIVE;
            announced = true;
        }

        int left;

        public override bool Act()
        {
            --left;
            if (left < 0)
            {
                Detach();
                return true;
            }
            else if (left % 18 == 0)
            {
                target.HP = Math.Min(target.HT, target.HP + 1);
            }

            Spend(TICK);
            return true;
        }

        public void Reset()
        {
            //heals one HP every 18 turns for 450 turns
            //25 HP healed in total
            left = (int)Hunger.STARVING;
        }

        public override int Icon()
        {
            return BuffIndicator.WELL_FED;
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (Hunger.STARVING - left) / Hunger.STARVING);
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }
        public override string Desc()
        {
            return Messages.Get(this, "desc", left + 1);
        }

        private const string LEFT = "left";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(LEFT, left);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            left = bundle.GetInt(LEFT);
        }
    }
}