using spdd.sprites;

namespace spdd.items.quest
{
    public class DwarfToken : Item
    {
        public DwarfToken()
        {
            image = ItemSpriteSheet.TOKEN;

            stackable = true;
            unique = true;
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }
    }
}