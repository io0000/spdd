using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.weapon.missiles.darts
{
    public class AdrenalineDart : TippedDart
    {
        public AdrenalineDart()
        {
            image = ItemSpriteSheet.ADRENALINE_DART;
        }

        public override int Proc(Character attacker, Character defender, int damage)
        {
            Buff.Prolong<Adrenaline>(defender, Adrenaline.DURATION);

            if (attacker.alignment == defender.alignment)
            {
                return 0;
            }

            return base.Proc(attacker, defender, damage);
        }
    }
}