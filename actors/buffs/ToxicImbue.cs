using System;
using watabou.utils;
using spdd.ui;
using spdd.scenes;
using spdd.actors.blobs;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class ToxicImbue : Buff
    {
        public ToxicImbue()
        {
            type = BuffType.POSITIVE;
            announced = true;

            immunities.Add(typeof(ToxicGas));
            immunities.Add(typeof(Poison));
        }

        public const float DURATION = 100f;

        protected float left;

        private const string LEFT = "left";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(LEFT, left);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            left = bundle.GetFloat(LEFT);
        }

        public void Set(float duration)
        {
            this.left = duration;
        }

        public override bool Act()
        {
            GameScene.Add(Blob.Seed(target.pos, 50, typeof(ToxicGas)));

            Spend(TICK);
            left -= TICK;
            if (left <= 0)
                Detach();
        
            return true;
        }

        public override int Icon()
        {
            return BuffIndicator.IMMUNITY;
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - left) / DURATION);
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", DispTurns(left));
        }
    }
}