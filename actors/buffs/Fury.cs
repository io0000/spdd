using spdd.ui;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Fury : Buff
    {
        public const float Level = 0.5f;

        public Fury()
        {
            type = BuffType.POSITIVE;
            announced = true;
        }

        public override bool Act()
        {
            if (target.HP > target.HT * Level)
                Detach();

            Spend(TICK);

            return true;
        }

        public override int Icon()
        {
            return BuffIndicator.FURY;
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
            return Messages.Get(this, "desc");
        }
    }
}