using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class WornShortsword : MeleeWeapon
    {
        public WornShortsword()
        {
            image = ItemSpriteSheet.WORN_SHORTSWORD;
            hitSound = Assets.Sounds.HIT_SLASH;
            hitSoundPitch = 1.1f;

            tier = 1;

            bones = false;
        }
    }
}