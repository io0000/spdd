using spdd.sprites;
using spdd.actors.hero;
using spdd.actors.buffs;

namespace spdd.items.potions.exotic
{
    public class PotionOfShielding : ExoticPotion
    {
        public PotionOfShielding()
        {
            icon = ItemSpriteSheet.Icons.POTION_SHIELDING;
        }

        public override void Apply(Hero hero)
        {
            SetKnown();

            //~75% of a potion of healing
            Buff.Affect<Barrier>(hero).SetShield((int)(0.6f * hero.HT + 10));
        }
    }
}
