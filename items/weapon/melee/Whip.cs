using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Whip : MeleeWeapon
    {
        public Whip()
        {
            image = ItemSpriteSheet.WHIP;
            hitSound = Assets.Sounds.HIT;
            hitSoundPitch = 1.1f;

            tier = 3;
            RCH = 3; //lots of extra reach
        }

        public override int Max(int lvl)
        {
            return 3 * (tier + 1) +     //12 base, down from 20
                   lvl * (tier);        //+3 per level, down from +4
        }
    }
}