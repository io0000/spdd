using spdd.sprites;

namespace spdd.items.weapon.missiles
{
    public class ThrowingHammer : MissileWeapon
    {
        public ThrowingHammer()
        {
            image = ItemSpriteSheet.THROWING_HAMMER;
            hitSound = Assets.Sounds.HIT_CRUSH;
            hitSoundPitch = 0.8f;

            tier = 5;
            baseUses = 15;
            sticky = false;
        }

        public override int Max(int lvl)
        {
            return 4 * tier +                  //20 base, down from 25
                   (tier) * lvl;               //scaling unchanged
        }
    }
}