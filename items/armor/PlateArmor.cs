using spdd.sprites;

namespace spdd.items.armor
{
    public class PlateArmor : Armor
    {
        public PlateArmor()
            : base(5)
        {
            image = ItemSpriteSheet.ARMOR_PLATE;
        }
    }
}