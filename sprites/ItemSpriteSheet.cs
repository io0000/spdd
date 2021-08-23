using watabou.noosa;

namespace spdd.sprites
{
    public class ItemSpriteSheet
    {
        private const int WIDTH = 16;
        public const int SIZE = 16;

        public static TextureFilm film = new TextureFilm(Assets.Sprites.ITEMS, SIZE, SIZE);

        private static int XY(int x, int y)
        {
            x -= 1; y -= 1;
            return x + WIDTH * y;
        }

        private static void AssignItemRect(int item, int width, int height)
        {
            int x = (item % WIDTH) * SIZE;
            int y = (item / WIDTH) * SIZE;
            film.Add(item, x, y, x + width, y + height);
        }

        private static int PLACEHOLDERS = XY(1, 1);   //16 slots
        //SOMETHING is the default item sprite at position 0. May show up ingame if there are bugs.
        public static int SOMETHING       = PLACEHOLDERS + 0;
        public static int WEAPON_HOLDER   = PLACEHOLDERS + 1;
        public static int ARMOR_HOLDER    = PLACEHOLDERS + 2;
        public static int MISSILE_HOLDER  = PLACEHOLDERS + 3;
        public static int WAND_HOLDER     = PLACEHOLDERS + 4;
        public static int RING_HOLDER     = PLACEHOLDERS + 5;
        public static int ARTIFACT_HOLDER = PLACEHOLDERS + 6;
        public static int FOOD_HOLDER     = PLACEHOLDERS + 7;
        public static int BOMB_HOLDER     = PLACEHOLDERS + 8;
        public static int POTION_HOLDER   = PLACEHOLDERS + 9;
        public static int SCROLL_HOLDER   = PLACEHOLDERS + 11;
        public static int SEED_HOLDER     = PLACEHOLDERS + 10;
        public static int STONE_HOLDER    = PLACEHOLDERS + 12;
        public static int CATA_HOLDER     = PLACEHOLDERS + 13;
        public static int ELIXIR_HOLDER   = PLACEHOLDERS + 14;
        public static int SPELL_HOLDER    = PLACEHOLDERS + 15;

        private static void AssignRect1()
        {
            AssignItemRect(SOMETHING, 8, 13);
            AssignItemRect(WEAPON_HOLDER, 14, 14);
            AssignItemRect(ARMOR_HOLDER, 14, 12);
            AssignItemRect(MISSILE_HOLDER, 15, 15);
            AssignItemRect(WAND_HOLDER, 14, 14);
            AssignItemRect(RING_HOLDER, 8, 10);
            AssignItemRect(ARTIFACT_HOLDER, 15, 15);
            AssignItemRect(FOOD_HOLDER, 15, 11);
            AssignItemRect(BOMB_HOLDER, 10, 13);
            AssignItemRect(POTION_HOLDER, 12, 14);
            AssignItemRect(SEED_HOLDER, 10, 10);
            AssignItemRect(SCROLL_HOLDER, 15, 14);
            AssignItemRect(STONE_HOLDER, 14, 12);
            AssignItemRect(CATA_HOLDER, 6, 15);
            AssignItemRect(ELIXIR_HOLDER, 12, 14);
            AssignItemRect(SPELL_HOLDER, 8, 16);
        }

        private static int UNCOLLECTIBLE = XY(1, 2);   //16 slots
        public static int GOLD          = UNCOLLECTIBLE + 0;
        public static int DEWDROP       = UNCOLLECTIBLE + 1;
        public static int PETAL         = UNCOLLECTIBLE + 2;
        public static int SANDBAG       = UNCOLLECTIBLE + 3;
        public static int SPIRIT_ARROW  = UNCOLLECTIBLE + 4;
        public static int GUIDE_PAGE    = UNCOLLECTIBLE + 6;
        public static int ALCH_PAGE     = UNCOLLECTIBLE + 7;
        public static int TENGU_BOMB    = UNCOLLECTIBLE + 9;
        public static int TENGU_SHOCKER = UNCOLLECTIBLE + 10;

        private static void AssignRect2()
        {
            AssignItemRect(GOLD, 15, 13);
            AssignItemRect(DEWDROP, 10, 10);
            AssignItemRect(PETAL, 8, 8);
            AssignItemRect(SANDBAG, 10, 10);
            AssignItemRect(SPIRIT_ARROW, 11, 11);
            AssignItemRect(GUIDE_PAGE, 10, 11);
            AssignItemRect(ALCH_PAGE, 10, 11);
            AssignItemRect(TENGU_BOMB, 10, 10);
            AssignItemRect(TENGU_SHOCKER, 10, 10);
        }

        private static int CONTAINERS = XY(1, 3);   //16 slots
        public static int BONES         = CONTAINERS + 0;
        public static int REMAINS       = CONTAINERS + 1;
        public static int TOMB          = CONTAINERS + 2;
        public static int GRAVE         = CONTAINERS + 3;
        public static int CHEST         = CONTAINERS + 4;
        public static int LOCKED_CHEST  = CONTAINERS + 5;
        public static int CRYSTAL_CHEST = CONTAINERS + 6;
        public static int EBONY_CHEST   = CONTAINERS + 7;

        private static void AssignRect3()
        {
            AssignItemRect(BONES, 14, 11);
            AssignItemRect(REMAINS, 14, 11);
            AssignItemRect(TOMB, 14, 15);
            AssignItemRect(GRAVE, 14, 15);
            AssignItemRect(CHEST, 16, 14);
            AssignItemRect(LOCKED_CHEST, 16, 14);
            AssignItemRect(CRYSTAL_CHEST, 16, 14);
            AssignItemRect(EBONY_CHEST, 16, 14);
        }

        private static int SINGLE_USE = XY(1, 4);   //16 slots
        public static int ANKH         = SINGLE_USE + 0;
        public static int STYLUS       = SINGLE_USE + 1;

