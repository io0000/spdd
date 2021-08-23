using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.weapon.missiles.darts
{
    public class PoisonDart : TippedDart
    {
        public PoisonDart()
        {
            image = ItemSpriteSheet.POISON_DART;
        }

        public override int Proc(Character attacker, Character defender, int damage)
        {
            Buff.Affect<Poison>(defender).Set(3 + Dungeon.depth / 3);

            return base.Proc(attacker, defender, damage);
        }
    }
}