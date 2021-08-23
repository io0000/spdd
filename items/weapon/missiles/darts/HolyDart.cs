using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.weapon.missiles.darts
{
    public class HolyDart : TippedDart
    {
        public HolyDart()
        {
            image = ItemSpriteSheet.HOLY_DART;
        }

        public override int Proc(Character attacker, Character defender, int damage)
        {
            Buff.Affect<Bless>(defender, Bless.DURATION);

            if (attacker.alignment == defender.alignment)
            {
                return 0;
            }

            return base.Proc(attacker, defender, damage);
        }
    }
}