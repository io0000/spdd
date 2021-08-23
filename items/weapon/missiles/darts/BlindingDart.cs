using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.weapon.missiles.darts
{
    public class BlindingDart : TippedDart
    {
        public BlindingDart()
        {
            image = ItemSpriteSheet.BLINDING_DART;
        }

        public override int Proc(Character attacker, Character defender, int damage)
        {
            Buff.Affect<Blindness>(defender, Blindness.DURATION);

            return base.Proc(attacker, defender, damage);
        }
    }
}