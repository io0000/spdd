using spdd.sprites;

namespace spdd.items.armor
{
    public class ScaleArmor : Armor
    {
        public ScaleArmor()
            : base(4)
        {
            image = ItemSpriteSheet.ARMOR_SCALE;
        }
    }
}