using spdd.actors.hero;
using spdd.sprites;

namespace spdd.items.potions
{
    public class PotionOfExperience : Potion
    {
        public PotionOfExperience()
        {
            icon = ItemSpriteSheet.Icons.POTION_EXP;

            bones = true;
        }

        public override void Apply(Hero hero)
        {
            SetKnown();
            hero.EarnExp(hero.MaxExp(), GetType());
        }

        public override int Value()
        {
            return IsKnown() ? 50 * quantity : base.Value();
        }
    }
}