using System;
using watabou.utils;
using spdd.ui;
using spdd.actors.hero;
using spdd.items.artifacts;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class ArtifactRecharge : Buff
    {
        public const float DURATION = 30f;

        public ArtifactRecharge()
        {
            type = BuffType.POSITIVE;
        }

        private int left;

        public override bool Act()
        {
            if (target is Hero)
            {
                Belongings b = ((Hero)target).belongings;

                if (b.artifact is Artifact)
                {
                    ((Artifact)b.artifact).Charge((Hero)target);
                }
                
                if (b.misc is Artifact)
                {
                    ((Artifact)b.misc).Charge((Hero)target);
                }
            }

            --left;
            if (left <= 0)
            {
                Detach();
            }
            else
            {
                Spend(TICK);
            }
            return true;
        }

        public void Set(int amount)
        {
            left = amount;
        }

        public void Prolong(int amount)
        {
            left += amount;
        }

        public override int Icon()
        {
            return BuffIndicator.RECHARGING;
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
            return Messages.Get(this, "desc", DispTurns(left + 1));
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