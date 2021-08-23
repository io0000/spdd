using System;
using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Flail : MeleeWeapon
    {
        public Flail()
        {
            image = ItemSpriteSheet.FLAIL;
            hitSound = Assets.Sounds.HIT_CRUSH;
            hitSoundPitch = 0.8f;

            tier = 4;
            ACC = 0.8f; //0.8x accuracy
            //also cannot surprise attack, see Hero.canSurpriseAttack
        }

        public override int Max(int lvl)
        {
            double value = 7 * (tier + 1) +               //35 base, up from 25
                    lvl * Math.Round(1.6f * (tier + 1), MidpointRounding.AwayFromZero);  //+8 per level, up from +5

            return (int)value;
        }
    }
}