        public static int SEAL         = SINGLE_USE + 3;
        public static int TORCH        = SINGLE_USE + 4;
        public static int BEACON       = SINGLE_USE + 5;

        public static int HONEYPOT     = SINGLE_USE + 7;
        public static int SHATTPOT     = SINGLE_USE + 8;
        public static int IRON_KEY     = SINGLE_USE + 9;
        public static int GOLDEN_KEY   = SINGLE_USE + 10;
        public static int CRYSTAL_KEY  = SINGLE_USE + 11;
        public static int SKELETON_KEY = SINGLE_USE + 12;
        public static int MASTERY      = SINGLE_USE + 13;
        public static int KIT          = SINGLE_USE + 14;
        public static int AMULET       = SINGLE_USE + 15;

        private static void AssignRect4()
        {
            AssignItemRect(ANKH, 10, 16);
            AssignItemRect(STYLUS, 12, 13);

            AssignItemRect(SEAL, 9, 15);
            AssignItemRect(TORCH, 12, 15);
            AssignItemRect(BEACON, 16, 15);

            AssignItemRect(HONEYPOT, 14, 12);
            AssignItemRect(SHATTPOT, 14, 12);
            AssignItemRect(IRON_KEY, 8, 14);
            AssignItemRect(GOLDEN_KEY, 8, 14);
            AssignItemRect(CRYSTAL_KEY, 8, 14);
            AssignItemRect(SKELETON_KEY, 8, 14);
            AssignItemRect(MASTERY, 13, 16);
            AssignItemRect(KIT, 16, 15);
            AssignItemRect(AMULET, 16, 16);
        }

        private static int BOMBS = XY(1, 5);   //16 slots
        public static int BOMB          = BOMBS + 0;
        public static int DBL_BOMB      = BOMBS + 1;
        public static int FIRE_BOMB     = BOMBS + 2;
        public static int FROST_BOMB    = BOMBS + 3;
        public static int REGROWTH_BOMB = BOMBS + 4;
        public static int FLASHBANG     = BOMBS + 5;
        public static int SHOCK_BOMB    = BOMBS + 6;
        public static int HOLY_BOMB     = BOMBS + 7;
        public static int WOOLY_BOMB    = BOMBS + 8;
        public static int NOISEMAKER    = BOMBS + 9;
        public static int ARCANE_BOMB   = BOMBS + 10;
        public static int SHRAPNEL_BOMB = BOMBS + 11;

        private static void AssignRect5()
        {
            AssignItemRect(BOMB, 10, 13);
            AssignItemRect(DBL_BOMB, 14, 13);
            AssignItemRect(FIRE_BOMB, 13, 12);
            AssignItemRect(FROST_BOMB, 13, 12);
            AssignItemRect(REGROWTH_BOMB, 13, 12);
            AssignItemRect(FLASHBANG, 13, 12);
            AssignItemRect(SHOCK_BOMB, 10, 13);
            AssignItemRect(HOLY_BOMB, 10, 13);
            AssignItemRect(WOOLY_BOMB, 10, 13);
            AssignItemRect(NOISEMAKER, 10, 13);
            AssignItemRect(ARCANE_BOMB, 10, 13);
            AssignItemRect(SHRAPNEL_BOMB, 10, 13);
        }

        private static int WEP_TIER1 = XY(1, 7);   //8 slots
        public static int WORN_SHORTSWORD = WEP_TIER1 + 0;
        public static int CUDGEL          = WEP_TIER1 + 1;
        public static int GLOVES          = WEP_TIER1 + 2;
        public static int RAPIER          = WEP_TIER1 + 3;
        public static int DAGGER          = WEP_TIER1 + 4;
        public static int MAGES_STAFF     = WEP_TIER1 + 5;

        private static void AssignRect6()
        {
            AssignItemRect(WORN_SHORTSWORD, 13, 13);
            AssignItemRect(GLOVES, 12, 16);
            AssignItemRect(DAGGER, 12, 13);
            AssignItemRect(MAGES_STAFF, 15, 16);
        }

        private static int WEP_TIER2 = XY(9, 7);   //8 slots
        public static int SHORTSWORD   = WEP_TIER2 + 0;
        public static int HAND_AXE     = WEP_TIER2 + 1;
        public static int SPEAR        = WEP_TIER2 + 2;
        public static int QUARTERSTAFF = WEP_TIER2 + 3;
        public static int DIRK         = WEP_TIER2 + 4;

        private static void AssignRect7()
        {
            AssignItemRect(SHORTSWORD, 13, 13);
            AssignItemRect(HAND_AXE, 12, 14);
            AssignItemRect(SPEAR, 16, 16);
            AssignItemRect(QUARTERSTAFF, 16, 16);
            AssignItemRect(DIRK, 13, 14);
        }

        private static int WEP_TIER3 = XY(1, 8);   //8 slots
        public static int SWORD        = WEP_TIER3 + 0;
        public static int MACE         = WEP_TIER3 + 1;
        public static int SCIMITAR     = WEP_TIER3 + 2;
        public static int ROUND_SHIELD = WEP_TIER3 + 3;
        public static int SAI          = WEP_TIER3 + 4;
        public static int WHIP         = WEP_TIER3 + 5;

        private static void AssignRect8()
        {
            AssignItemRect(SWORD, 14, 14);
            AssignItemRect(MACE, 15, 15);
            AssignItemRect(SCIMITAR, 13, 16);
            AssignItemRect(ROUND_SHIELD, 16, 16);
            AssignItemRect(SAI, 16, 16);
            AssignItemRect(WHIP, 14, 14);
        }

        private static int WEP_TIER4 = XY(9, 8);   //8 slots
        public static int LONGSWORD       = WEP_TIER4 + 0;
        public static int BATTLE_AXE      = WEP_TIER4 + 1;
        public static int FLAIL           = WEP_TIER4 + 2;
        public static int RUNIC_BLADE     = WEP_TIER4 + 3;
        public static int ASSASSINS_BLADE = WEP_TIER4 + 4;
        public static int CROSSBOW        = WEP_TIER4 + 5;

