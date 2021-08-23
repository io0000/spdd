using System;
using System.Globalization;
using spdd.actors;
using spdd.actors.hero;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.rings
{
    public class RingOfMight : Ring
    {
        public RingOfMight()
        {
            icon = ItemSpriteSheet.Icons.RING_MIGHT;
        }

        public override bool DoEquip(Hero hero)
        {
            if (base.DoEquip(hero))
            {
                hero.UpdateHT(false);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool DoUnequip(Hero hero, bool collect, bool single)
        {
            if (base.DoUnequip(hero, collect, single))
            {
                hero.UpdateHT(false);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override Item Upgrade()
        {
            base.Upgrade();
            UpdateTargetHT();
            return this;
        }

        public override void SetLevel(int value)
        {
            base.SetLevel(value);
            UpdateTargetHT();
        }

        private void UpdateTargetHT()
        {
            if (buff != null && buff.target is Hero)
            {
                ((Hero)buff.target).UpdateHT(false);
            }
        }

        public override string StatsInfo()
        {
            if (IsIdentified())
            {
                var value = 100f * (Math.Pow(1.035, SoloBuffedBonus()) - 1f);
                return Messages.Get(this, "stats", SoloBonus(), value.ToString("0.00", CultureInfo.InvariantCulture));
            }
            else
            {
                var value = 3.5f;
                return Messages.Get(this, "typical_stats", 1, value.ToString("0.00", CultureInfo.InvariantCulture));
            }
        }

        public override RingBuff Buff()
        {
            return new Might(this);
        }

        public static int StrengthBonus(Character target)
        {
            return GetBonus<Might>(target);
        }

        public static float HTMultiplier(Character target)
        {
            return (float)Math.Pow(1.035, GetBuffedBonus<Might>(target));
        }

        public class Might : RingBuff
        {
            public Might(Ring ring)
                : base(ring)
            { }
        }
    }
}