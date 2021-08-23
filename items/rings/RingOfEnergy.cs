using System;
using System.Globalization;
using spdd.sprites;
using spdd.actors;
using spdd.messages;

namespace spdd.items.rings
{
    public class RingOfEnergy : Ring
    {
        public RingOfEnergy()
        {
            icon = ItemSpriteSheet.Icons.RING_ENERGY;
        }

        public override string StatsInfo()
        {
            if (IsIdentified())
            {
                var value1 = 100f * (Math.Pow(1.20f, SoloBuffedBonus()) - 1f);
                var value2 = 100f * (Math.Pow(1.10f, SoloBuffedBonus()) - 1f);
                return Messages.Get(this, "stats",
                        value1.ToString("0.00", CultureInfo.InvariantCulture),
                        value2.ToString("0.00", CultureInfo.InvariantCulture));
            }
            else
            {
                var value1 = 20f;
                var value2 = 10f;
                return Messages.Get(this, "typical_stats",
                        value1.ToString("0.00", CultureInfo.InvariantCulture),
                        value2.ToString("0.00", CultureInfo.InvariantCulture));
            }
        }

        public override RingBuff Buff()
        {
            return new Energy(this);
        }

        public static float WandChargeMultiplier(Character target)
        {
            return (float)Math.Pow(1.20, GetBuffedBonus<Energy>(target));
        }

        public static float ArtifactChargeMultiplier(Character target)
        {
            return (float)Math.Pow(1.10, GetBuffedBonus<Energy>(target));
        }

        public class Energy : RingBuff
        {
            public Energy(Ring ring)
                : base(ring)
            { }
        }
    }
}