        private static void AssignRect9()
        {
            AssignItemRect(LONGSWORD, 15, 15);
            AssignItemRect(BATTLE_AXE, 16, 16);
            AssignItemRect(FLAIL, 14, 14);
            AssignItemRect(RUNIC_BLADE, 14, 14);
            AssignItemRect(ASSASSINS_BLADE, 14, 15);
            AssignItemRect(CROSSBOW, 15, 15);
        }

        private static int WEP_TIER5 = XY(1, 9);   //8 slots
        public static int GREATSWORD  = WEP_TIER5 + 0;
        public static int WAR_HAMMER  = WEP_TIER5 + 1;
        public static int GLAIVE      = WEP_TIER5 + 2;
        public static int GREATAXE    = WEP_TIER5 + 3;
        public static int GREATSHIELD = WEP_TIER5 + 4;
        public static int GAUNTLETS   = WEP_TIER5 + 5;

        private static void AssignRect10()
        {
            AssignItemRect(GREATSWORD, 16, 16);
            AssignItemRect(WAR_HAMMER, 16, 16);
            AssignItemRect(GLAIVE, 16, 16);
            AssignItemRect(GREATAXE, 12, 16);
            AssignItemRect(GREATSHIELD, 12, 16);
            AssignItemRect(GAUNTLETS, 13, 15);
        }

        private static int MISSILE_WEP = XY(1, 10);  //16 slots. 3 per tier + boomerang
        public static int SPIRIT_BOW      = MISSILE_WEP + 0;
                                          
        public static int DART            = MISSILE_WEP + 1;
        public static int THROWING_KNIFE  = MISSILE_WEP + 2;
        public static int THROWING_STONE  = MISSILE_WEP + 3;
                                          
        public static int FISHING_SPEAR   = MISSILE_WEP + 4;
        public static int SHURIKEN        = MISSILE_WEP + 5;
        public static int THROWING_CLUB   = MISSILE_WEP + 6;
                                          
        public static int THROWING_SPEAR  = MISSILE_WEP + 7;
        public static int BOLAS           = MISSILE_WEP + 8;
        public static int KUNAI           = MISSILE_WEP + 9;

        public static int JAVELIN         = MISSILE_WEP + 10;
        public static int TOMAHAWK        = MISSILE_WEP + 11;
        public static int BOOMERANG       = MISSILE_WEP + 12;
                                          
        public static int TRIDENT         = MISSILE_WEP + 13;
        public static int THROWING_HAMMER = MISSILE_WEP + 14;
        public static int FORCE_CUBE      = MISSILE_WEP + 15;

        private static void AssignRect11()
        {
            AssignItemRect(SPIRIT_BOW, 16, 16);

            AssignItemRect(DART, 15, 15);
            AssignItemRect(THROWING_KNIFE, 12, 13);
            AssignItemRect(THROWING_STONE, 12, 10);

            AssignItemRect(FISHING_SPEAR, 11, 11);
            AssignItemRect(SHURIKEN, 12, 12);
            AssignItemRect(THROWING_CLUB, 12, 12);

            AssignItemRect(THROWING_SPEAR, 13, 13);
            AssignItemRect(BOLAS, 15, 14);
            AssignItemRect(KUNAI, 15, 15);

            AssignItemRect(JAVELIN, 16, 16);
            AssignItemRect(TOMAHAWK, 13, 13);
            AssignItemRect(BOOMERANG, 14, 14);

            AssignItemRect(TRIDENT, 16, 16);
            AssignItemRect(THROWING_HAMMER, 12, 12);
            AssignItemRect(FORCE_CUBE, 11, 12);
        }

        public static int TIPPED_DARTS = XY(1, 11);  //16 slots
        public static int ROT_DART        = TIPPED_DARTS + 0;
        public static int INCENDIARY_DART = TIPPED_DARTS + 1;
        public static int ADRENALINE_DART = TIPPED_DARTS + 2;
        public static int HEALING_DART    = TIPPED_DARTS + 3;
        public static int CHILLING_DART   = TIPPED_DARTS + 4;
        public static int SHOCKING_DART   = TIPPED_DARTS + 5;
        public static int POISON_DART     = TIPPED_DARTS + 6;
        public static int SLEEP_DART      = TIPPED_DARTS + 7;
        public static int PARALYTIC_DART  = TIPPED_DARTS + 8;
        public static int HOLY_DART       = TIPPED_DARTS + 9;
        public static int DISPLACING_DART = TIPPED_DARTS + 10;
        public static int BLINDING_DART   = TIPPED_DARTS + 11;

        private static void AssignRect12()
        {
            for (int i = TIPPED_DARTS; i < TIPPED_DARTS + 16; ++i)
                AssignItemRect(i, 15, 15);
        }

        private static int ARMOR = XY(1, 12);  //16 slots
        public static int ARMOR_CLOTH    = ARMOR + 0;
        public static int ARMOR_LEATHER  = ARMOR + 1;
        public static int ARMOR_MAIL     = ARMOR + 2;
        public static int ARMOR_SCALE    = ARMOR + 3;
        public static int ARMOR_PLATE    = ARMOR + 4;
        public static int ARMOR_WARRIOR  = ARMOR + 5;
        public static int ARMOR_MAGE     = ARMOR + 6;
        public static int ARMOR_ROGUE    = ARMOR + 7;
        public static int ARMOR_HUNTRESS = ARMOR + 8;

