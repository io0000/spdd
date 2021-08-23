using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Gloves : MeleeWeapon
    {
        public Gloves()
        {
            image = ItemSpriteSheet.GLOVES;
            hitSound = Assets.Sounds.HIT;
            hitSoundPitch = 1.3f;

            tier = 1;
            DLY = 0.5f; //2x speed

            bones = false;
        }

        public override int Max(int lvl)
        {
            return (3 * (tier + 1)) +    //6 base, down from 10
                   lvl * tier;           //+1 per level, down from +2
        }
    }
}