using System;
using System.Globalization;
using spdd.actors;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.rings
{
    public class RingOfFuror : Ring
    {
        public RingOfFuror()
        {
            icon = ItemSpriteSheet.Icons.RING_FUROR;
        }

        public override string StatsInfo()
        {
            if (IsIdentified())
            {
                var value = 100f * (Math.Pow(1.105f, SoloBuffedBonus()) - 1f);
                return Messages.Get(this, "stats", value.ToString("0.00", CultureInfo.InvariantCulture));
            }
            else
            {
                var value = 10.5f;
                return Messages.Get(this, "typical_stats", value.ToString("0.00", CultureInfo.InvariantCulture));
            }
        }

        public override RingBuff Buff()
        {
            return new Furor(this);
        }

        public static float AttackDelayMultiplier(Character target)
        {
            return 1f / (float)Math.Pow(1.105, GetBuffedBonus<Furor>(target));
        }

        public class Furor : RingBuff
        {
            public Furor(Ring ring)
                : base(ring)
            { }
        }
    }
}