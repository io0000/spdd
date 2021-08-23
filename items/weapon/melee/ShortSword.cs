using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Shortsword : MeleeWeapon
    {
        public Shortsword()
        {
            image = ItemSpriteSheet.SHORTSWORD;
            hitSound = Assets.Sounds.HIT_SLASH;
            hitSoundPitch = 1.1f;

            tier = 2;
        }
    }
}