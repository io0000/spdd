using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.weapon.missiles.darts
{
    public class ChillingDart : TippedDart
    {
        public ChillingDart()
        {
            image = ItemSpriteSheet.CHILLING_DART;
        }

        public override int Proc(Character attacker, Character defender, int damage)
        {
            if (Dungeon.level.water[defender.pos])
            {
                Buff.Prolong<Chill>(defender, Chill.DURATION);
            }
            else
            {
                Buff.Prolong<Chill>(defender, 6f);
            }

            return base.Proc(attacker, defender, damage);
        }
    }
}