        private static void AssignRect13()
        {
            AssignItemRect(ARMOR_CLOTH, 15, 12);
            AssignItemRect(ARMOR_LEATHER, 14, 13);
            AssignItemRect(ARMOR_MAIL, 14, 12);
            AssignItemRect(ARMOR_SCALE, 14, 11);
            AssignItemRect(ARMOR_PLATE, 12, 12);
            AssignItemRect(ARMOR_WARRIOR, 12, 12);
            AssignItemRect(ARMOR_MAGE, 15, 15);
            AssignItemRect(ARMOR_ROGUE, 14, 12);
            AssignItemRect(ARMOR_HUNTRESS, 13, 15);
        }

        private static int WANDS = XY(1, 14);  //16 slots
        public static int WAND_MAGIC_MISSILE   = WANDS + 0;
        public static int WAND_FIREBOLT        = WANDS + 1;
        public static int WAND_FROST           = WANDS + 2;
        public static int WAND_LIGHTNING       = WANDS + 3;
        public static int WAND_DISINTEGRATION  = WANDS + 4;
        public static int WAND_PRISMATIC_LIGHT = WANDS + 5;
        public static int WAND_CORROSION       = WANDS + 6;
        public static int WAND_LIVING_EARTH    = WANDS + 7;
        public static int WAND_BLAST_WAVE      = WANDS + 8;
        public static int WAND_CORRUPTION      = WANDS + 9;
        public static int WAND_WARDING         = WANDS + 10;
        public static int WAND_REGROWTH        = WANDS + 11;
        public static int WAND_TRANSFUSION     = WANDS + 12;

        private static void AssignRect14()
        {
            for (int i = WANDS; i < WANDS + 16; ++i)
                AssignItemRect(i, 14, 14);
        }

        private static int RINGS = XY(1, 15);  //16 slots
        public static int RING_GARNET     = RINGS + 0;
        public static int RING_RUBY       = RINGS + 1;
        public static int RING_TOPAZ      = RINGS + 2;
        public static int RING_EMERALD    = RINGS + 3;
        public static int RING_ONYX       = RINGS + 4;
        public static int RING_OPAL       = RINGS + 5;
        public static int RING_TOURMALINE = RINGS + 6;
        public static int RING_SAPPHIRE   = RINGS + 7;
        public static int RING_AMETHYST   = RINGS + 8;
        public static int RING_QUARTZ     = RINGS + 9;
        public static int RING_AGATE      = RINGS + 10;
        public static int RING_DIAMOND    = RINGS + 11;

        private static void AssignRect15()
        {
            for (int i = RINGS; i < RINGS + 16; ++i)
                AssignItemRect(i, 8, 10);
        }


        private static int ARTIFACTS = XY(1, 16);  //32 slots
        public static int ARTIFACT_CLOAK     = ARTIFACTS + 0;
        public static int ARTIFACT_ARMBAND   = ARTIFACTS + 1;
        public static int ARTIFACT_CAPE      = ARTIFACTS + 2;
        public static int ARTIFACT_TALISMAN  = ARTIFACTS + 3;
        public static int ARTIFACT_HOURGLASS = ARTIFACTS + 4;
        public static int ARTIFACT_TOOLKIT   = ARTIFACTS + 5;
        public static int ARTIFACT_SPELLBOOK = ARTIFACTS + 6;
        public static int ARTIFACT_BEACON    = ARTIFACTS + 7;
        public static int ARTIFACT_CHAINS    = ARTIFACTS + 8;
        public static int ARTIFACT_HORN1     = ARTIFACTS + 9;
        public static int ARTIFACT_HORN2     = ARTIFACTS + 10;
        public static int ARTIFACT_HORN3     = ARTIFACTS + 11;
        public static int ARTIFACT_HORN4     = ARTIFACTS + 12;
        public static int ARTIFACT_CHALICE1  = ARTIFACTS + 13;
        public static int ARTIFACT_CHALICE2  = ARTIFACTS + 14;
        public static int ARTIFACT_CHALICE3  = ARTIFACTS + 15;
        public static int ARTIFACT_SANDALS   = ARTIFACTS + 16;
        public static int ARTIFACT_SHOES     = ARTIFACTS + 17;
        public static int ARTIFACT_BOOTS     = ARTIFACTS + 18;
        public static int ARTIFACT_GREAVES   = ARTIFACTS + 19;
        public static int ARTIFACT_ROSE1     = ARTIFACTS + 20;
        public static int ARTIFACT_ROSE2     = ARTIFACTS + 21;
        public static int ARTIFACT_ROSE3     = ARTIFACTS + 22;

        private static void AssignRect16()
        {
            AssignItemRect(ARTIFACT_CLOAK, 9, 15);
            AssignItemRect(ARTIFACT_ARMBAND, 16, 13);
            AssignItemRect(ARTIFACT_CAPE, 16, 14);
            AssignItemRect(ARTIFACT_TALISMAN, 15, 13);
            AssignItemRect(ARTIFACT_HOURGLASS, 13, 16);
            AssignItemRect(ARTIFACT_TOOLKIT, 15, 13);
            AssignItemRect(ARTIFACT_SPELLBOOK, 13, 16);
            AssignItemRect(ARTIFACT_BEACON, 16, 16);
            AssignItemRect(ARTIFACT_CHAINS, 16, 16);
            AssignItemRect(ARTIFACT_HORN1, 15, 15);
            AssignItemRect(ARTIFACT_HORN2, 15, 15);
            AssignItemRect(ARTIFACT_HORN3, 15, 15);
            AssignItemRect(ARTIFACT_HORN4, 15, 15);
            AssignItemRect(ARTIFACT_CHALICE1, 12, 15);
            AssignItemRect(ARTIFACT_CHALICE2, 12, 15);
            AssignItemRect(ARTIFACT_CHALICE3, 12, 15);
            AssignItemRect(ARTIFACT_SANDALS, 16, 6);
            AssignItemRect(ARTIFACT_SHOES, 16, 6);
            AssignItemRect(ARTIFACT_BOOTS, 16, 9);
            AssignItemRect(ARTIFACT_GREAVES, 16, 14);
            AssignItemRect(ARTIFACT_ROSE1, 14, 14);
            AssignItemRect(ARTIFACT_ROSE2, 14, 14);
            AssignItemRect(ARTIFACT_ROSE3, 14, 14);
        }

