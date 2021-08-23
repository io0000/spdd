using System;
using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Greataxe : MeleeWeapon
    {
        public Greataxe()
        {
            image = ItemSpriteSheet.GREATAXE;
            hitSound = Assets.Sounds.HIT_SLASH;
            hitSoundPitch = 1f;

            tier = 5;
        }

        public override int Max(int lvl)
        {
            return 5 * (tier + 4) +    //45 base, up from 30
                   lvl * (tier + 1);   //scaling unchanged
        }

        public override int STRReq(int lvl)
        {
            lvl = Math.Max(0, lvl);
            //20 base strength req, up from 18
            return (10 + tier * 2) - (int)(Math.Sqrt(8 * lvl + 1) - 1) / 2;
        }
    }
}