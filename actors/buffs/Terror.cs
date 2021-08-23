using System;
using watabou.utils;
using spdd.ui;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Terror : FlavourBuff
    {
        public int obj;

        private const string OBJECT = "object";

        public const float DURATION = 20f;

        public Terror()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(OBJECT, obj);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            obj = bundle.GetInt(OBJECT);
        }

        public override int Icon()
        {
            return BuffIndicator.TERROR;
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

        public bool ignoreNextHit;

        public void Recover()
        {
            if (ignoreNextHit)
            {
                ignoreNextHit = false;
                return;
            }
            Spend(-5f);
            if (Cooldown() <= 0)
                Detach();
        }
    }
}