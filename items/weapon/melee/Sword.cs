using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Sword : MeleeWeapon
    {
        public Sword()
        {
            image = ItemSpriteSheet.SWORD;
            hitSound = Assets.Sounds.HIT_SLASH;
            hitSoundPitch = 1f;

            tier = 3;
        }
    }
}