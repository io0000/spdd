using spdd.sprites;

namespace spdd.items.weapon.missiles
{
    public class ThrowingSpear : MissileWeapon
    {
        public ThrowingSpear()
        {
            image = ItemSpriteSheet.THROWING_SPEAR;
            hitSound = Assets.Sounds.HIT_STAB;
            hitSoundPitch = 1f;

            tier = 3;
        }
    }
}