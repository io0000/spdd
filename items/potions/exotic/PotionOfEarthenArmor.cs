using spdd.sprites;
using spdd.actors.hero;
using spdd.actors.buffs;

namespace spdd.items.potions.exotic
{
    public class PotionOfEarthenArmor : ExoticPotion
    {
        public PotionOfEarthenArmor()
        {
            icon = ItemSpriteSheet.Icons.POTION_EARTHARMR;
        }

        public override void Apply(Hero hero)
        {
            SetKnown();

            Buff.Affect<Barkskin>(hero).Set(2 + hero.lvl / 3, 50);
        }
    }
}