        private static int SCROLLS = XY(1, 19);  //16 slots
        public static int SCROLL_KAUNAN   = SCROLLS + 0;
        public static int SCROLL_SOWILO   = SCROLLS + 1;
        public static int SCROLL_LAGUZ    = SCROLLS + 2;
        public static int SCROLL_YNGVI    = SCROLLS + 3;
        public static int SCROLL_GYFU     = SCROLLS + 4;
        public static int SCROLL_RAIDO    = SCROLLS + 5;
        public static int SCROLL_ISAZ     = SCROLLS + 6;
        public static int SCROLL_MANNAZ   = SCROLLS + 7;
        public static int SCROLL_NAUDIZ   = SCROLLS + 8;
        public static int SCROLL_BERKANAN = SCROLLS + 9;
        public static int SCROLL_ODAL     = SCROLLS + 10;
        public static int SCROLL_TIWAZ    = SCROLLS + 11;
        public static int SCROLL_CATALYST = SCROLLS + 13;

        private static void AssignRect17()
        {
            for (int i = SCROLLS; i < SCROLLS + 16; ++i)
                AssignItemRect(i, 15, 14);
            AssignItemRect(SCROLL_CATALYST, 12, 11);
        }

        private static int EXOTIC_SCROLLS = XY(1, 20);  //16 slots
        public static int EXOTIC_KAUNAN   = EXOTIC_SCROLLS + 0;
        public static int EXOTIC_SOWILO   = EXOTIC_SCROLLS + 1;
        public static int EXOTIC_LAGUZ    = EXOTIC_SCROLLS + 2;
        public static int EXOTIC_YNGVI    = EXOTIC_SCROLLS + 3;
        public static int EXOTIC_GYFU     = EXOTIC_SCROLLS + 4;
        public static int EXOTIC_RAIDO    = EXOTIC_SCROLLS + 5;
        public static int EXOTIC_ISAZ     = EXOTIC_SCROLLS + 6;
        public static int EXOTIC_MANNAZ   = EXOTIC_SCROLLS + 7;
        public static int EXOTIC_NAUDIZ   = EXOTIC_SCROLLS + 8;
        public static int EXOTIC_BERKANAN = EXOTIC_SCROLLS + 9;
        public static int EXOTIC_ODAL     = EXOTIC_SCROLLS + 10;
        public static int EXOTIC_TIWAZ    = EXOTIC_SCROLLS + 11;

        private static void AssignRect18()
        {
            for (int i = EXOTIC_SCROLLS; i < EXOTIC_SCROLLS + 16; ++i)
                AssignItemRect(i, 15, 14);
        }

        private static int STONES = XY(1, 21);  //16 slots
        public static int STONE_AGGRESSION   = STONES + 0;
        public static int STONE_AUGMENTATION = STONES + 1;
        public static int STONE_AFFECTION    = STONES + 2;
        public static int STONE_BLAST        = STONES + 3;
        public static int STONE_BLINK        = STONES + 4;
        public static int STONE_CLAIRVOYANCE = STONES + 5;
        public static int STONE_SLEEP        = STONES + 6;
        public static int STONE_DISARM       = STONES + 7;
        public static int STONE_ENCHANT      = STONES + 8;
        public static int STONE_FLOCK        = STONES + 9;
        public static int STONE_INTUITION    = STONES + 10;
        public static int STONE_SHOCK        = STONES + 11;

        private static void AssignRect19()
        {
            for (int i = STONES; i < STONES + 16; ++i)
                AssignItemRect(i, 14, 12);
        }

        private static int POTIONS = XY(1, 22);  //16 slots
        public static int POTION_CRIMSON   = POTIONS + 0;
        public static int POTION_AMBER     = POTIONS + 1;
        public static int POTION_GOLDEN    = POTIONS + 2;
        public static int POTION_JADE      = POTIONS + 3;
        public static int POTION_TURQUOISE = POTIONS + 4;
        public static int POTION_AZURE     = POTIONS + 5;
        public static int POTION_INDIGO    = POTIONS + 6;
        public static int POTION_MAGENTA   = POTIONS + 7;
        public static int POTION_BISTRE    = POTIONS + 8;
        public static int POTION_CHARCOAL  = POTIONS + 9;
        public static int POTION_SILVER    = POTIONS + 10;
        public static int POTION_IVORY     = POTIONS + 11;
        public static int POTION_CATALYST  = POTIONS + 13;

        private static void AssignRect20()
        {
            for (int i = POTIONS; i < POTIONS + 16; ++i)
                AssignItemRect(i, 12, 14);
            AssignItemRect(POTION_CATALYST, 6, 15);
        }

        private static int EXOTIC_POTIONS = XY(1, 23);  //16 slots
        public static int EXOTIC_CRIMSON   = EXOTIC_POTIONS + 0;
        public static int EXOTIC_AMBER     = EXOTIC_POTIONS + 1;
        public static int EXOTIC_GOLDEN    = EXOTIC_POTIONS + 2;
        public static int EXOTIC_JADE      = EXOTIC_POTIONS + 3;
        public static int EXOTIC_TURQUOISE = EXOTIC_POTIONS + 4;
        public static int EXOTIC_AZURE     = EXOTIC_POTIONS + 5;
        public static int EXOTIC_INDIGO    = EXOTIC_POTIONS + 6;
        public static int EXOTIC_MAGENTA   = EXOTIC_POTIONS + 7;
        public static int EXOTIC_BISTRE    = EXOTIC_POTIONS + 8;
        public static int EXOTIC_CHARCOAL  = EXOTIC_POTIONS + 9;
        public static int EXOTIC_SILVER    = EXOTIC_POTIONS + 10;
        public static int EXOTIC_IVORY     = EXOTIC_POTIONS + 11;

