using System;
using watabou.utils;
using spdd.ui;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class AdrenalineSurge : Buff
    {
        public const float DURATION = 800.0f;

        public AdrenalineSurge()
        {
            type = BuffType.POSITIVE;
        }

        private int boost;
        private float interval;

        public void Reset(int boost, float interval)
        {
            this.boost = boost;
            this.interval = interval;
            Spend(interval - Cooldown());
        }

        public int Boost()
        {
            return boost;
        }

        public override bool Act()
        {
            --boost;
            if (boost > 0)
            {
                Spend(interval);
            }
            else
            {
                Detach();
            }
            return true;
        }

        public override int Icon()
        {
            return BuffIndicator.FURY;
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
            return Messages.Get(this, "desc", boost, DispTurns(Visualcooldown()));
        }

        private const string BOOST = "boost";
        private const string INTERVAL = "interval";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(BOOST, boost);
            bundle.Put(INTERVAL, interval);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            boost = bundle.GetInt(BOOST);
            interval = bundle.GetFloat(INTERVAL);
        }
    }
}