using spdd.sprites;
using spdd.actors.hero;
using spdd.actors.buffs;

namespace spdd.items.potions.exotic
{
    public class PotionOfStamina : ExoticPotion
    {
        public PotionOfStamina()
        {
            icon = ItemSpriteSheet.Icons.POTION_STAMINA;
        }

        public override void Apply(Hero hero)
        {
            SetKnown();

            Buff.Affect<Stamina>(hero, Stamina.DURATION);
        }
    }
}
