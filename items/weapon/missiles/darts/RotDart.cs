using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.weapon.missiles.darts
{
    public class RotDart : TippedDart
    {
        public RotDart()
        {
            image = ItemSpriteSheet.ROT_DART;
        }

        public override int Proc(Character attacker, Character defender, int damage)
        {
            if (defender.Properties().Contains(Character.Property.BOSS) ||
                defender.Properties().Contains(Character.Property.MINIBOSS))
            {
                Buff.Affect<Corrosion>(defender).Set(5f, Dungeon.depth / 3);
            }
            else
            {
                Buff.Affect<Corrosion>(defender).Set(10f, Dungeon.depth);
            }

            return base.Proc(attacker, defender, damage);
        }

        protected override float DurabilityPerUse()
        {
            return 100f;
        }
    }
}