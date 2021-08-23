using spdd.sprites;

namespace spdd.items.weapon.missiles
{
    public class ThrowingClub : MissileWeapon
    {
        public ThrowingClub()
        {
            image = ItemSpriteSheet.THROWING_CLUB;
            hitSound = Assets.Sounds.HIT_CRUSH;
            hitSoundPitch = 1.1f;

            tier = 2;
            baseUses = 15;
            sticky = false;
        }

        public override int Max(int lvl)
        {
            return 4 * tier +                //8 base, down from 10
                   tier * lvl;               //scaling unchanged
        }
    }
}