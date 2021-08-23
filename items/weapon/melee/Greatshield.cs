using System;
using spdd.actors;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.weapon.melee
{
    public class Greatshield : MeleeWeapon
    {
        public Greatshield()
        {
            image = ItemSpriteSheet.GREATSHIELD;

            tier = 5;
        }

        public override int Max(int lvl)
        {
            return (int)Math.Round(2.5f * (tier + 1), MidpointRounding.AwayFromZero) +     //15 base, down from 30
                    lvl * (tier - 2);                       //+3 per level, down from +6
        }

        public override int DefenseFactor(Character owner)
        {
            return 6 + 3 * BuffedLvl(); //6 extra defence, plus 3 per level;
        }

        public override string StatsInfo()
        {
            if (IsIdentified())
            {
                return Messages.Get(this, "stats_desc", 6 + 3 * BuffedLvl());
            }
            else
            {
                return Messages.Get(this, "typical_stats_desc", 6);
            }
        }
    }
}