using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Greatsword : MeleeWeapon
    {
        public Greatsword()
        {
            image = ItemSpriteSheet.GREATSWORD;
            hitSound = Assets.Sounds.HIT_SLASH;
            hitSoundPitch = 1f;

            tier = 5;
        }
    }
}