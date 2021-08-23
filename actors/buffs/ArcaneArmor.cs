using System;
using watabou.noosa;
using watabou.utils;
using spdd.ui;
using spdd.actors.hero;
using spdd.messages;

namespace spdd.actors.buffs
{
    //A magical version of barkskin, essentially
    public class ArcaneArmor : Buff
    {
        public ArcaneArmor()
        {
            type = BuffType.POSITIVE;
        }

        private int level;
        private int interval = 1;

        public override bool Act()
        {
            if (target.IsAlive())
            {
                Spend(interval);
                if (--level <= 0)
                {
                    Detach();
                }
            }
            else
            {
                Detach();
            }

            return true;
        }

        public int Level()
        {
            return level;
        }

        public void Set(int value, int time)
        {
            //decide whether to override, preferring high value + low interval
            if (Math.Sqrt(interval) * level < Math.Sqrt(time) * value)
            {
                level = value;
                interval = time;
                Spend(time - Cooldown() - 1.0f);
            }
        }

        public override int Icon()
        {
            return BuffIndicator.ARMOR;
        }

        public override void TintIcon(Image icon)
        {
            icon.Hardlight(1.0f, 0.5f, 2.0f);
        }

        public override float IconFadePercent()
        {
            if (target is Hero)
            {
                float max = ((Hero)target).lvl / 2 + 5;
                return (max - level) / max;
            }
            return 0;
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", level, DispTurns(Visualcooldown()));
        }

        private const string LEVEL = "level";
        private const string INTERVAL = "interval";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(INTERVAL, interval);
            bundle.Put(LEVEL, level);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            interval = bundle.GetInt(INTERVAL);
            level = bundle.GetInt(LEVEL);
        }
    }
}