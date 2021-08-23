using spdd.sprites;

namespace spdd.items.weapon.missiles
{
    public class Javelin : MissileWeapon
    {
        public Javelin()
        {
            image = ItemSpriteSheet.JAVELIN;
            hitSound = Assets.Sounds.HIT_STAB;
            hitSoundPitch = 1f;

            tier = 4;
        }
    }
}