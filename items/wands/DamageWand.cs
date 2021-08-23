using watabou.utils;
using spdd.messages;

namespace spdd.items.wands
{
    //for wands that directly damage a target
    //wands with AOE effects count here (e.g. fireblast), but wands with indrect damage do not (e.g. venom, transfusion)
    public abstract class DamageWand : Wand
    {
        public int Min()
        {
            return Min(BuffedLvl());
        }

        public abstract int Min(int lvl);

        public int Max()
        {
            return Max(BuffedLvl());
        }

        public abstract int Max(int lvl);

        public int DamageRoll()
        {
            return Rnd.NormalIntRange(Min(), Max());
        }

        public int DamageRoll(int lvl)
        {
            return Rnd.NormalIntRange(Min(lvl), Max(lvl));
        }

        public override string StatsDesc()
        {
            if (levelKnown)
                return Messages.Get(this, "stats_desc", Min(), Max());
            else
                return Messages.Get(this, "stats_desc", Min(0), Max(0));
        }
    }
}