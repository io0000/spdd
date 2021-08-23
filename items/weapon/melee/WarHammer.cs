using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class WarHammer : MeleeWeapon
    {
        public WarHammer()
        {
            image = ItemSpriteSheet.WAR_HAMMER;
            hitSound = Assets.Sounds.HIT_CRUSH;
            hitSoundPitch = 1f;

            tier = 5;
            ACC = 1.20f; //20% boost to accuracy
        }

        public override int Max(int lvl)
        {
            return 4 * (tier + 1) +     //24 base, down from 30
                   lvl * (tier + 1);    //scaling unchanged
        }
    }
}