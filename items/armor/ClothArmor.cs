using spdd.sprites;

namespace spdd.items.armor
{
    public class ClothArmor : Armor
    {
        public ClothArmor()
            : base(1)
        {
            image = ItemSpriteSheet.ARMOR_CLOTH;

            bones = false; //Finding them in bones would be semi-frequent and disappointing.
        }
    }
}