using spdd.actors;
using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Quarterstaff : MeleeWeapon
    {
        public Quarterstaff()
        {
            image = ItemSpriteSheet.QUARTERSTAFF;
            hitSound = Assets.Sounds.HIT_CRUSH;
            hitSoundPitch = 1f;

            tier = 2;
        }

        public override int Max(int lvl)
        {
            return 4 * (tier + 1) +     //12 base, down from 15
                   lvl * (tier + 1);    //scaling unchanged
        }

        public override int DefenseFactor(Character owner)
        {
            return 2; //2 extra defence
        }
    }
}