using System;
using spdd.ui;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.utils;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class MagicalSleep : Buff
    {
        private const float STEP = 1.0f;

        public override bool AttachTo(Character target)
        {
            if (!target.IsImmune(typeof(Sleep)) && base.AttachTo(target))
            {
                ++target.paralysed;

                if (target.alignment == Character.Alignment.ALLY)
                {
                    if (target.HP == target.HT)
                    {
                        if (target is Hero)
                            GLog.Information(Messages.Get(this, "toohealthy"));
                        Detach();
                    }
                    else
                    {
                        if (target is Hero)
                            GLog.Information(Messages.Get(this, "fallasleep"));
                    }
                }

                if (target is Mob)
                    ((Mob)target).state = ((Mob)target).SLEEPING;

                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool Act()
        {
            if (target is Mob && ((Mob)target).state != ((Mob)target).SLEEPING)
            {
                Detach();
                return true;
            }

            if (target.alignment == Character.Alignment.ALLY)
            {
                target.HP = Math.Min(target.HP + 1, target.HT);
                if (target is Hero)
                    ((Hero)target).resting = true;

                if (target.HP == target.HT)
                {
                    if (target is Hero)
                        GLog.Positive(Messages.Get(this, "wakeup"));
                    Detach();
                }
            }
            Spend(STEP);
            return true;
        }

        public override void Detach()
        {
            if (target.paralysed > 0)
                --target.paralysed;

            if (target is Hero)
                ((Hero)target).resting = false;

            base.Detach();
        }

        public override int Icon()
        {
            return BuffIndicator.MAGIC_SLEEP;
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc");
        }
    }
}