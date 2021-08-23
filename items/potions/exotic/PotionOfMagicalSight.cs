using spdd.sprites;
using spdd.actors.hero;
using spdd.actors.buffs;

namespace spdd.items.potions.exotic
{
    public class PotionOfMagicalSight : ExoticPotion
    {
        public PotionOfMagicalSight()
        {
            icon = ItemSpriteSheet.Icons.POTION_MAGISIGHT;
        }

        public override void Apply(Hero hero)
        {
            SetKnown();
            Buff.Affect<MagicalSight>(hero, MagicalSight.DURATION);
            Dungeon.Observe();
        }
    }
}
