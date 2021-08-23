using spdd.items;
using spdd.items.armor;
using spdd.items.artifacts;
using spdd.items.food;
using spdd.items.potions;

namespace spdd
{
    public class Challenges
    {
        //Some of these internal IDs are outdated and don't represent what these challenges do
        public const int NO_FOOD = 1;
        public const int NO_ARMOR = 2;
        public const int NO_HEALING = 4;
        public const int NO_HERBALISM = 8;
        public const int SWARM_INTELLIGENCE = 16;
        public const int DARKNESS = 32;
        public const int NO_SCROLLS = 64;

        public const int MAX_VALUE = 127;

        public static readonly string[] NAME_IDS = {
            "no_food",
            "no_armor",
            "no_healing",
            "no_herbalism",
            "swarm_intelligence",
            "darkness",
            "no_scrolls"
        };

        public static readonly int[] MASKS = {
            NO_FOOD, NO_ARMOR, NO_HEALING, NO_HERBALISM, SWARM_INTELLIGENCE, DARKNESS, NO_SCROLLS
        };

        public static bool IsItemBlocked(Item item)
        {
            if (Dungeon.IsChallenged(NO_FOOD))
            {
                if (item is Food && !(item is SmallRation))
                    return true;
                else if (item is HornOfPlenty)
                    return true;
            }

            if (Dungeon.IsChallenged(NO_ARMOR))
            {
                if (item is Armor && !(item is ClothArmor || item is ClassArmor))
                    return true;
            }

            if (Dungeon.IsChallenged(NO_HEALING))
            {
                if (item is PotionOfHealing)
                    return true;
                else if (item is Blandfruit && ((Blandfruit)item).potionAttrib is PotionOfHealing)
                    return true;
            }

            if (Dungeon.IsChallenged(NO_HERBALISM))
            {
                if (item is Dewdrop)
                    return true;
            }

            return false;
        }
    }
}