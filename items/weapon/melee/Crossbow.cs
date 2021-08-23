using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Crossbow : MeleeWeapon
    {
        public Crossbow()
        {
            image = ItemSpriteSheet.CROSSBOW;
            hitSound = Assets.Sounds.HIT;
            hitSoundPitch = 1f;

            //check Dart.class for additional properties

            tier = 4;
        }

        public override int Max(int lvl)
        {
            return 4 * (tier + 1) +     //20 base, down from 25
                   lvl * (tier);        //+4 per level, down from +5
        }
    }
}