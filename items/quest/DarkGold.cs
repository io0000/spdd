using spdd.sprites;

namespace spdd.items.quest
{
    public class DarkGold : Item
    {
        public DarkGold()
        {
            image = ItemSpriteSheet.ORE;

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