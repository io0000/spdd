using System;
using watabou.utils;
using spdd.ui;
using spdd.utils;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Ooze : Buff
    {
        public const float DURATION = 20f;

        public Ooze()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        private float left;
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

        public override int Icon()
        {
            return BuffIndicator.OOZE;
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - left) / DURATION);
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
            return Messages.Get(this, "desc", DispTurns(left));
        }

        public void Set(float left)
        {
            this.left = left;
        }

        public override bool Act()
        {
            if (target.IsAlive())
            {
                if (Dungeon.depth > 4)
                    target.Damage(Dungeon.depth / 5, this);
                else if (Rnd.Int(2) == 0)
                    target.Damage(1, this);

                if (!target.IsAlive() && target == Dungeon.hero)
                {
                    Dungeon.Fail(GetType());
                    GLog.Negative(Messages.Get(this, "ondeath"));
                }
                Spend(TICK);

                left -= TICK;
                if (left <= 0)
                    Detach();
            }
            else
            {
                Detach();
            }

            if (Dungeon.level.water[target.pos])
                Detach();

            return true;
        }
    }
}