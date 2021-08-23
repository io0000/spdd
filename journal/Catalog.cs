using System;
using System.Collections.Generic;
using Microsoft.Collections.Extensions;
using watabou.utils;
using spdd.items.armor;
using spdd.items.artifacts;
using spdd.items.potions;
using spdd.items.rings;
using spdd.items.scrolls;
using spdd.items.wands;
using spdd.items.weapon.melee;

namespace spdd.journal
{
    public enum Catalog
    {
        WEAPONS,
        ARMOR,
        WANDS,
        RINGS,
        ARTIFACTS,
        POTIONS,
        SCROLLS
    }

    public static class CatalogExtensions
    {
        private class CatalogUtil
        {
            public OrderedDictionary<Type, bool> seen = new OrderedDictionary<Type, bool>();

            public CatalogUtil(Type[] types)
            {
                foreach (var type in types)
                {
                    seen.Add(type, false);
                }
            }
        }

        private static CatalogUtil[] table = new CatalogUtil[]
        {
            // WEAPONS
            new CatalogUtil(new Type[] {
                typeof(WornShortsword),
                typeof(Gloves),
                typeof(Dagger),
                typeof(MagesStaff),
                typeof(Shortsword),
                typeof(HandAxe),
                typeof(Spear),
                typeof(Quarterstaff),
                typeof(Dirk),
                typeof(Sword),
                typeof(Mace),
                typeof(Scimitar),
                typeof(RoundShield),
                typeof(Sai),
                typeof(Whip),
                typeof(Longsword),
                typeof(BattleAxe),
                typeof(Flail),
                typeof(RunicBlade),
                typeof(AssassinsBlade),
                typeof(Crossbow),
                typeof(Greatsword),
                typeof(WarHammer),
                typeof(Glaive),
                typeof(Greataxe),
                typeof(Greatshield),
                typeof(Gauntlet)
            }),

            // ARMOR
            new CatalogUtil(new Type[] {
                typeof(ClothArmor),
                typeof(LeatherArmor),
                typeof(MailArmor),
                typeof(ScaleArmor),
                typeof(PlateArmor),
                typeof(WarriorArmor),
                typeof(MageArmor),
                typeof(RogueArmor),
                typeof(HuntressArmor)
            }),

            // WAND
            new CatalogUtil(new Type[] {
                typeof(WandOfMagicMissile),
                typeof(WandOfLightning),
                typeof(WandOfDisintegration),
                typeof(WandOfFireblast),
                typeof(WandOfCorrosion),
                typeof(WandOfBlastWave),
                typeof(WandOfLivingEarth),
                typeof(WandOfFrost),
                typeof(WandOfPrismaticLight),
                typeof(WandOfWarding),
                typeof(WandOfTransfusion),
                typeof(WandOfCorruption),
                typeof(WandOfRegrowth)
            }),

            // RINGS
            new CatalogUtil(new Type[] {
                typeof(RingOfAccuracy),
                typeof(RingOfEnergy),
                typeof(RingOfElements),
                typeof(RingOfEvasion),
                typeof(RingOfForce),
                typeof(RingOfFuror),
                typeof(RingOfHaste),
                typeof(RingOfMight),
                typeof(RingOfSharpshooting),
                typeof(RingOfTenacity),
                typeof(RingOfWealth)
            }),

            //ARTIFACTS
            new CatalogUtil(new Type[] {
                typeof(AlchemistsToolkit),
                //typeof(CapeOfThorns),
                typeof(ChaliceOfBlood),
                typeof(CloakOfShadows),
                typeof(DriedRose),
                typeof(EtherealChains),
                typeof(HornOfPlenty),
                //typeof(LloydsBeacon),
                typeof(MasterThievesArmband),
                typeof(SandalsOfNature),
                typeof(TalismanOfForesight),
                typeof(TimekeepersHourglass),
                typeof(UnstableSpellbook)
            }),

            //POTIONS
            new CatalogUtil(new Type[] {
                typeof(PotionOfHealing),
                typeof(PotionOfStrength),
                typeof(PotionOfLiquidFlame),
                typeof(PotionOfFrost),
                typeof(PotionOfToxicGas),
                typeof(PotionOfParalyticGas),
                typeof(PotionOfPurity),
                typeof(PotionOfLevitation),
                typeof(PotionOfMindVision),
                typeof(PotionOfInvisibility),
                typeof(PotionOfExperience),
                typeof(PotionOfHaste)
            }),

            //SCROLLS
            new CatalogUtil(new Type[] {
                typeof(ScrollOfIdentify),
                typeof(ScrollOfUpgrade),
                typeof(ScrollOfRemoveCurse),
                typeof(ScrollOfMagicMapping),
                typeof(ScrollOfTeleportation),
                typeof(ScrollOfRecharging),
                typeof(ScrollOfMirrorImage),
                typeof(ScrollOfTerror),
                typeof(ScrollOfLullaby),
                typeof(ScrollOfRage),
                typeof(ScrollOfRetribution),
                typeof(ScrollOfTransmutation)
            })
        };

