using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Scimitar : MeleeWeapon
    {
        public Scimitar()
        {
            image = ItemSpriteSheet.SCIMITAR;
            hitSound = Assets.Sounds.HIT_SLASH;
            hitSoundPitch = 1.2f;

            tier = 3;
            DLY = 0.8f; //1.25x speed
        }

        public override int Max(int lvl)
        {
            return 4 * (tier + 1) +     //16 base, down from 20
                   lvl * (tier + 1);    //scaling unchanged
        }
    }
}