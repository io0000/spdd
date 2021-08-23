using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Longsword : MeleeWeapon
    {
        public Longsword()
        {
            image = ItemSpriteSheet.LONGSWORD;
            hitSound = Assets.Sounds.HIT_SLASH;
            hitSoundPitch = 1f;

            tier = 4;
        }
    }
}