using System;
using System.Collections.Generic;
using System.IO;
using watabou.utils;
using spdd.actors.hero;
using spdd.items;
using spdd.items.artifacts;
using spdd.items.bags;
using spdd.journal;
using spdd.messages;
using spdd.scenes;
using spdd.utils;

namespace spdd
{
    public class Badges
    {
        public enum Badge
        {
            MONSTERS_SLAIN_1,
            MONSTERS_SLAIN_2,
            MONSTERS_SLAIN_3,
            MONSTERS_SLAIN_4,
            GOLD_COLLECTED_1,
            GOLD_COLLECTED_2,
            GOLD_COLLECTED_3,
            GOLD_COLLECTED_4,
            LEVEL_REACHED_1,
            LEVEL_REACHED_2,
            LEVEL_REACHED_3,
            LEVEL_REACHED_4,
            ALL_WEAPONS_IDENTIFIED,
            ALL_ARMOR_IDENTIFIED,
            ALL_WANDS_IDENTIFIED,
            ALL_RINGS_IDENTIFIED,
            ALL_ARTIFACTS_IDENTIFIED,
            ALL_POTIONS_IDENTIFIED,
            ALL_SCROLLS_IDENTIFIED,
            ALL_ITEMS_IDENTIFIED,
            //these names are a bit outdated, but it doesn't really matter.
            BAG_BOUGHT_SEED_POUCH,
            BAG_BOUGHT_SCROLL_HOLDER,
            BAG_BOUGHT_POTION_BANDOLIER,
            BAG_BOUGHT_WAND_HOLSTER,
            ALL_BAGS_BOUGHT,
            DEATH_FROM_FIRE,
            DEATH_FROM_POISON,
            DEATH_FROM_GAS,
            DEATH_FROM_HUNGER,
            DEATH_FROM_GLYPH,
            DEATH_FROM_FALLING,
            YASD,
            BOSS_SLAIN_1_WARRIOR,
            BOSS_SLAIN_1_MAGE,
            BOSS_SLAIN_1_ROGUE,
            BOSS_SLAIN_1_HUNTRESS,
            BOSS_SLAIN_1,
            BOSS_SLAIN_2,
            BOSS_SLAIN_3,
            BOSS_SLAIN_4,
            BOSS_SLAIN_1_ALL_CLASSES,
            BOSS_SLAIN_3_GLADIATOR,
            BOSS_SLAIN_3_BERSERKER,
            BOSS_SLAIN_3_WARLOCK,
            BOSS_SLAIN_3_BATTLEMAGE,
            BOSS_SLAIN_3_FREERUNNER,
            BOSS_SLAIN_3_ASSASSIN,
            BOSS_SLAIN_3_SNIPER,
            BOSS_SLAIN_3_WARDEN,
            BOSS_SLAIN_3_ALL_SUBCLASSES,
            VICTORY_WARRIOR,
            VICTORY_MAGE,
            VICTORY_ROGUE,
            VICTORY_HUNTRESS,
            VICTORY,
            VICTORY_ALL_CLASSES,
            HAPPY_END,
            CHAMPION_1,
            CHAMPION_2,
            CHAMPION_3,
            STRENGTH_ATTAINED_1,
            STRENGTH_ATTAINED_2,
            STRENGTH_ATTAINED_3,
            STRENGTH_ATTAINED_4,
            FOOD_EATEN_1,
            FOOD_EATEN_2,
            FOOD_EATEN_3,
            FOOD_EATEN_4,
            MASTERY_WARRIOR,
            MASTERY_MAGE,
            MASTERY_ROGUE,
            MASTERY_HUNTRESS,
            UNLOCK_MAGE,
            UNLOCK_ROGUE,
            UNLOCK_HUNTRESS,
            ITEM_LEVEL_1,
            ITEM_LEVEL_2,
            ITEM_LEVEL_3,
            ITEM_LEVEL_4,
            POTIONS_COOKED_1,
            POTIONS_COOKED_2,
            POTIONS_COOKED_3,
            POTIONS_COOKED_4,
            MASTERY_COMBO,
            NO_MONSTERS_SLAIN,
            GRIM_WEAPON,
            PIRANHAS,
            GAMES_PLAYED_1,
            GAMES_PLAYED_2,
            GAMES_PLAYED_3,
            GAMES_PLAYED_4,
            MAX_BADGE
        }
    }

    public static class BadgesExtensions
    {
        static bool[] metaArray = new bool[(int)Badges.Badge.MAX_BADGE];
        static int[] imageArray = new int[(int)Badges.Badge.MAX_BADGE];

