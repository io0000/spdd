using spdd.actors;
using spdd.actors.hero;
using spdd.sprites;

namespace spdd.items.weapon.missiles
{
    public class Shuriken : MissileWeapon
    {
        public Shuriken()
        {
            image = ItemSpriteSheet.SHURIKEN;
            hitSound = Assets.Sounds.HIT_STAB;
            hitSoundPitch = 1.2f;

            tier = 2;
            baseUses = 5;
        }

        public override int Max(int lvl)
        {
            return 4 * tier +                          //8 base, down from 10
                   (tier == 1 ? 2 * lvl : tier * lvl); //scaling unchanged
        }

        public override float SpeedFactor(Character owner)
        {
            if (owner is Hero && ((Hero)owner).justMoved)
            {
                return 0;
            }
            else
            {
                return base.SpeedFactor(owner);
            }
        }
    }
}