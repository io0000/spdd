using System;
using System.Collections.Generic;
using System.Globalization;
using spdd.actors.blobs;
using spdd.actors.buffs;
using spdd.items.armor.glyphs;
using spdd.actors;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.rings
{
    public class RingOfElements : Ring
    {
        public RingOfElements()
        {
            icon = ItemSpriteSheet.Icons.RING_ELEMENTS;
        }

        public override string StatsInfo()
        {
            if (IsIdentified())
            {
                var value = 100f * (1f - Math.Pow(0.825f, SoloBuffedBonus()));
                return Messages.Get(this, "stats", value.ToString("0.00", CultureInfo.InvariantCulture));
            }
            else
            {
                var value = 17.5f;
                return Messages.Get(this, "typical_stats", value.ToString("0.00", CultureInfo.InvariantCulture));
            }
        }

        public override RingBuff Buff()
        {
            return new Resistance(this);
        }

        public static HashSet<Type> RESISTS = new HashSet<Type>();

        static RingOfElements()
        {
            RESISTS.Add(typeof(Burning));
            RESISTS.Add(typeof(Chill));
            RESISTS.Add(typeof(Frost));
            RESISTS.Add(typeof(Ooze));
            RESISTS.Add(typeof(Paralysis));
            RESISTS.Add(typeof(Poison));
            RESISTS.Add(typeof(Corrosion));

            RESISTS.Add(typeof(ToxicGas));
            RESISTS.Add(typeof(Electricity));

            RESISTS.UnionWith(AntiMagic.RESISTS);
        }

        public static float Resist(Character target, Type effect)
        {
            int bonus = GetBuffedBonus<Resistance>(target);
            if (bonus == 0)
                return 1f;

            foreach (Type c in RESISTS)
            {
                if (c.IsAssignableFrom(effect))
                {
                    return (float)Math.Pow(0.825, bonus);
                }
            }

            return 1f;
        }

        public class Resistance : RingBuff
        {
            public Resistance(Ring ring)
                : base(ring)
            { }
        }
    }
}