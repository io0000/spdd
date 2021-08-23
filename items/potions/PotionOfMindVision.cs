using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.utils;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.potions
{
    public class PotionOfMindVision : Potion
    {
        public PotionOfMindVision()
        {
            icon = ItemSpriteSheet.Icons.POTION_MINDVIS;
        }

        public override void Apply(Hero hero)
        {
            SetKnown();
            Buff.Affect<MindVision>(hero, MindVision.DURATION);
            Dungeon.Observe();

            if (Dungeon.level.mobs.Count > 0)
                GLog.Information(Messages.Get(this, "see_mobs"));
            else
                GLog.Information(Messages.Get(this, "see_none"));
        }

        public override int Value()
        {
            return IsKnown() ? 30 * quantity : base.Value();
        }
    }
}