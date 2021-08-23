using spdd.sprites;
using spdd.actors.hero;
using spdd.actors.buffs;

namespace spdd.items.potions.exotic
{
    public class PotionOfHolyFuror : ExoticPotion
    {
        public PotionOfHolyFuror()
        {
            icon = ItemSpriteSheet.Icons.POTION_HOLYFUROR;
        }

        public override void Apply(Hero hero)
        {
            SetKnown();
            Buff.Prolong<Bless>(hero, Bless.DURATION * 4f);
        }
    }
}
