using System;
using spdd.sprites;
using spdd.actors;
using spdd.messages;

namespace spdd.items.weapon.melee
{
    public class RoundShield : MeleeWeapon
    {
        public RoundShield()
        {
            image = ItemSpriteSheet.ROUND_SHIELD;
            hitSound = Assets.Sounds.HIT;
            hitSoundPitch = 1f;

            tier = 3;
        }

        public override int Max(int lvl)
        {
            double value = Math.Round(2.5f * (tier + 1), MidpointRounding.AwayFromZero) +     //10 base, down from 20
                    lvl * (tier - 1);                                                         //+2 per level, down from +4

            return (int)value;
        }

        public override int DefenseFactor(Character owner)
        {
            return 4 + 2 * BuffedLvl(); //4 extra defence, plus 2 per level;
        }

        public override string StatsInfo()
        {
            if (IsIdentified())
                return Messages.Get(this, "stats_desc", 4 + 2 * BuffedLvl());
            else
                return Messages.Get(this, "typical_stats_desc", 4);
        }
    }
}