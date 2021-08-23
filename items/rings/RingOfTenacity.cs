using System;
using System.Globalization;
using spdd.actors;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.rings
{
    public class RingOfTenacity : Ring
    {
        public RingOfTenacity()
        {
            icon = ItemSpriteSheet.Icons.RING_TENACITY;
        }

        public override string StatsInfo()
        {
            if (IsIdentified())
            {
                var value = 100f * (1f - Math.Pow(0.85f, SoloBuffedBonus()));
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
            return new Tenacity(this);
        }

        public static float DamageMultiplier(Character t)
        {
            //(HT - HP)/HT = heroes current % missing health.
            return (float)Math.Pow(0.85, GetBuffedBonus<Tenacity>(t) * ((float)(t.HT - t.HP) / t.HT));
        }

        public class Tenacity : RingBuff
        {
            public Tenacity(Ring ring)
                : base(ring)
            { }
        }
    }
}