        private static void AssignRect21()
        {
            for (int i = EXOTIC_POTIONS; i < EXOTIC_POTIONS + 16; ++i)
                AssignItemRect(i, 12, 13);
        }

        private static int SEEDS = XY(1, 24);  //16 slots
        public static int SEED_ROTBERRY     = SEEDS + 0;
        public static int SEED_FIREBLOOM    = SEEDS + 1;
        public static int SEED_SWIFTTHISTLE = SEEDS + 2;
        public static int SEED_SUNGRASS     = SEEDS + 3;
        public static int SEED_ICECAP       = SEEDS + 4;
        public static int SEED_STORMVINE    = SEEDS + 5;
        public static int SEED_SORROWMOSS   = SEEDS + 6;
        public static int SEED_DREAMFOIL    = SEEDS + 7;
        public static int SEED_EARTHROOT    = SEEDS + 8;
        public static int SEED_STARFLOWER   = SEEDS + 9;
        public static int SEED_FADELEAF     = SEEDS + 10;
        public static int SEED_BLINDWEED    = SEEDS + 11;

        private static void AssignRect22()
        {
            for (int i = SEEDS; i < SEEDS + 16; ++i)
                AssignItemRect(i, 10, 10);
        }

        private static int BREWS = XY(1, 25);  //8 slots
        public static int BREW_INFERNAL = BREWS + 0;
        public static int BREW_BLIZZARD = BREWS + 1;
        public static int BREW_SHOCKING = BREWS + 2;
        public static int BREW_CAUSTIC  = BREWS + 3;

        private static int ELIXIRS = XY(9, 25);  //8 slots
        public static int ELIXIR_HONEY  = ELIXIRS + 0;
        public static int ELIXIR_AQUA   = ELIXIRS + 1;
        public static int ELIXIR_MIGHT  = ELIXIRS + 2;
        public static int ELIXIR_DRAGON = ELIXIRS + 3;
        public static int ELIXIR_TOXIC  = ELIXIRS + 4;
        public static int ELIXIR_ICY    = ELIXIRS + 5;
        public static int ELIXIR_ARCANE = ELIXIRS + 6;

        private static void AssignRect23()
        {
            for (int i = BREWS; i < BREWS + 16; ++i)
                AssignItemRect(i, 12, 14);
        }

        private static int SPELLS = XY(1, 27);  //16 slots
        public static int MAGIC_PORTER  = SPELLS + 0;
        public static int PHASE_SHIFT   = SPELLS + 1;
        public static int WILD_ENERGY   = SPELLS + 2;
        public static int RETURN_BEACON = SPELLS + 3;

        public static int AQUA_BLAST    = SPELLS + 5;
        public static int FEATHER_FALL  = SPELLS + 6;
        public static int RECLAIM_TRAP  = SPELLS + 7;
                                        
        public static int CURSE_INFUSE  = SPELLS + 9;
        public static int MAGIC_INFUSE  = SPELLS + 10;
        public static int ALCHEMIZE     = SPELLS + 11;
        public static int RECYCLE       = SPELLS + 12;

        private static void AssignRect24()
        {
            AssignItemRect(MAGIC_PORTER, 12, 11);
            AssignItemRect(PHASE_SHIFT, 12, 11);
            AssignItemRect(WILD_ENERGY, 8, 16);
            AssignItemRect(RETURN_BEACON, 8, 16);

            AssignItemRect(AQUA_BLAST, 11, 11);
            AssignItemRect(FEATHER_FALL, 11, 11);
            AssignItemRect(RECLAIM_TRAP, 11, 11);

            AssignItemRect(CURSE_INFUSE, 10, 15);
            AssignItemRect(MAGIC_INFUSE, 10, 15);
            AssignItemRect(ALCHEMIZE, 10, 15);
            AssignItemRect(RECYCLE, 10, 15);
        }

        private static int FOOD = XY(1, 28);  //16 slots
        public static int MEAT         = FOOD + 0;
        public static int STEAK        = FOOD + 1;
        public static int STEWED       = FOOD + 2;
        public static int OVERPRICED   = FOOD + 3;
        public static int CARPACCIO    = FOOD + 4;
        public static int RATION       = FOOD + 5;
        public static int PASTY        = FOOD + 6;
        public static int PUMPKIN_PIE  = FOOD + 7;
        public static int CANDY_CANE   = FOOD + 8;
        public static int MEAT_PIE     = FOOD + 9;
        public static int BLANDFRUIT   = FOOD + 10;
        public static int BLAND_CHUNKS = FOOD + 11;

        private static void AssignRect25()
        {
            AssignItemRect(MEAT, 15, 11);
            AssignItemRect(STEAK, 15, 11);
            AssignItemRect(STEWED, 15, 11);
            AssignItemRect(OVERPRICED, 14, 11);
            AssignItemRect(CARPACCIO, 15, 11);
            AssignItemRect(RATION, 16, 12);
            AssignItemRect(PASTY, 16, 11);
            AssignItemRect(PUMPKIN_PIE, 16, 12);
            AssignItemRect(CANDY_CANE, 13, 16);
            AssignItemRect(MEAT_PIE, 16, 12);
            AssignItemRect(BLANDFRUIT, 9, 12);
            AssignItemRect(BLAND_CHUNKS, 14, 6);
        }

        private static int QUEST = XY(1, 29);  //32 slots
        public static int SKULL   = QUEST + 0;
        public static int DUST    = QUEST + 1;
        public static int CANDLE  = QUEST + 2;
        public static int EMBER   = QUEST + 3;
        public static int PICKAXE = QUEST + 4;
        public static int ORE     = QUEST + 5;
        public static int TOKEN   = QUEST + 6;
        public static int BLOB    = QUEST + 7;
        public static int SHARD   = QUEST + 8;

