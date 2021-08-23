using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.utils;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.potions
{
    public class PotionOfHaste : Potion
    {
        public PotionOfHaste()
        {
            icon = ItemSpriteSheet.Icons.POTION_HASTE;
        }

        public override void Apply(Hero hero)
        {
            SetKnown();

            GLog.Warning(Messages.Get(this, "energetic"));
            Buff.Prolong<Haste>(hero, Haste.DURATION);
        }

        public override int Value()
        {
            return IsKnown() ? 40 * Quantity() : base.Value();
        }
    }
}