using spdd.sprites;

namespace spdd.items.armor
{
    public class LeatherArmor : Armor
    {
        public LeatherArmor()
            : base(2)
        {
            image = ItemSpriteSheet.ARMOR_LEATHER;
        }
    }
}