using System;
using System.Globalization;
using spdd.actors;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.rings
{
    public class RingOfEvasion : Ring
    {
        public RingOfEvasion()
        {
            icon = ItemSpriteSheet.Icons.RING_EVASION;
        }

        public override string StatsInfo()
        {
            if (IsIdentified())
            {
                var value = 100f * (Math.Pow(1.15f, SoloBuffedBonus()) - 1f);
                return Messages.Get(this, "stats", value.ToString("0.00", CultureInfo.InvariantCulture));
            }
            else
            {
                var value = 15f;
                return Messages.Get(this, "typical_stats", value.ToString("0.00", CultureInfo.InvariantCulture));
            }
        }

        public override RingBuff Buff()
        {
            return new Evasion(this);
        }

        public static float EvasionMultiplier(Character target)
        {
            return (float)Math.Pow(1.15, GetBuffedBonus<Evasion>(target));
        }

        public class Evasion : RingBuff
        {
            public Evasion(Ring ring)
                : base(ring)
            { }
        }
    }
}