        private static void AssignRect26()
        {
            AssignItemRect(SKULL, 16, 11);
            AssignItemRect(DUST, 12, 11);
            AssignItemRect(CANDLE, 12, 12);
            AssignItemRect(EMBER, 12, 11);
            AssignItemRect(PICKAXE, 14, 14);
            AssignItemRect(ORE, 15, 15);
            AssignItemRect(TOKEN, 12, 12);
            AssignItemRect(BLOB, 10, 9);
            AssignItemRect(SHARD, 8, 10);
        }

        private static int BAGS = XY(1, 31);  //16 slots
        public static int VIAL      = BAGS + 0;
        public static int POUCH     = BAGS + 1;
        public static int HOLDER    = BAGS + 2;
        public static int BANDOLIER = BAGS + 3;
        public static int HOLSTER   = BAGS + 4;

        private static void AssignRect27()
        {
            AssignItemRect(VIAL, 12, 12);
            AssignItemRect(POUCH, 14, 15);
            AssignItemRect(HOLDER, 16, 16);
            AssignItemRect(BANDOLIER, 15, 16);
            AssignItemRect(HOLSTER, 15, 16);
        }

        static ItemSpriteSheet()
        {
            AssignRect1();
            AssignRect2();
            AssignRect3();
            AssignRect4();
            AssignRect5();
            AssignRect6();
            AssignRect7();
            AssignRect8();
            AssignRect9();
            AssignRect10();
            AssignRect11();
            AssignRect12();
            AssignRect13();
            AssignRect14();
            AssignRect15();
            AssignRect16();
            AssignRect17();
            AssignRect18();
            AssignRect19();
            AssignRect20();
            AssignRect21();
            AssignRect22();
            AssignRect23();
            AssignRect24();
            AssignRect25();
            AssignRect26();
            AssignRect27();
        }

        public class Icons
        {
            private const int WIDTH = 16;
            public const int SIZE = 8;

            public static TextureFilm film = new TextureFilm(Assets.Sprites.ITEM_ICONS, SIZE, SIZE);

            private static int XY(int x, int y)
            {
                x -= 1; y -= 1;
                return x + WIDTH * y;
            }

            private static void AssignIconRect(int item, int width, int height)
            {
                int x = (item % WIDTH) * SIZE;
                int y = (item / WIDTH) * SIZE;
                film.Add(item, x, y, x + width, y + height);
            }

            private static int RINGS = XY(1, 1);  //16 slots
            public static int RING_ACCURACY   = RINGS + 0;
            public static int RING_ELEMENTS   = RINGS + 1;
            public static int RING_ENERGY     = RINGS + 2;
            public static int RING_EVASION    = RINGS + 3;
            public static int RING_FORCE      = RINGS + 4;
            public static int RING_FUROR      = RINGS + 5;
            public static int RING_HASTE      = RINGS + 6;
            public static int RING_MIGHT      = RINGS + 7;
            public static int RING_SHARPSHOOT = RINGS + 8;
            public static int RING_TENACITY   = RINGS + 9;
            public static int RING_WEALTH     = RINGS + 10;
            public static int RING_UNUSED     = RINGS + 11;

            private static void AssignRect1()
            {
                AssignIconRect(RING_ACCURACY, 7, 7);
                AssignIconRect(RING_ELEMENTS, 7, 7);
                AssignIconRect(RING_ENERGY, 7, 5);
                AssignIconRect(RING_EVASION, 7, 7);
                AssignIconRect(RING_FORCE, 5, 6);
                AssignIconRect(RING_FUROR, 7, 6);
                AssignIconRect(RING_HASTE, 6, 6);
                AssignIconRect(RING_MIGHT, 7, 7);
                AssignIconRect(RING_SHARPSHOOT, 7, 7);
                AssignIconRect(RING_TENACITY, 6, 6);
                AssignIconRect(RING_WEALTH, 7, 6);
            }

            private static int SCROLLS = XY(1, 3);  //16 slots
            public static int SCROLL_UPGRADE   = SCROLLS + 0;
            public static int SCROLL_IDENTIFY  = SCROLLS + 1;
            public static int SCROLL_REMCURSE  = SCROLLS + 2;
            public static int SCROLL_MIRRORIMG = SCROLLS + 3;
            public static int SCROLL_RECHARGE  = SCROLLS + 4;
            public static int SCROLL_TELEPORT  = SCROLLS + 5;
            public static int SCROLL_LULLABY   = SCROLLS + 6;
            public static int SCROLL_MAGICMAP  = SCROLLS + 7;
            public static int SCROLL_RAGE      = SCROLLS + 8;
            public static int SCROLL_RETRIB    = SCROLLS + 9;
            public static int SCROLL_TERROR    = SCROLLS + 10;
            public static int SCROLL_TRANSMUTE = SCROLLS + 11;

            private static void AssignRect2()
            {
                AssignIconRect(SCROLL_UPGRADE, 7, 7);
                AssignIconRect(SCROLL_IDENTIFY, 4, 7);
                AssignIconRect(SCROLL_REMCURSE, 7, 7);
                AssignIconRect(SCROLL_MIRRORIMG, 7, 5);
                AssignIconRect(SCROLL_RECHARGE, 7, 5);
                AssignIconRect(SCROLL_TELEPORT, 7, 7);
                AssignIconRect(SCROLL_LULLABY, 7, 6);
                AssignIconRect(SCROLL_MAGICMAP, 7, 7);
                AssignIconRect(SCROLL_RAGE, 6, 6);
                AssignIconRect(SCROLL_RETRIB, 5, 6);
                AssignIconRect(SCROLL_TERROR, 5, 7);
                AssignIconRect(SCROLL_TRANSMUTE, 7, 7);
            }


