using spdd.actors.hero;
using spdd.items.rings;
using spdd.items.artifacts;

namespace spdd.actors.buffs
{
    public class Regeneration : Buff
    {
        public Regeneration()
        {
            //unlike other buffs, this one acts after the hero and takes priority against other effects
            //healing is much more useful if you get some of it off before taking damage
            actPriority = HERO_PRIO - 1;
        }

        private const float REGENERATION_DELAY = 10.0f;

        public override bool Act()
        {
            if (target.IsAlive())
            {
                if (target.HP < Regencap() && !((Hero)target).IsStarving())
                {
                    LockedFloor locked = target.FindBuff<LockedFloor>();
                    if (target.HP > 0 && (locked == null || locked.RegenOn()))
                    {
                        target.HP += 1;
                        if (target.HP == Regencap())
                        {
                            ((Hero)target).resting = false;
                        }
                    }
                }

                var regenBuff = Dungeon.hero.FindBuff<ChaliceOfBlood.ChaliceRegen>();

                float delay = REGENERATION_DELAY;
                if (regenBuff != null)
                {
                    if (regenBuff.IsCursed())
                    {
                        delay *= 1.5f;
                    }
                    else
                    {
                        delay -= regenBuff.ItemLevel() * 0.9f;
                        delay /= RingOfEnergy.ArtifactChargeMultiplier(target);
                    }
                }
                Spend(delay);
            }
            else
            {
                Deactivate();
            }

            return true;
        }

        public int Regencap()
        {
            return target.HT;
        }
    }
}