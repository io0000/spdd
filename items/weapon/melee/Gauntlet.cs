using System;
using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Gauntlet : MeleeWeapon
    {
        public Gauntlet()
        {
            image = ItemSpriteSheet.GAUNTLETS;
            hitSound = Assets.Sounds.HIT_CRUSH;
            hitSoundPitch = 1.2f;

            tier = 5;
            DLY = 0.5f; //2x speed
        }

        public override int Max(int lvl)
        {
            double value = Math.Round(2.5f * (tier + 1), MidpointRounding.AwayFromZero) +  //15 base, down from 30
                    lvl * Math.Round(0.5f * (tier + 1), MidpointRounding.AwayFromZero);    //+3 per level, down from +6
            return (int)value;
        }
    }
}