using spdd.actors.hero;
using spdd.sprites;
using spdd.utils;
using spdd.messages;

namespace spdd.items.potions
{
    public class PotionOfStrength : Potion
    {
        public PotionOfStrength()
        {
            icon = ItemSpriteSheet.Icons.POTION_STRENGTH;

            unique = true;
        }

        public override void Apply(Hero hero)
        {
            SetKnown();

            ++hero.STR;
            hero.sprite.ShowStatus(CharSprite.POSITIVE, Messages.Get(this, "msg_1"));
            GLog.Positive(Messages.Get(this, "msg_2"));

            BadgesExtensions.ValidateStrengthAttained();
        }

        public override int Value()
        {
            return IsKnown() ? 50 * quantity : base.Value();
        }
    }
}