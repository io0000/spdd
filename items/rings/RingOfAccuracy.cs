using System;
using System.Globalization;
using spdd.actors;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.rings
{
    public class RingOfAccuracy : Ring
    {
        public RingOfAccuracy()
        {
            icon = ItemSpriteSheet.Icons.RING_ACCURACY;
        }

        public override string StatsInfo()
        {
            if (IsIdentified())
            {
                var value = 100f * (Math.Pow(1.3f, SoloBuffedBonus()) - 1f);
                return Messages.Get(this, "stats", value.ToString("0.00", CultureInfo.InvariantCulture));
            }
            else
            {
                var value = 30.0f;
                return Messages.Get(this, "typical_stats", value.ToString("0.00", CultureInfo.InvariantCulture));
            }
        }

        public override RingBuff Buff()
        {
            return new Accuracy(this);
        }

        public static float AccuracyMultiplier(Character target)
        {
            return (float)Math.Pow(1.3f, GetBuffedBonus<Accuracy>(target));
        }

        public class Accuracy : RingBuff
        {
            public Accuracy(Ring ring)
                : base(ring)
            { }
        }
    }
}