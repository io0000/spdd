using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.rings
{
    public class RingOfForce : Ring
    {
        public RingOfForce()
        {
            icon = ItemSpriteSheet.Icons.RING_FORCE;
        }

        public override RingBuff Buff()
        {
            return new Force(this);
        }

        public static int ArmedDamageBonus(Character ch)
        {
            return GetBuffedBonus<Force>(ch);
        }

        // *** Weapon-like properties ***

        private static float Tier(int str)
        {
            float tier = Math.Max(1, (str - 8) / 2f);
            //each str point after 18 is half as effective
            if (tier > 5)
            {
                tier = 5 + (tier - 5) / 2f;
            }
            return tier;
        }

        public static int DamageRoll(Hero hero)
        {
            if (hero.FindBuff<Force>() != null)
            {
                int level = GetBuffedBonus<Force>(hero);
                float tier = Tier(hero.GetSTR());
                return Rnd.NormalIntRange(Min(level, tier), Max(level, tier));
            }
            else
            {
                //attack without any ring of force influence
                return Rnd.NormalIntRange(1, Math.Max(hero.GetSTR() - 8, 1));
            }
        }

        //same as equivalent tier weapon
        private static int Min(int lvl, float tier)
        {
            return (int)Math.Max(0, Math.Round(
                    tier +  //base
                    lvl     //level scaling
                    , MidpointRounding.AwayFromZero
            ));
        }

        //same as equivalent tier weapon
        private static int Max(int lvl, float tier)
        {
            return (int)Math.Max(0, Math.Round(
                    5 * (tier + 1) +    //base
                    lvl * (tier + 1)    //level scaling
                    , MidpointRounding.AwayFromZero
            ));
        }

        public override string StatsInfo()
        {
            float tier = Tier(Dungeon.hero.GetSTR());
            if (IsIdentified())
            {
                int level = SoloBuffedBonus();
                return Messages.Get(this, "stats", Min(level, tier), Max(level, tier), level);
            }
            else
            {
                return Messages.Get(this, "typical_stats", Min(1, tier), Max(1, tier), 1);
            }
        }

        public class Force : RingBuff
        {
            public Force(Ring ring)
                : base(ring)
            { }
        }
    }
}