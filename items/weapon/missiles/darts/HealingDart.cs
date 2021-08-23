using spdd.actors;
using spdd.actors.buffs;
using spdd.items.potions;
using spdd.sprites;

namespace spdd.items.weapon.missiles.darts
{
    public class HealingDart : TippedDart
    {
        public HealingDart()
        {
            image = ItemSpriteSheet.HEALING_DART;
        }

        public override int Proc(Character attacker, Character defender, int damage)
        {
            //heals 30 hp at base, scaling with enemy HT
            var healing = Buff.Affect<Healing>(defender);
            healing.SetHeal((int)(0.5f * defender.HT + 30), 0.25f, 0);
            PotionOfHealing.Cure(defender);

            if (attacker.alignment == defender.alignment)
            {
                return 0;
            }

            return base.Proc(attacker, defender, damage);
        }
    }
}