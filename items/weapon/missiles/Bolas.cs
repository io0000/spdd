using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.weapon.missiles
{
    public class Bolas : MissileWeapon
    {
        public Bolas()
        {
            image = ItemSpriteSheet.BOLAS;
            hitSound = Assets.Sounds.HIT;
            hitSoundPitch = 1f;

            tier = 3;
            baseUses = 5;
        }

        public override int Max(int lvl)
        {
            return 3 * tier +                          //9 base, down from 15
                   (tier == 1 ? 2 * lvl : tier * lvl); //scaling unchanged
        }

        public override int Proc(Character attacker, Character defender, int damage)
        {
            Buff.Prolong<Cripple>(defender, Cripple.DURATION);
            return base.Proc(attacker, defender, damage);
        }
    }
}