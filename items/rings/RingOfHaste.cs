using System;
using System.Globalization;
using spdd.actors;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.rings
{
    public class RingOfHaste : Ring
    {
        public RingOfHaste()
        {
            icon = ItemSpriteSheet.Icons.RING_HASTE;
        }

        public override string StatsInfo()
        {
            if (IsIdentified())
            {
                var value = 100f * (Math.Pow(1.2f, SoloBuffedBonus()) - 1f);
                return Messages.Get(this, "stats", value.ToString("0.00", CultureInfo.InvariantCulture));
            }
            else
            {
                var value = 20f;
                return Messages.Get(this, "typical_stats", value.ToString("0.00", CultureInfo.InvariantCulture));
            }
        }

        public override RingBuff Buff()
        {
            return new Haste(this);
        }

        public static float SpeedMultiplier(Character target)
        {
            return (float)Math.Pow(1.2, GetBuffedBonus<Haste>(target));
        }

        public class Haste : RingBuff
        {
            public Haste(Ring ring)
                : base(ring)
            { }
        }
    }
}