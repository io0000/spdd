using System;
using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Glaive : MeleeWeapon
    {
        public Glaive()
        {
            image = ItemSpriteSheet.GLAIVE;
            hitSound = Assets.Sounds.HIT_SLASH;
            hitSoundPitch = 0.8f;

            tier = 5;
            DLY = 1.5f; //0.67x speed
            RCH = 2;    //extra reach
        }

        public override int Max(int lvl)
        {
            double value = Math.Round(6.67f * (tier + 1), MidpointRounding.AwayFromZero) +     //40 base, up from 30
                    lvl * Math.Round(1.33f * (tier + 1), MidpointRounding.AwayFromZero);       //+8 per level, up from +6

            return (int)value;
        }
    }
}