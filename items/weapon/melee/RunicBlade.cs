using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class RunicBlade : MeleeWeapon
    {
        public RunicBlade()
        {
            image = ItemSpriteSheet.RUNIC_BLADE;
            hitSound = Assets.Sounds.HIT_SLASH;
            hitSoundPitch = 1f;

            tier = 4;
        }

        //Essentially it's a tier 4 weapon, with tier 3 base max damage, and tier 5 scaling.
        //equal to tier 4 in damage at +5
        public override int Max(int lvl)
        {
            return 5 * tier +         //20 base, down from 25
                    lvl * (tier + 2);   //+6 per level, up from +5
        }
    }
}