            private static int EXOTIC_SCROLLS = XY(1, 4);  //16 slots
            public static int SCROLL_ENCHANT   = EXOTIC_SCROLLS + 0;
            public static int SCROLL_DIVINATE  = EXOTIC_SCROLLS + 1;
            public static int SCROLL_ANTIMAGIC = EXOTIC_SCROLLS + 2;
            public static int SCROLL_PRISIMG   = EXOTIC_SCROLLS + 3;
            public static int SCROLL_MYSTENRG  = EXOTIC_SCROLLS + 4;
            public static int SCROLL_PASSAGE   = EXOTIC_SCROLLS + 5;
            public static int SCROLL_AFFECTION = EXOTIC_SCROLLS + 6;
            public static int SCROLL_FORESIGHT = EXOTIC_SCROLLS + 7;
            public static int SCROLL_CONFUSION = EXOTIC_SCROLLS + 8;
            public static int SCROLL_PSIBLAST  = EXOTIC_SCROLLS + 9;
            public static int SCROLL_PETRIF    = EXOTIC_SCROLLS + 10;
            public static int SCROLL_POLYMORPH = EXOTIC_SCROLLS + 11;

            private static void AssignRect3()
            {
                AssignIconRect(SCROLL_ENCHANT, 7, 7);
                AssignIconRect(SCROLL_DIVINATE, 7, 6);
                AssignIconRect(SCROLL_ANTIMAGIC, 7, 7);
                AssignIconRect(SCROLL_PRISIMG, 5, 7);
                AssignIconRect(SCROLL_MYSTENRG, 7, 5);
                AssignIconRect(SCROLL_PASSAGE, 5, 7);
                AssignIconRect(SCROLL_AFFECTION, 7, 6);
                AssignIconRect(SCROLL_FORESIGHT, 7, 5);
                AssignIconRect(SCROLL_CONFUSION, 7, 7);
                AssignIconRect(SCROLL_PSIBLAST, 5, 6);
                AssignIconRect(SCROLL_PETRIF, 7, 5);
                AssignIconRect(SCROLL_POLYMORPH, 7, 6);
            }


            private static int POTIONS = XY(1, 6);  //16 slots
            public static int POTION_STRENGTH = POTIONS + 0;
            public static int POTION_HEALING  = POTIONS + 1;
            public static int POTION_MINDVIS  = POTIONS + 2;
            public static int POTION_FROST    = POTIONS + 3;
            public static int POTION_LIQFLAME = POTIONS + 4;
            public static int POTION_TOXICGAS = POTIONS + 5;
            public static int POTION_HASTE    = POTIONS + 6;
            public static int POTION_INVIS    = POTIONS + 7;
            public static int POTION_LEVITATE = POTIONS + 8;
            public static int POTION_PARAGAS  = POTIONS + 9;
            public static int POTION_PURITY   = POTIONS + 10;
            public static int POTION_EXP      = POTIONS + 11;

            private static void AssignRect4()
            {
                AssignIconRect(POTION_STRENGTH, 7, 7);
                AssignIconRect(POTION_HEALING, 6, 7);
                AssignIconRect(POTION_MINDVIS, 7, 5);
                AssignIconRect(POTION_FROST, 7, 7);
                AssignIconRect(POTION_LIQFLAME, 5, 7);
                AssignIconRect(POTION_TOXICGAS, 7, 7);
                AssignIconRect(POTION_HASTE, 6, 6);
                AssignIconRect(POTION_INVIS, 5, 7);
                AssignIconRect(POTION_LEVITATE, 6, 7);
                AssignIconRect(POTION_PARAGAS, 7, 7);
                AssignIconRect(POTION_PURITY, 5, 7);
                AssignIconRect(POTION_EXP, 7, 7);
            }

            private static int EXOTIC_POTIONS = XY(1, 7);  //16 slots
            public static int POTION_ARENSURGE = EXOTIC_POTIONS + 0;
            public static int POTION_SHIELDING = EXOTIC_POTIONS + 1;
            public static int POTION_MAGISIGHT = EXOTIC_POTIONS + 2;
            public static int POTION_SNAPFREEZ = EXOTIC_POTIONS + 3;
            public static int POTION_DRGBREATH = EXOTIC_POTIONS + 4;
            public static int POTION_CORROGAS  = EXOTIC_POTIONS + 5;
            public static int POTION_STAMINA   = EXOTIC_POTIONS + 6;
            public static int POTION_SHROUDFOG = EXOTIC_POTIONS + 7;
            public static int POTION_STRMCLOUD = EXOTIC_POTIONS + 8;
            public static int POTION_EARTHARMR = EXOTIC_POTIONS + 9;
            public static int POTION_CLEANSE   = EXOTIC_POTIONS + 10;
            public static int POTION_HOLYFUROR = EXOTIC_POTIONS + 11;

            private static void AssignRect5()
            {
                AssignIconRect(POTION_ARENSURGE, 7, 7);
                AssignIconRect(POTION_SHIELDING, 6, 6);
                AssignIconRect(POTION_MAGISIGHT, 7, 5);
                AssignIconRect(POTION_SNAPFREEZ, 7, 7);
                AssignIconRect(POTION_DRGBREATH, 7, 7);
                AssignIconRect(POTION_CORROGAS, 7, 7);
                AssignIconRect(POTION_STAMINA, 6, 6);
                AssignIconRect(POTION_SHROUDFOG, 7, 7);
                AssignIconRect(POTION_STRMCLOUD, 7, 7);
                AssignIconRect(POTION_EARTHARMR, 6, 6);
                AssignIconRect(POTION_CLEANSE, 7, 7);
                AssignIconRect(POTION_HOLYFUROR, 5, 7);
            }

            static Icons()
            {
                AssignRect1();
                AssignRect2();
                AssignRect3();
                AssignRect4();
                AssignRect5();
            }
        } // Icons
    }
}