        static BadgesExtensions()
        {
            // All elements of the new array instance are initialized to their default values (¡×5.2).
            for (int i = 0; i < (int)Badges.Badge.MAX_BADGE; ++i)
                metaArray[i] = false;

            metaArray[(int)Badges.Badge.ALL_ITEMS_IDENTIFIED] = true;
            metaArray[(int)Badges.Badge.YASD] = true;
            metaArray[(int)Badges.Badge.BOSS_SLAIN_1_ALL_CLASSES] = true;
            metaArray[(int)Badges.Badge.BOSS_SLAIN_3_ALL_SUBCLASSES] = true;
            metaArray[(int)Badges.Badge.VICTORY_ALL_CLASSES] = true;
            metaArray[(int)Badges.Badge.CHAMPION_1] = true;
            metaArray[(int)Badges.Badge.CHAMPION_2] = true;
            metaArray[(int)Badges.Badge.CHAMPION_3] = true;
            metaArray[(int)Badges.Badge.GAMES_PLAYED_1] = true;
            metaArray[(int)Badges.Badge.GAMES_PLAYED_2] = true;
            metaArray[(int)Badges.Badge.GAMES_PLAYED_3] = true;
            metaArray[(int)Badges.Badge.GAMES_PLAYED_4] = true;

            imageArray[(int)Badges.Badge.MONSTERS_SLAIN_1] = 0;
            imageArray[(int)Badges.Badge.MONSTERS_SLAIN_2] = 1;
            imageArray[(int)Badges.Badge.MONSTERS_SLAIN_3] = 2;
            imageArray[(int)Badges.Badge.MONSTERS_SLAIN_4] = 3;
            imageArray[(int)Badges.Badge.GOLD_COLLECTED_1] = 4;
            imageArray[(int)Badges.Badge.GOLD_COLLECTED_2] = 5;
            imageArray[(int)Badges.Badge.GOLD_COLLECTED_3] = 6;
            imageArray[(int)Badges.Badge.GOLD_COLLECTED_4] = 7;
            imageArray[(int)Badges.Badge.LEVEL_REACHED_1] = 8;
            imageArray[(int)Badges.Badge.LEVEL_REACHED_2] = 9;
            imageArray[(int)Badges.Badge.LEVEL_REACHED_3] = 10;
            imageArray[(int)Badges.Badge.LEVEL_REACHED_4] = 11;
            imageArray[(int)Badges.Badge.ALL_WEAPONS_IDENTIFIED] = 16;
            imageArray[(int)Badges.Badge.ALL_ARMOR_IDENTIFIED] = 17;
            imageArray[(int)Badges.Badge.ALL_WANDS_IDENTIFIED] = 18;
            imageArray[(int)Badges.Badge.ALL_RINGS_IDENTIFIED] = 19;
            imageArray[(int)Badges.Badge.ALL_ARTIFACTS_IDENTIFIED] = 20;
            imageArray[(int)Badges.Badge.ALL_POTIONS_IDENTIFIED] = 21;
            imageArray[(int)Badges.Badge.ALL_SCROLLS_IDENTIFIED] = 22;
            imageArray[(int)Badges.Badge.ALL_ITEMS_IDENTIFIED] = 23;
            //these names are a bit outdated, but it doesn't really matter.
            imageArray[(int)Badges.Badge.BAG_BOUGHT_SEED_POUCH] = -1;
            imageArray[(int)Badges.Badge.BAG_BOUGHT_SCROLL_HOLDER] = -1;
            imageArray[(int)Badges.Badge.BAG_BOUGHT_POTION_BANDOLIER] = -1;
            imageArray[(int)Badges.Badge.BAG_BOUGHT_WAND_HOLSTER] = -1;
            imageArray[(int)Badges.Badge.ALL_BAGS_BOUGHT] = 24;
            imageArray[(int)Badges.Badge.DEATH_FROM_FIRE] = 25;
            imageArray[(int)Badges.Badge.DEATH_FROM_POISON] = 26;
            imageArray[(int)Badges.Badge.DEATH_FROM_GAS] = 27;
            imageArray[(int)Badges.Badge.DEATH_FROM_HUNGER] = 28;
            imageArray[(int)Badges.Badge.DEATH_FROM_GLYPH] = 29;
            imageArray[(int)Badges.Badge.DEATH_FROM_FALLING] = 30;
            imageArray[(int)Badges.Badge.YASD] = 31;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_1_WARRIOR] = -1;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_1_MAGE] = -1;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_1_ROGUE] = -1;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_1_HUNTRESS] = -1;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_1] = 12;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_2] = 13;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_3] = 14;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_4] = 15;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_1_ALL_CLASSES] = 32;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_3_GLADIATOR] = -1;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_3_BERSERKER] = -1;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_3_WARLOCK] = -1;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_3_BATTLEMAGE] = -1;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_3_FREERUNNER] = -1;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_3_ASSASSIN] = -1;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_3_SNIPER] = -1;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_3_WARDEN] = -1;
            imageArray[(int)Badges.Badge.BOSS_SLAIN_3_ALL_SUBCLASSES] = 33;
            imageArray[(int)Badges.Badge.VICTORY_WARRIOR] = -1;
            imageArray[(int)Badges.Badge.VICTORY_MAGE] = -1;
            imageArray[(int)Badges.Badge.VICTORY_ROGUE] = -1;
            imageArray[(int)Badges.Badge.VICTORY_HUNTRESS] = -1;
            imageArray[(int)Badges.Badge.VICTORY] = 34;
            imageArray[(int)Badges.Badge.VICTORY_ALL_CLASSES] = 35;
            imageArray[(int)Badges.Badge.HAPPY_END] = 36;
            imageArray[(int)Badges.Badge.CHAMPION_1] = 37;
            imageArray[(int)Badges.Badge.CHAMPION_2] = 38;
            imageArray[(int)Badges.Badge.CHAMPION_3] = 39;
            imageArray[(int)Badges.Badge.STRENGTH_ATTAINED_1] = 40;
            imageArray[(int)Badges.Badge.STRENGTH_ATTAINED_2] = 41;
            imageArray[(int)Badges.Badge.STRENGTH_ATTAINED_3] = 42;
            imageArray[(int)Badges.Badge.STRENGTH_ATTAINED_4] = 43;
            imageArray[(int)Badges.Badge.FOOD_EATEN_1] = 44;
            imageArray[(int)Badges.Badge.FOOD_EATEN_2] = 45;
            imageArray[(int)Badges.Badge.FOOD_EATEN_3] = 46;
            imageArray[(int)Badges.Badge.FOOD_EATEN_4] = 47;
            imageArray[(int)Badges.Badge.MASTERY_WARRIOR] = -1;
            imageArray[(int)Badges.Badge.MASTERY_MAGE] = -1;
            imageArray[(int)Badges.Badge.MASTERY_ROGUE] = -1;
            imageArray[(int)Badges.Badge.MASTERY_HUNTRESS] = -1;
            imageArray[(int)Badges.Badge.UNLOCK_MAGE] = 65;
            imageArray[(int)Badges.Badge.UNLOCK_ROGUE] = 66;
            imageArray[(int)Badges.Badge.UNLOCK_HUNTRESS] = 67;
            imageArray[(int)Badges.Badge.ITEM_LEVEL_1] = 48;
            imageArray[(int)Badges.Badge.ITEM_LEVEL_2] = 49;
            imageArray[(int)Badges.Badge.ITEM_LEVEL_3] = 50;
            imageArray[(int)Badges.Badge.ITEM_LEVEL_4] = 51;
            imageArray[(int)Badges.Badge.POTIONS_COOKED_1] = 52;
            imageArray[(int)Badges.Badge.POTIONS_COOKED_2] = 53;
            imageArray[(int)Badges.Badge.POTIONS_COOKED_3] = 54;
            imageArray[(int)Badges.Badge.POTIONS_COOKED_4] = 55;
            imageArray[(int)Badges.Badge.MASTERY_COMBO] = 56;
            imageArray[(int)Badges.Badge.NO_MONSTERS_SLAIN] = 57;
            imageArray[(int)Badges.Badge.GRIM_WEAPON] = 58;
            imageArray[(int)Badges.Badge.PIRANHAS] = 59;
            imageArray[(int)Badges.Badge.GAMES_PLAYED_1] = 60;
            imageArray[(int)Badges.Badge.GAMES_PLAYED_2] = 61;
            imageArray[(int)Badges.Badge.GAMES_PLAYED_3] = 62;
            imageArray[(int)Badges.Badge.GAMES_PLAYED_4] = 63;
        }

        public static string Desc(this Badges.Badge badge)
        {
            return Messages.Get(badge.GetType(), badge.ToString());
        }

        public static bool GetMeta(this Badges.Badge badge)
        {
            int index = (int)badge;
            return metaArray[index];
        }

        public static int GetImage(this Badges.Badge badge)
        {
            int index = (int)badge;
            return imageArray[index];
        }

        private static HashSet<Badges.Badge> global;
        private static HashSet<Badges.Badge> local = new HashSet<Badges.Badge>();

        private static bool saveNeeded;

        public static void Reset()
        {
            local.Clear();
            LoadGlobal();
        }

        private const string BADGES_FILE = "badges.dat";
        private const string BADGES = "badges";

        private static HashSet<string> removedBadges = new HashSet<string>();

        private static Dictionary<string, string> renamedBadges = new Dictionary<string, string>();

        private static HashSet<Badges.Badge> Restore(Bundle bundle)
        {
            var badges = new HashSet<Badges.Badge>();
            if (bundle == null)
                return badges;

            var names = bundle.GetStringArray(BADGES);
            for (int i = 0; i < names.Length; ++i)
            {
                try
                {
                    if (renamedBadges.ContainsKey(names[i]))
                    {
                        names[i] = renamedBadges[names[i]];
                    }
                    if (!removedBadges.Contains(names[i]))
                    {
                        var value = (Badges.Badge)Enum.Parse(typeof(Badges.Badge), names[i]);
                        badges.Add(value);
                    }
                }
                catch (Exception e)
                {
                    ShatteredPixelDungeonDash.ReportException(e);
                }
            }

            return badges;
        }

        public static void Store(Bundle bundle, HashSet<Badges.Badge> badges)
        {
            int count = 0;
            string[] names = new string[badges.Count];

            foreach (var badge in badges)
            {
                names[count++] = badge.ToString();
            }
            bundle.Put(BADGES, names);
        }

        public static void LoadLocal(Bundle bundle)
        {
            local = Restore(bundle);
        }

        public static void SaveLocal(Bundle bundle)
        {
            Store(bundle, local);
        }

        public static void LoadGlobal()
        {
            if (global == null)
            {
                try
                {
                    Bundle bundle = FileUtils.BundleFromFile(BADGES_FILE);
                    global = Restore(bundle);
                }
                catch (IOException)
                {
                    global = new HashSet<Badges.Badge>();
                }
            }
        }

        public static void SaveGlobal()
        {
            if (saveNeeded)
            {
                Bundle bundle = new Bundle();
                Store(bundle, global);

                try
                {
                    FileUtils.BundleToFile(BADGES_FILE, bundle);
                    saveNeeded = false;
                }
                catch (IOException e)
                {
                    ShatteredPixelDungeonDash.ReportException(e);
                }
            }
        }

        public static void ValidateMonstersSlain()
        {
            Badges.Badge? badge = null;

            if (!local.Contains(Badges.Badge.MONSTERS_SLAIN_1) && Statistics.enemiesSlain >= 10)
            {
                badge = Badges.Badge.MONSTERS_SLAIN_1;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.MONSTERS_SLAIN_2) && Statistics.enemiesSlain >= 50)
            {
                badge = Badges.Badge.MONSTERS_SLAIN_2;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.MONSTERS_SLAIN_3) && Statistics.enemiesSlain >= 150)
            {
                badge = Badges.Badge.MONSTERS_SLAIN_3;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.MONSTERS_SLAIN_4) && Statistics.enemiesSlain >= 250)
            {
                badge = Badges.Badge.MONSTERS_SLAIN_4;
                local.Add(badge.Value);
            }

            DisplayBadge(badge);
        }

        public static void ValidateGoldCollected()
        {
            Badges.Badge? badge = null;

            if (!local.Contains(Badges.Badge.GOLD_COLLECTED_1) && Statistics.goldCollected >= 100)
            {
                badge = Badges.Badge.GOLD_COLLECTED_1;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.GOLD_COLLECTED_2) && Statistics.goldCollected >= 500)
            {
                badge = Badges.Badge.GOLD_COLLECTED_2;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.GOLD_COLLECTED_3) && Statistics.goldCollected >= 2500)
            {
                badge = Badges.Badge.GOLD_COLLECTED_3;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.GOLD_COLLECTED_4) && Statistics.goldCollected >= 7500)
            {
                badge = Badges.Badge.GOLD_COLLECTED_4;
                local.Add(badge.Value);
            }

            DisplayBadge(badge);
        }

        public static void ValidateLevelReached()
        {
            Badges.Badge? badge = null;

            if (!local.Contains(Badges.Badge.LEVEL_REACHED_1) && Dungeon.hero.lvl >= 6)
            {
                badge = Badges.Badge.LEVEL_REACHED_1;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.LEVEL_REACHED_2) && Dungeon.hero.lvl >= 12)
            {
                badge = Badges.Badge.LEVEL_REACHED_2;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.LEVEL_REACHED_3) && Dungeon.hero.lvl >= 18)
            {
                badge = Badges.Badge.LEVEL_REACHED_3;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.LEVEL_REACHED_4) && Dungeon.hero.lvl >= 24)
            {
                badge = Badges.Badge.LEVEL_REACHED_4;
                local.Add(badge.Value);
            }

            DisplayBadge(badge);
        }

        public static void ValidateStrengthAttained()
        {
            Badges.Badge? badge = null;

            if (!local.Contains(Badges.Badge.STRENGTH_ATTAINED_1) && Dungeon.hero.STR >= 13)
            {
                badge = Badges.Badge.STRENGTH_ATTAINED_1;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.STRENGTH_ATTAINED_2) && Dungeon.hero.STR >= 15)
            {
                badge = Badges.Badge.STRENGTH_ATTAINED_2;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.STRENGTH_ATTAINED_3) && Dungeon.hero.STR >= 17)
            {
                badge = Badges.Badge.STRENGTH_ATTAINED_3;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.STRENGTH_ATTAINED_4) && Dungeon.hero.STR >= 19)
            {
                badge = Badges.Badge.STRENGTH_ATTAINED_4;
                local.Add(badge.Value);
            }

            DisplayBadge(badge);
        }

        public static void ValidateFoodEaten()
        {
            Badges.Badge? badge = null;

            if (!local.Contains(Badges.Badge.FOOD_EATEN_1) && Statistics.foodEaten >= 10)
            {
                badge = Badges.Badge.FOOD_EATEN_1;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.FOOD_EATEN_2) && Statistics.foodEaten >= 20)
            {
                badge = Badges.Badge.FOOD_EATEN_2;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.FOOD_EATEN_3) && Statistics.foodEaten >= 30)
            {
                badge = Badges.Badge.FOOD_EATEN_3;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.FOOD_EATEN_4) && Statistics.foodEaten >= 40)
            {
                badge = Badges.Badge.FOOD_EATEN_4;
                local.Add(badge.Value);
            }

            DisplayBadge(badge);
        }

        public static void ValidatePotionsCooked()
        {
            Badges.Badge? badge = null;

            if (!local.Contains(Badges.Badge.POTIONS_COOKED_1) && Statistics.potionsCooked >= 3)
            {
                badge = Badges.Badge.POTIONS_COOKED_1;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.POTIONS_COOKED_2) && Statistics.potionsCooked >= 6)
            {
                badge = Badges.Badge.POTIONS_COOKED_2;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.POTIONS_COOKED_3) && Statistics.potionsCooked >= 9)
            {
                badge = Badges.Badge.POTIONS_COOKED_3;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.POTIONS_COOKED_4) && Statistics.potionsCooked >= 12)
            {
                badge = Badges.Badge.POTIONS_COOKED_4;
                local.Add(badge.Value);
            }

            DisplayBadge(badge);
        }

        public static void ValidatePiranhasKilled()
        {
            Badges.Badge? badge = null;

            if (!local.Contains(Badges.Badge.PIRANHAS) && Statistics.piranhasKilled >= 6)
            {
                badge = Badges.Badge.PIRANHAS;
                local.Add(badge.Value);
            }

            DisplayBadge(badge);
        }

        public static void ValidateItemLevelAquired(Item item)
        {
            // This method should be called:
            // 1) When an item is obtained (Item.collect)
            // 2) When an item is upgraded (ScrollOfUpgrade, ScrollOfWeaponUpgrade, ShortSword, WandOfMagicMissile)
            // 3) When an item is identified

            // Note that artifacts should never trigger this badge as they are alternatively upgraded
            if (!item.levelKnown || item is Artifact)
                return;

            Badges.Badge? badge = null;
            if (!local.Contains(Badges.Badge.ITEM_LEVEL_1) && item.GetLevel() >= 3)
            {
                badge = Badges.Badge.ITEM_LEVEL_1;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.ITEM_LEVEL_2) && item.GetLevel() >= 6)
            {
                badge = Badges.Badge.ITEM_LEVEL_2;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.ITEM_LEVEL_3) && item.GetLevel() >= 9)
            {
                badge = Badges.Badge.ITEM_LEVEL_3;
                local.Add(badge.Value);
            }
            if (!local.Contains(Badges.Badge.ITEM_LEVEL_4) && item.GetLevel() >= 12)
            {
                badge = Badges.Badge.ITEM_LEVEL_4;
                local.Add(badge.Value);
            }

            DisplayBadge(badge);
        }

        public static void ValidateAllBagsBought(Item bag)
        {
            Badges.Badge? badge = null;
            if (bag is VelvetPouch)
            {
                badge = Badges.Badge.BAG_BOUGHT_SEED_POUCH;
            }
            else if (bag is ScrollHolder)
            {
                badge = Badges.Badge.BAG_BOUGHT_SCROLL_HOLDER;
            }
            else if (bag is PotionBandolier)
            {
                badge = Badges.Badge.BAG_BOUGHT_POTION_BANDOLIER;
            }
            else if (bag is MagicalHolster)
            {
                badge = Badges.Badge.BAG_BOUGHT_WAND_HOLSTER;
            }

            if (badge != null)
            {
                local.Add(badge.Value);

                if (!local.Contains(Badges.Badge.ALL_BAGS_BOUGHT) &&
                    local.Contains(Badges.Badge.BAG_BOUGHT_SEED_POUCH) &&
                    local.Contains(Badges.Badge.BAG_BOUGHT_SCROLL_HOLDER) &&
                    local.Contains(Badges.Badge.BAG_BOUGHT_POTION_BANDOLIER) &&
                    local.Contains(Badges.Badge.BAG_BOUGHT_WAND_HOLSTER))
                {
                    badge = Badges.Badge.ALL_BAGS_BOUGHT;
                    local.Add(badge.Value);
                    DisplayBadge(badge.Value);
                }
            }
        }

        public static void ValidateItemsIdentified()
        {
            var values = Enum.GetValues(typeof(Catalog));

            foreach (Catalog cat in values)
            {
                if (cat.AllSeen())
                {
                    Badges.Badge b = CatalogExtensions.catalogBadges[cat];
                    if (!global.Contains(b))
                    {
                        DisplayBadge(b);
                    }
                }
            }

            if (!global.Contains(Badges.Badge.ALL_ITEMS_IDENTIFIED) &&
                global.Contains(Badges.Badge.ALL_WEAPONS_IDENTIFIED) &&
                global.Contains(Badges.Badge.ALL_ARMOR_IDENTIFIED) &&
                global.Contains(Badges.Badge.ALL_WANDS_IDENTIFIED) &&
                global.Contains(Badges.Badge.ALL_RINGS_IDENTIFIED) &&
                global.Contains(Badges.Badge.ALL_ARTIFACTS_IDENTIFIED) &&
                global.Contains(Badges.Badge.ALL_POTIONS_IDENTIFIED) &&
                global.Contains(Badges.Badge.ALL_SCROLLS_IDENTIFIED))
            {
                DisplayBadge(Badges.Badge.ALL_ITEMS_IDENTIFIED);
            }
        }

        public static void ValidateDeathFromFire()
        {
            Badges.Badge badge = Badges.Badge.DEATH_FROM_FIRE;
            local.Add(badge);
            DisplayBadge(badge);

            ValidateYASD();
        }

        public static void ValidateDeathFromPoison()
        {
            Badges.Badge badge = Badges.Badge.DEATH_FROM_POISON;
            local.Add(badge);
            DisplayBadge(badge);

            ValidateYASD();
        }

        public static void ValidateDeathFromGas()
        {
            Badges.Badge badge = Badges.Badge.DEATH_FROM_GAS;
            local.Add(badge);
            DisplayBadge(badge);

            ValidateYASD();
        }

        public static void ValidateDeathFromHunger()
        {
            Badges.Badge badge = Badges.Badge.DEATH_FROM_HUNGER;
            local.Add(badge);
            DisplayBadge(badge);

            ValidateYASD();
        }

        public static void ValidateDeathFromGlyph()
        {
            Badges.Badge badge = Badges.Badge.DEATH_FROM_GLYPH;
            local.Add(badge);
            DisplayBadge(badge);

            ValidateYASD();
        }

        public static void ValidateDeathFromFalling()
        {
            Badges.Badge badge = Badges.Badge.DEATH_FROM_FALLING;
            local.Add(badge);
            DisplayBadge(badge);

            ValidateYASD();
        }

        private static void ValidateYASD()
        {
            if (global.Contains(Badges.Badge.DEATH_FROM_FIRE) &&
                global.Contains(Badges.Badge.DEATH_FROM_POISON) &&
                global.Contains(Badges.Badge.DEATH_FROM_GAS) &&
                global.Contains(Badges.Badge.DEATH_FROM_HUNGER) &&
                global.Contains(Badges.Badge.DEATH_FROM_GLYPH) &&
                global.Contains(Badges.Badge.DEATH_FROM_FALLING))
            {
                Badges.Badge badge = Badges.Badge.YASD;
                local.Add(badge);
                DisplayBadge(badge);
            }
        }

        public static void ValidateBossSlain()
        {
            Badges.Badge? badge = null;
            switch (Dungeon.depth)
            {
                case 5:
                    badge = Badges.Badge.BOSS_SLAIN_1;
                    break;
                case 10:
                    badge = Badges.Badge.BOSS_SLAIN_2;
                    break;
                case 15:
                    badge = Badges.Badge.BOSS_SLAIN_3;
                    break;
                case 20:
                    badge = Badges.Badge.BOSS_SLAIN_4;
                    break;
            }

            if (badge != null)
            {
                local.Add(badge.Value);
                DisplayBadge(badge.Value);

                if (badge == Badges.Badge.BOSS_SLAIN_1)
                {
                    switch (Dungeon.hero.heroClass)
                    {
                        case HeroClass.WARRIOR:
                            badge = Badges.Badge.BOSS_SLAIN_1_WARRIOR;
                            break;
                        case HeroClass.MAGE:
                            badge = Badges.Badge.BOSS_SLAIN_1_MAGE;
                            break;
                        case HeroClass.ROGUE:
                            badge = Badges.Badge.BOSS_SLAIN_1_ROGUE;
                            break;
                        case HeroClass.HUNTRESS:
                            badge = Badges.Badge.BOSS_SLAIN_1_HUNTRESS;
                            break;
                    }
                    local.Add(badge.Value);
                    if (!global.Contains(badge.Value))
                    {
                        global.Add(badge.Value);
                        saveNeeded = true;
                    }

                    if (global.Contains(Badges.Badge.BOSS_SLAIN_1_WARRIOR) &&
                        global.Contains(Badges.Badge.BOSS_SLAIN_1_MAGE) &&
                        global.Contains(Badges.Badge.BOSS_SLAIN_1_ROGUE) &&
                        global.Contains(Badges.Badge.BOSS_SLAIN_1_HUNTRESS))
                    {

                        badge = Badges.Badge.BOSS_SLAIN_1_ALL_CLASSES;
                        if (!global.Contains(badge.Value))
                        {
                            DisplayBadge(badge.Value);
                            global.Add(badge.Value);
                            saveNeeded = true;
                        }
                    }
                }
                else if (badge == Badges.Badge.BOSS_SLAIN_3)
                {
                    switch (Dungeon.hero.subClass)
                    {
                        case HeroSubClass.GLADIATOR:
                            badge = Badges.Badge.BOSS_SLAIN_3_GLADIATOR;
                            break;
                        case HeroSubClass.BERSERKER:
                            badge = Badges.Badge.BOSS_SLAIN_3_BERSERKER;
                            break;
                        case HeroSubClass.WARLOCK:
                            badge = Badges.Badge.BOSS_SLAIN_3_WARLOCK;
                            break;
                        case HeroSubClass.BATTLEMAGE:
                            badge = Badges.Badge.BOSS_SLAIN_3_BATTLEMAGE;
                            break;
                        case HeroSubClass.FREERUNNER:
                            badge = Badges.Badge.BOSS_SLAIN_3_FREERUNNER;
                            break;
                        case HeroSubClass.ASSASSIN:
                            badge = Badges.Badge.BOSS_SLAIN_3_ASSASSIN;
                            break;
                        case HeroSubClass.SNIPER:
                            badge = Badges.Badge.BOSS_SLAIN_3_SNIPER;
                            break;
                        case HeroSubClass.WARDEN:
                            badge = Badges.Badge.BOSS_SLAIN_3_WARDEN;
                            break;
                        default:
                            return;
                    }
                    local.Add(badge.Value);
                    if (!global.Contains(badge.Value))
                    {
                        global.Add(badge.Value);
                        saveNeeded = true;
                    }

                    if (global.Contains(Badges.Badge.BOSS_SLAIN_3_GLADIATOR) &&
                        global.Contains(Badges.Badge.BOSS_SLAIN_3_BERSERKER) &&
                        global.Contains(Badges.Badge.BOSS_SLAIN_3_WARLOCK) &&
                        global.Contains(Badges.Badge.BOSS_SLAIN_3_BATTLEMAGE) &&
                        global.Contains(Badges.Badge.BOSS_SLAIN_3_FREERUNNER) &&
                        global.Contains(Badges.Badge.BOSS_SLAIN_3_ASSASSIN) &&
                        global.Contains(Badges.Badge.BOSS_SLAIN_3_SNIPER) &&
                        global.Contains(Badges.Badge.BOSS_SLAIN_3_WARDEN))
                    {
                        badge = Badges.Badge.BOSS_SLAIN_3_ALL_SUBCLASSES;
                        if (!global.Contains(badge.Value))
                        {
                            DisplayBadge(badge.Value);
                            global.Add(badge.Value);
                            saveNeeded = true;
                        }
                    }
                }
            }
        }

        public static void ValidateMastery()
        {
            Badges.Badge? badge = null;
            switch (Dungeon.hero.heroClass)
            {
                case HeroClass.WARRIOR:
                    badge = Badges.Badge.MASTERY_WARRIOR;
                    break;
                case HeroClass.MAGE:
                    badge = Badges.Badge.MASTERY_MAGE;
                    break;
                case HeroClass.ROGUE:
                    badge = Badges.Badge.MASTERY_ROGUE;
                    break;
                case HeroClass.HUNTRESS:
                    badge = Badges.Badge.MASTERY_HUNTRESS;
                    break;
            }

            if (!global.Contains(badge.Value))
            {
                global.Add(badge.Value);
                saveNeeded = true;
            }
        }

        public static void ValidateMageUnlock()
        {
            if (Statistics.upgradesUsed >= 2 && !global.Contains(Badges.Badge.UNLOCK_MAGE))
            {
                DisplayBadge(Badges.Badge.UNLOCK_MAGE);
            }
        }

        public static void ValidateRogueUnlock()
        {
            if (Statistics.sneakAttacks >= 20 && !global.Contains(Badges.Badge.UNLOCK_ROGUE))
            {
                DisplayBadge(Badges.Badge.UNLOCK_ROGUE);
            }
        }

        public static void ValidateHuntressUnlock()
        {
            if (Statistics.thrownAssists >= 20 && !global.Contains(Badges.Badge.UNLOCK_HUNTRESS))
            {
                DisplayBadge(Badges.Badge.UNLOCK_HUNTRESS);
            }
        }

        public static void ValidateMasteryCombo(int n)
        {
            if (!local.Contains(Badges.Badge.MASTERY_COMBO) && n == 10)
            {
                Badges.Badge badge = Badges.Badge.MASTERY_COMBO;
                local.Add(badge);
                DisplayBadge(badge);
            }
        }

        public static void ValidateVictory()
        {
            Badges.Badge badge = Badges.Badge.VICTORY;
            DisplayBadge(badge);

            switch (Dungeon.hero.heroClass)
            {
                case HeroClass.WARRIOR:
                    badge = Badges.Badge.VICTORY_WARRIOR;
                    break;
                case HeroClass.MAGE:
                    badge = Badges.Badge.VICTORY_MAGE;
                    break;
                case HeroClass.ROGUE:
                    badge = Badges.Badge.VICTORY_ROGUE;
                    break;
                case HeroClass.HUNTRESS:
                    badge = Badges.Badge.VICTORY_HUNTRESS;
                    break;
            }
            local.Add(badge);
            if (!global.Contains(badge))
            {
                global.Add(badge);
                saveNeeded = true;
            }

            if (global.Contains(Badges.Badge.VICTORY_WARRIOR) &&
                global.Contains(Badges.Badge.VICTORY_MAGE) &&
                global.Contains(Badges.Badge.VICTORY_ROGUE) &&
                global.Contains(Badges.Badge.VICTORY_HUNTRESS))
            {
                badge = Badges.Badge.VICTORY_ALL_CLASSES;
                DisplayBadge(badge);
            }
        }

        public static void ValidateNoKilling()
        {
            if (!local.Contains(Badges.Badge.NO_MONSTERS_SLAIN) && Statistics.completedWithNoKilling)
            {
                Badges.Badge badge = Badges.Badge.NO_MONSTERS_SLAIN;
                local.Add(badge);
                DisplayBadge(badge);
            }
        }

        public static void ValidateGrimWeapon()
        {
            if (!local.Contains(Badges.Badge.GRIM_WEAPON))
            {
                Badges.Badge badge = Badges.Badge.GRIM_WEAPON;
                local.Add(badge);
                DisplayBadge(badge);
            }
        }

        public static void ValidateGamesPlayed()
        {
            Badges.Badge? badge = null;
            if (Rankings.Instance.totalNumber >= 10)
            {
                badge = Badges.Badge.GAMES_PLAYED_1;
            }
            if (Rankings.Instance.totalNumber >= 50)
            {
                badge = Badges.Badge.GAMES_PLAYED_2;
            }
            if (Rankings.Instance.totalNumber >= 250)
            {
                badge = Badges.Badge.GAMES_PLAYED_3;
            }
            if (Rankings.Instance.totalNumber >= 1000)
            {
                badge = Badges.Badge.GAMES_PLAYED_4;
            }

            DisplayBadge(badge);
        }

        //necessary in order to display the happy end badge in the surface scene
        public static void SilentValidateHappyEnd()
        {
            if (!local.Contains(Badges.Badge.HAPPY_END))
            {
                local.Add(Badges.Badge.HAPPY_END);
            }
        }

        public static void ValidateHappyEnd()
        {
            DisplayBadge(Badges.Badge.HAPPY_END);
        }

        public static void ValidateChampion(int challenges)
        {
            Badges.Badge? badge = null;
            if (challenges >= 1)
            {
                badge = Badges.Badge.CHAMPION_1;
            }
            if (challenges >= 3)
            {
                badge = Badges.Badge.CHAMPION_2;
            }
            if (challenges >= 6)
            {
                badge = Badges.Badge.CHAMPION_3;
            }
            DisplayBadge(badge);
        }

        private static void DisplayBadge(Badges.Badge? _badge)
        {
            if (_badge == null)
                return;

            Badges.Badge badge = _badge.Value;

            if (global.Contains(badge))
            {
                if (!badge.GetMeta())
                {
                    GLog.Highlight(Messages.Get(typeof(Badges), "endorsed", badge.Desc()));
                }
            }
            else
            {
                global.Add(badge);
                saveNeeded = true;

                if (badge.GetMeta())
                {
                    GLog.Highlight(Messages.Get(typeof(Badges), "new_super", badge.Desc()));
                }
                else
                {
                    GLog.Highlight(Messages.Get(typeof(Badges), "new", badge.Desc()));
                }
                PixelScene.ShowBadge(badge);
            }
        }

        public static bool IsUnlocked(Badges.Badge badge)
        {
            return global.Contains(badge);
        }

        //public static HashSet<Badges.Badge> AllUnlocked()
        //{
        //    LoadGlobal();
        //    return new HashSet<Badges.Badge>(global);
        //}
        //
        //public static void Disown(Badges.Badge badge)
        //{
        //    LoadGlobal();
        //    global.Remove(badge);
        //    saveNeeded = true;
        //}
        //
        //public static void AddGlobal(Badges.Badge badge)
        //{
        //    if (!global.Contains(badge))
        //    {
        //        global.Add(badge);
        //        saveNeeded = true;
        //    }
        //}

        public static List<Badges.Badge> Filtered(bool global)
        {
            HashSet<Badges.Badge> filtered = new HashSet<Badges.Badge>(global ? BadgesExtensions.global : local);

            //Iterator<Badge> iterator = filtered.iterator();
            //while (iterator.hasNext())
            //{
            //    Badge badge = iterator.next();
            //    if ((!global && badge.meta) || badge.image == -1)
            //    {
            //        iterator.remove();
            //    }
            //}

            filtered.RemoveWhere(delegate (Badges.Badge badge)
                {
                    return ((!global && badge.GetMeta()) || badge.GetImage() == -1);
                });

            LeaveBest(filtered, Badges.Badge.MONSTERS_SLAIN_1, Badges.Badge.MONSTERS_SLAIN_2, Badges.Badge.MONSTERS_SLAIN_3, Badges.Badge.MONSTERS_SLAIN_4);
            LeaveBest(filtered, Badges.Badge.GOLD_COLLECTED_1, Badges.Badge.GOLD_COLLECTED_2, Badges.Badge.GOLD_COLLECTED_3, Badges.Badge.GOLD_COLLECTED_4);
            LeaveBest(filtered, Badges.Badge.BOSS_SLAIN_1, Badges.Badge.BOSS_SLAIN_2, Badges.Badge.BOSS_SLAIN_3, Badges.Badge.BOSS_SLAIN_4);
            LeaveBest(filtered, Badges.Badge.LEVEL_REACHED_1, Badges.Badge.LEVEL_REACHED_2, Badges.Badge.LEVEL_REACHED_3, Badges.Badge.LEVEL_REACHED_4);
            LeaveBest(filtered, Badges.Badge.STRENGTH_ATTAINED_1, Badges.Badge.STRENGTH_ATTAINED_2, Badges.Badge.STRENGTH_ATTAINED_3, Badges.Badge.STRENGTH_ATTAINED_4);
            LeaveBest(filtered, Badges.Badge.FOOD_EATEN_1, Badges.Badge.FOOD_EATEN_2, Badges.Badge.FOOD_EATEN_3, Badges.Badge.FOOD_EATEN_4);
            LeaveBest(filtered, Badges.Badge.ITEM_LEVEL_1, Badges.Badge.ITEM_LEVEL_2, Badges.Badge.ITEM_LEVEL_3, Badges.Badge.ITEM_LEVEL_4);
            LeaveBest(filtered, Badges.Badge.POTIONS_COOKED_1, Badges.Badge.POTIONS_COOKED_2, Badges.Badge.POTIONS_COOKED_3, Badges.Badge.POTIONS_COOKED_4);
            LeaveBest(filtered, Badges.Badge.DEATH_FROM_FIRE, Badges.Badge.YASD);
            LeaveBest(filtered, Badges.Badge.DEATH_FROM_GAS, Badges.Badge.YASD);
            LeaveBest(filtered, Badges.Badge.DEATH_FROM_HUNGER, Badges.Badge.YASD);
            LeaveBest(filtered, Badges.Badge.DEATH_FROM_POISON, Badges.Badge.YASD);
            LeaveBest(filtered, Badges.Badge.DEATH_FROM_GLYPH, Badges.Badge.YASD);
            LeaveBest(filtered, Badges.Badge.DEATH_FROM_FALLING, Badges.Badge.YASD);
            LeaveBest(filtered, Badges.Badge.ALL_WEAPONS_IDENTIFIED, Badges.Badge.ALL_ITEMS_IDENTIFIED);
            LeaveBest(filtered, Badges.Badge.ALL_ARMOR_IDENTIFIED, Badges.Badge.ALL_ITEMS_IDENTIFIED);
            LeaveBest(filtered, Badges.Badge.ALL_WANDS_IDENTIFIED, Badges.Badge.ALL_ITEMS_IDENTIFIED);
            LeaveBest(filtered, Badges.Badge.ALL_RINGS_IDENTIFIED, Badges.Badge.ALL_ITEMS_IDENTIFIED);
            LeaveBest(filtered, Badges.Badge.ALL_ARTIFACTS_IDENTIFIED, Badges.Badge.ALL_ITEMS_IDENTIFIED);
            LeaveBest(filtered, Badges.Badge.ALL_POTIONS_IDENTIFIED, Badges.Badge.ALL_ITEMS_IDENTIFIED);
            LeaveBest(filtered, Badges.Badge.ALL_SCROLLS_IDENTIFIED, Badges.Badge.ALL_ITEMS_IDENTIFIED);
            LeaveBest(filtered, Badges.Badge.GAMES_PLAYED_1, Badges.Badge.GAMES_PLAYED_2, Badges.Badge.GAMES_PLAYED_3, Badges.Badge.GAMES_PLAYED_4);
            LeaveBest(filtered, Badges.Badge.CHAMPION_1, Badges.Badge.CHAMPION_2, Badges.Badge.CHAMPION_3);

            List<Badges.Badge> list = new List<Badges.Badge>(filtered);
            list.Sort();

            return list;
        }

        private static void LeaveBest(HashSet<Badges.Badge> list, params Badges.Badge[] badges)
        {
            for (int i = badges.Length - 1; i > 0; --i)
            {
                if (list.Contains(badges[i]))
                {
                    for (int j = 0; j < i; ++j)
                        list.Remove(badges[j]);

                    break;
                }
            }
        }
    }
}