        public static OrderedDictionary<Type, bool> GetSeen(this Catalog cat)
        {
            var index = (int)cat;
            var seen = table[index].seen;

            return seen;
        }

        public static OrderedDictionary<Type, bool>.KeyCollection Items(this Catalog cat)
        {
            var seen = cat.GetSeen();
            return seen.Keys;
        }

        public static bool AllSeen(this Catalog cat)
        {
            var seen = cat.GetSeen();

            foreach (var pair in seen)
            {
                if (!pair.Value)
                {
                    return false;
                }
            }
            return true;
        }

        public static OrderedDictionary<Catalog, Badges.Badge> catalogBadges = new OrderedDictionary<Catalog, Badges.Badge>();

        static CatalogExtensions()
        {
            catalogBadges.Add(Catalog.WEAPONS, Badges.Badge.ALL_WEAPONS_IDENTIFIED);
            catalogBadges.Add(Catalog.ARMOR, Badges.Badge.ALL_ARMOR_IDENTIFIED);
            catalogBadges.Add(Catalog.WANDS, Badges.Badge.ALL_WANDS_IDENTIFIED);
            catalogBadges.Add(Catalog.RINGS, Badges.Badge.ALL_RINGS_IDENTIFIED);
            catalogBadges.Add(Catalog.ARTIFACTS, Badges.Badge.ALL_ARTIFACTS_IDENTIFIED);
            catalogBadges.Add(Catalog.POTIONS, Badges.Badge.ALL_POTIONS_IDENTIFIED);
            catalogBadges.Add(Catalog.SCROLLS, Badges.Badge.ALL_SCROLLS_IDENTIFIED);
        }

        // Type - Class<? extends Item>
        public static bool IsSeen(Type itemClass)
        {
            var values = Enum.GetValues(typeof(Catalog));

            foreach (Catalog cat in values)
            {
                var seen = cat.GetSeen();

                if (seen.ContainsKey(itemClass))
                {
                    return seen[itemClass];
                }
            }
            return false;
        }

        // Class<? extends Item>
        public static void SetSeen(Type itemClass)
        {
            var values = Enum.GetValues(typeof(Catalog));

            foreach (Catalog cat in values)
            {
                var seen = cat.GetSeen();

                if (seen.ContainsKey(itemClass) && !seen[itemClass])
                {
                    seen[itemClass] = true;
                    Journal.saveNeeded = true;
                }
            }
            BadgesExtensions.ValidateItemsIdentified();
        }

        private const string CATALOG_ITEMS = "catalog_items";

        public static void Store(Bundle bundle)
        {
            BadgesExtensions.LoadGlobal();

            List<Type> seen = new List<Type>();

            //if we have identified all items of a set, we use the badge to keep track instead.
            if (!BadgesExtensions.IsUnlocked(Badges.Badge.ALL_ITEMS_IDENTIFIED))
            {
                var values = Enum.GetValues(typeof(Catalog));

                foreach (Catalog cat in values)
                {
                    Badges.Badge badge;
                    catalogBadges.TryGetValue(cat, out badge);

                    if (!BadgesExtensions.IsUnlocked(badge))
                    {
                        var catSeenDic = cat.GetSeen();

                        foreach (var item in cat.Items())
                        {
                            bool catSeen = false;
                            if (catSeenDic.TryGetValue(item, out catSeen) && catSeen == true)
                                seen.Add(item);
                        }
                    }
                }
            }

            bundle.Put(CATALOG_ITEMS, seen.ToArray());
        }

        public static void Restore(Bundle bundle)
        {
            BadgesExtensions.LoadGlobal();

            var values = Enum.GetValues(typeof(Catalog));

            //logic for if we have all badges
            if (BadgesExtensions.IsUnlocked(Badges.Badge.ALL_ITEMS_IDENTIFIED))
            {
                foreach (Catalog cat in values)
                {
                    var seen = cat.GetSeen();

                    foreach (var item in cat.Items())
                    {
                        seen[item] = true;
                    }
                }
                return;
            }

            //catalog-specific badge logic
            foreach (Catalog cat in values)
            {
                Badges.Badge badge;
                catalogBadges.TryGetValue(cat, out badge);

                if (BadgesExtensions.IsUnlocked(badge))
                {
                    var seen = cat.GetSeen();

                    foreach (var item in cat.Items())
                    {
                        seen[item] = true;
                    }
                }
            }
        }

    }  // class CatalogExtensions
}   // namespace
