using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.weapon.missiles.darts
{
    public class ParalyticDart : TippedDart
    {
        public ParalyticDart()
        {
            image = ItemSpriteSheet.PARALYTIC_DART;
        }

        public override int Proc(Character attacker, Character defender, int damage)
        {
            Buff.Prolong<Paralysis>(defender, 5f);
            return base.Proc(attacker, defender, damage);
        }
    }
}