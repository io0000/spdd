using spdd.sprites;
using spdd.actors.hero;
using spdd.actors.buffs;

namespace spdd.items.potions.exotic
{
    public class PotionOfAdrenalineSurge : ExoticPotion
    {
        public PotionOfAdrenalineSurge()
        {
            icon = ItemSpriteSheet.Icons.POTION_ARENSURGE;

            unique = true;
        }

        public override void Apply(Hero hero)
        {
            SetKnown();
            Buff.Affect<AdrenalineSurge>(hero).Reset(2, 800f);
        }
    }
}
