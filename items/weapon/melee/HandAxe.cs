using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class HandAxe : MeleeWeapon
    {
        public HandAxe()
        {
            image = ItemSpriteSheet.HAND_AXE;
            hitSound = Assets.Sounds.HIT_SLASH;
            hitSoundPitch = 1f;

            tier = 2;
            ACC = 1.32f; //32% boost to accuracy
        }

        public override int Max(int lvl)
        {
            return 4 * (tier + 1) +     //12 base, down from 15
                   lvl * (tier + 1);    //scaling unchanged
        }
    }
}