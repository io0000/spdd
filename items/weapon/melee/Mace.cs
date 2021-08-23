using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Mace : MeleeWeapon
    {
        public Mace()
        {
            image = ItemSpriteSheet.MACE;
            hitSound = Assets.Sounds.HIT_CRUSH;
            hitSoundPitch = 1f;

            tier = 3;
            ACC = 1.28f; //28% boost to accuracy
        }

        public override int Max(int lvl)
        {
            return 4 * (tier + 1) +     //16 base, down from 20
                   lvl * (tier + 1);    //scaling unchanged
        }
    }
}