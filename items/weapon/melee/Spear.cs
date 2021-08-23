using System;
using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Spear : MeleeWeapon
    {
        public Spear()
        {
            image = ItemSpriteSheet.SPEAR;
            hitSound = Assets.Sounds.HIT_STAB;
            hitSoundPitch = 0.9f;

            tier = 2;
            DLY = 1.5f; //0.67x speed
            RCH = 2;    //extra reach
        }

        public override int Max(int lvl)
        {
            double value = Math.Round(6.67f * (tier + 1), MidpointRounding.AwayFromZero) +         //20 base, up from 15
                    lvl * Math.Round(1.33f * (tier + 1), MidpointRounding.AwayFromZero);           //+4 per level, up from +3
            return (int)value;
        }
    }
}