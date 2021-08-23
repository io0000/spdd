using System;
using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Sai : MeleeWeapon
    {
        public Sai()
        {
            image = ItemSpriteSheet.SAI;
            hitSound = Assets.Sounds.HIT_STAB;
            hitSoundPitch = 1.3f;

            tier = 3;
            DLY = 0.5f; //2x speed
        }

        public override int Max(int lvl)
        {
            double value = Math.Round(2.5f * (tier + 1), MidpointRounding.AwayFromZero) +        //10 base, down from 20
                    lvl * Math.Round(0.5f * (tier + 1), MidpointRounding.AwayFromZero);          //+2 per level, down from +4

            return (int)value;
        }
    }
}