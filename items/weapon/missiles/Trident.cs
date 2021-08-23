using spdd.sprites;

namespace spdd.items.weapon.missiles
{
    public class Trident : MissileWeapon
    {
        public Trident()
        {
            image = ItemSpriteSheet.TRIDENT;
            hitSound = Assets.Sounds.HIT_SLASH;
            hitSoundPitch = 0.9f;

            tier = 5;
        }
    }
}