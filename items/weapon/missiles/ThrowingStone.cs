using spdd.sprites;

namespace spdd.items.weapon.missiles
{
    public class ThrowingStone : MissileWeapon
    {
        public ThrowingStone()
        {
            image = ItemSpriteSheet.THROWING_STONE;
            hitSound = Assets.Sounds.HIT;
            hitSoundPitch = 1.1f;

            bones = false;

            tier = 1;
            baseUses = 5;
            sticky = false;
        }

        public override int Value()
        {
            return base.Value() / 2; //half normal value
        }
    }
}