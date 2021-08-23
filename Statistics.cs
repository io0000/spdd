using watabou.utils;

namespace spdd
{
    // hero ΩΩ∑‘∏∂¥Ÿ ¿˙¿Âµ 
    public class Statistics
    {
        public static int goldCollected;
        public static int deepestFloor;
        public static int enemiesSlain;
        public static int foodEaten;
        public static int potionsCooked;
        public static int piranhasKilled;
        public static int ankhsUsed;

        //used for hero unlock badges
        public static int upgradesUsed;
        public static int sneakAttacks;
        public static int thrownAssists;

        public static int spawnersAlive;

        public static float duration;

        public static bool qualifiedForNoKilling;
        public static bool completedWithNoKilling;

        public static bool amuletObtained;

        public static void Reset()
        {
            goldCollected = 0;
            deepestFloor = 0;
            enemiesSlain = 0;
            foodEaten = 0;
            potionsCooked = 0;
            piranhasKilled = 0;
            ankhsUsed = 0;

            upgradesUsed = 0;
            sneakAttacks = 0;
            thrownAssists = 0;

            spawnersAlive = 0;

            duration = 0;

            qualifiedForNoKilling = false;

            amuletObtained = false;
        }

        private const string GOLD = "score";
        private const string DEEPEST = "maxDepth";
        private const string SLAIN = "enemiesSlain";
        private const string FOOD = "foodEaten";
        private const string ALCHEMY = "potionsCooked";
        private const string PIRANHAS = "priranhas";
        private const string ANKHS = "ankhsUsed";

        private const string UPGRADES = "upgradesUsed";
        private const string SNEAKS = "sneakAttacks";
        private const string THROWN = "thrownAssists";

        private const string SPAWNERS = "spawnersAlive";

        private const string DURATION = "duration";

        private const string AMULET = "amuletObtained";

        public static void StoreInBundle(Bundle bundle)
        {
            bundle.Put(GOLD, goldCollected);
            bundle.Put(DEEPEST, deepestFloor);
            bundle.Put(SLAIN, enemiesSlain);
            bundle.Put(FOOD, foodEaten);
            bundle.Put(ALCHEMY, potionsCooked);
            bundle.Put(PIRANHAS, piranhasKilled);
            bundle.Put(ANKHS, ankhsUsed);

            bundle.Put(UPGRADES, upgradesUsed);
            bundle.Put(SNEAKS, sneakAttacks);
            bundle.Put(THROWN, thrownAssists);

            bundle.Put(SPAWNERS, spawnersAlive);

            bundle.Put(DURATION, duration);

            bundle.Put(AMULET, amuletObtained);
        }

        public static void RestoreFromBundle(Bundle bundle)
        {
            goldCollected = bundle.GetInt(GOLD);
            deepestFloor = bundle.GetInt(DEEPEST);
            enemiesSlain = bundle.GetInt(SLAIN);
            foodEaten = bundle.GetInt(FOOD);
            potionsCooked = bundle.GetInt(ALCHEMY);
            piranhasKilled = bundle.GetInt(PIRANHAS);
            ankhsUsed = bundle.GetInt(ANKHS);

            upgradesUsed = bundle.GetInt(UPGRADES);
            sneakAttacks = bundle.GetInt(SNEAKS);
            thrownAssists = bundle.GetInt(THROWN);

            spawnersAlive = bundle.GetInt(SPAWNERS);

            duration = bundle.GetFloat(DURATION);

            amuletObtained = bundle.GetBoolean(AMULET);
        }

        public static void Preview(GamesInProgress.Info info, Bundle bundle)
        {
            info.goldCollected = bundle.GetInt(GOLD);
            info.maxDepth = bundle.GetInt(DEEPEST);
        }
    }
}