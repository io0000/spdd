using watabou.noosa.audio;
using spdd.sprites;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.utils;
using spdd.messages;

namespace spdd.items.potions
{
    public class PotionOfInvisibility : Potion
    {
        public PotionOfInvisibility()
        {
            icon = ItemSpriteSheet.Icons.POTION_INVIS;
        }

        public override void Apply(Hero hero)
        {
            SetKnown();
            Buff.Affect<Invisibility>(hero, Invisibility.DURATION);
            GLog.Information(Messages.Get(this, "invisible"));
            Sample.Instance.Play(Assets.Sounds.MELD);
        }

        public override int Value()
        {
            return IsKnown() ? 40 * quantity : base.Value();
        }
    }
}