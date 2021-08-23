using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Collections.Extensions;
using watabou.utils;
using spdd.items.armor;
using spdd.items.artifacts;
using spdd.items.bags;
using spdd.items.food;
using spdd.items.potions;
using spdd.items.rings;
using spdd.items.scrolls;
using spdd.items.stones;
using spdd.items.wands;
using spdd.items.weapon.melee;
using spdd.items.weapon.missiles;
using spdd.plants;

namespace spdd.items
{
    public static class CategoryExtensions
    {
        public static float GetProb(this Generator.Category category)
        {
            int index = (int)category;
            Generator.CategoryUtil cu = Generator.table[index];
            return cu.prob;
        }

        public static Type GetSuperClass(this Generator.Category category)
        {
            int index = (int)category;
            Generator.CategoryUtil cu = Generator.table[index];
            return cu.superClass;
        }

        public static void SetClasses(this Generator.Category category, Type[] classes)
        {
            int index = (int)category;
            Generator.CategoryUtil cu = Generator.table[index];
            cu.classes = classes;
        }

        public static Type[] GetClasses(this Generator.Category category)
        {
            int index = (int)category;
            Generator.CategoryUtil cu = Generator.table[index];
            return cu.classes;
        }

        public static void SetProbs(this Generator.Category category, float[] probs)
        {
            int index = (int)category;
            Generator.CategoryUtil cu = Generator.table[index];
            cu.probs = probs;
        }

        public static float[] GetProbs(this Generator.Category category)
        {
            int index = (int)category;
            Generator.CategoryUtil cu = Generator.table[index];
            return cu.probs;
        }

        public static void SetDefaultProbs(this Generator.Category category, float[] defaultProbs)
        {
            int index = (int)category;
            Generator.CategoryUtil cu = Generator.table[index];
            cu.defaultProbs = defaultProbs;
        }

        public static float[] GetDefaultProbs(this Generator.Category category)
        {
            int index = (int)category;
            Generator.CategoryUtil cu = Generator.table[index];
            return cu.defaultProbs;
        }
    }

    public class Generator
    {
        public enum Category
        {
            WEAPON,
            WEP_T1,
            WEP_T2,
            WEP_T3,
            WEP_T4,
            WEP_T5,

            ARMOR,

            MISSILE,
            MIS_T1,
            MIS_T2,
            MIS_T3,
            MIS_T4,
            MIS_T5,

            WAND,
            RING,
            ARTIFACT,

            FOOD,

            POTION,
            SEED,

            SCROLL,
            STONE,

            GOLD
        }

        public class CategoryUtil
        {
            public Type[] classes;

            //some item types use a deck-based system, where the probs decrement as items are picked
            // until they are all 0, and then they reset. Those generator classes should define
            // defaultProbs. If defaultProbs is null then a deck system isn't used.
            //Artifacts in particular don't reset, no duplicates!
            public float[] probs;
            public float[] defaultProbs;

            //public string name;

            public float prob;
            public Type superClass;

            public CategoryUtil(float prob, Type superClass)
            {
                this.prob = prob;
                this.superClass = superClass;
            }
        }

        public static CategoryUtil[] table = new CategoryUtil[]
        {
            new CategoryUtil(4, typeof(MeleeWeapon)),       // WEAPON
            new CategoryUtil(0, typeof(MeleeWeapon)),       // WEP_T1
            new CategoryUtil(0, typeof(MeleeWeapon)),       // WEP_T2
            new CategoryUtil(0, typeof(MeleeWeapon)),       // WEP_T3
            new CategoryUtil(0, typeof(MeleeWeapon)),       // WEP_T4
            new CategoryUtil(0, typeof(MeleeWeapon)),       // WEP_T5

            new CategoryUtil(3, typeof(Armor)),

            new CategoryUtil(3, typeof(MissileWeapon)),     // MISSILE
            new CategoryUtil(0, typeof(MissileWeapon)),     // MIS_T1
            new CategoryUtil(0, typeof(MissileWeapon)),     // MIS_T2
            new CategoryUtil(0, typeof(MissileWeapon)),     // MIS_T3
            new CategoryUtil(0, typeof(MissileWeapon)),     // MIS_T4
            new CategoryUtil(0, typeof(MissileWeapon)),     // MIS_T5

            new CategoryUtil(2, typeof(Wand)),
            new CategoryUtil(1, typeof(Ring)),
            new CategoryUtil(1, typeof(Artifact)),

            new CategoryUtil(0, typeof(Food)),

            new CategoryUtil(16, typeof(Potion)),
            new CategoryUtil(2, typeof(Plant.Seed)),

            new CategoryUtil(16, typeof(Scroll)),
            new CategoryUtil(2, typeof(Runestone)),

            new CategoryUtil(20, typeof(Gold))
        };

        public static int Order(Item item)
        {
            var i = 0;
            var itemType = item.GetType();

            var values = Enum.GetValues(typeof(Generator.Category));

            // TODO: 비효율적, superClass는 중복된 값이 많다.
            // ex) MeleeWeapon, MeleeWeapon,..., Armor, MissileWeapon, MissileWeapon, ....

            foreach (Generator.Category value in values)
            {
                var superClass = value.GetSuperClass();
                //if (values()[i].superClass.isInstance( item ))
                if (superClass.IsAssignableFrom(itemType))
                    return i;

                ++i;
            }

            return item is Bag ? int.MaxValue : int.MaxValue - 1;
        }

        static Generator()
        {
            Category.GOLD.SetClasses(new[] {
                typeof(Gold)
            });

            Category.GOLD.SetProbs(new float[] { 1 });

            Category.POTION.SetClasses(new[] {
                typeof(PotionOfStrength),   //2 drop every chapter, see Dungeon.posNeeded()
                typeof(PotionOfHealing),
                typeof(PotionOfMindVision),
                typeof(PotionOfFrost),
                typeof(PotionOfLiquidFlame),
                typeof(PotionOfToxicGas),
                typeof(PotionOfHaste),
                typeof(PotionOfInvisibility),
                typeof(PotionOfLevitation),
                typeof(PotionOfParalyticGas),
                typeof(PotionOfPurity),
                typeof(PotionOfExperience)
            });

            var potionDefaultProbs = new float[] { 0, 6, 4, 3, 3, 3, 2, 2, 2, 2, 2, 1 };
            Category.POTION.SetDefaultProbs(potionDefaultProbs);
            Category.POTION.SetProbs((float[])potionDefaultProbs.Clone());

            Category.SEED.SetClasses(new[] {
                typeof(Rotberry.Seed), //quest item
                typeof(Sungrass.Seed),
                typeof(Fadeleaf.Seed),
                typeof(Icecap.Seed),
                typeof(Firebloom.Seed),
                typeof(Sorrowmoss.Seed),
                typeof(Swiftthistle.Seed),
                typeof(Blindweed.Seed),
                typeof(Stormvine.Seed),
                typeof(Earthroot.Seed),
                typeof(Dreamfoil.Seed),
                typeof(Starflower.Seed)
            });

            var seedDefaultProbs = new float[] { 0, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 2 };
            Category.SEED.SetDefaultProbs(seedDefaultProbs);
            Category.SEED.SetProbs((float[])seedDefaultProbs.Clone());

            Category.SCROLL.SetClasses(new[] {
                typeof(ScrollOfUpgrade),    //3 drop every chapter, see Dungeon.souNeeded()
                typeof(ScrollOfIdentify),
                typeof(ScrollOfRemoveCurse),
                typeof(ScrollOfMirrorImage),
                typeof(ScrollOfRecharging),
                typeof(ScrollOfTeleportation),
                typeof(ScrollOfLullaby),
                typeof(ScrollOfMagicMapping),
                typeof(ScrollOfRage),
                typeof(ScrollOfRetribution),
                typeof(ScrollOfTerror),
                typeof(ScrollOfTransmutation)
            });
            var scrollDefaultProbs = new float[] { 0, 6, 4, 3, 3, 3, 2, 2, 2, 2, 2, 1 };
            Category.SCROLL.SetDefaultProbs(scrollDefaultProbs);
            Category.SCROLL.SetProbs((float[])scrollDefaultProbs.Clone());

            Category.STONE.SetClasses(new[] {
                typeof(StoneOfEnchantment),   //1 is guaranteed to drop on floors 6-19
                typeof(StoneOfIntuition),     //1 additional stone is also dropped on floors 1-3
                typeof(StoneOfDisarming),
                typeof(StoneOfFlock),
                typeof(StoneOfShock),
                typeof(StoneOfBlink),
                typeof(StoneOfDeepenedSleep),
                typeof(StoneOfClairvoyance),
                typeof(StoneOfAggression),
                typeof(StoneOfBlast),
                typeof(StoneOfAffection),
                typeof(StoneOfAugmentation)  //1 is sold in each shop
            });

            var stoneDefaultProbs = new float[] { 0, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 0 };
            Category.STONE.SetDefaultProbs(stoneDefaultProbs);
            Category.STONE.SetProbs((float[])stoneDefaultProbs.Clone());

            Category.WAND.SetClasses(new[] {
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
            });
            var wandProbs = new float[] { 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 3, 3, 3 };
            Category.WAND.SetProbs(wandProbs);

            //see generator.randomWeapon
            Category.WEAPON.SetClasses(new Type[] { });
            Category.WEAPON.SetProbs(new float[] { });

            Category.WEP_T1.SetClasses(new[]{
                typeof(WornShortsword),
                typeof(Gloves),
                typeof(Dagger),
                typeof(MagesStaff)
            });
            Category.WEP_T1.SetProbs(new float[] { 1, 1, 1, 0 });

            Category.WEP_T2.SetClasses(new[]{
                typeof(Shortsword),
                typeof(HandAxe),
                typeof(Spear),
                typeof(Quarterstaff),
                typeof(Dirk)
            });
            Category.WEP_T2.SetProbs(new float[] { 6, 5, 5, 4, 4 });

            Category.WEP_T3.SetClasses(new[]{
                typeof(Sword),
                typeof(Mace),
                typeof(Scimitar),
                typeof(RoundShield),
                typeof(Sai),
                typeof(Whip)
            });
            Category.WEP_T3.SetProbs(new float[] { 6, 5, 5, 4, 4, 4 });

            Category.WEP_T4.SetClasses(new[]{
                typeof(Longsword),
                typeof(BattleAxe),
                typeof(Flail),
                typeof(RunicBlade),
                typeof(AssassinsBlade),
                typeof(Crossbow)
            });
            Category.WEP_T4.SetProbs(new float[] { 6, 5, 5, 4, 4, 4 });

            Category.WEP_T5.SetClasses(new[]{
                typeof(Greatsword),
                typeof(WarHammer),
                typeof(Glaive),
                typeof(Greataxe),
                typeof(Greatshield),
                typeof(Gauntlet)
            });
            Category.WEP_T5.SetProbs(new float[] { 6, 5, 5, 4, 4, 4 });

            //see Generator.randomArmor
            Category.ARMOR.SetClasses(new[] {
                typeof(ClothArmor),
                typeof(LeatherArmor),
                typeof(MailArmor),
                typeof(ScaleArmor),
                typeof(PlateArmor)
            });
            Category.ARMOR.SetProbs(new float[] { 0, 0, 0, 0, 0 });

            //see Generator.randomMissile
            Category.MISSILE.SetClasses(new Type[] { });
            Category.MISSILE.SetProbs(new float[] { });

            Category.MIS_T1.SetClasses(new[]{
                typeof(ThrowingStone),
                typeof(ThrowingKnife)
            });
            Category.MIS_T1.SetProbs(new float[] { 6, 5 });

            Category.MIS_T2.SetClasses(new[]{
                typeof(FishingSpear),
                typeof(ThrowingClub),
                typeof(Shuriken)
            });
            Category.MIS_T2.SetProbs(new float[] { 6, 5, 4 });

            Category.MIS_T3.SetClasses(new[]{
                typeof(ThrowingSpear),
                typeof(Kunai),
                typeof(Bolas)
            });
            Category.MIS_T3.SetProbs(new float[] { 6, 5, 4 });

            Category.MIS_T4.SetClasses(new[]{
                typeof(Javelin),
                typeof(Tomahawk),
                typeof(HeavyBoomerang)
            });
            Category.MIS_T4.SetProbs(new float[] { 6, 5, 4 });

            Category.MIS_T5.SetClasses(new[]{
                typeof(Trident),
                typeof(ThrowingHammer),
                typeof(ForceCube)
            });
            Category.MIS_T5.SetProbs(new float[] { 6, 5, 4 });

            Category.FOOD.SetClasses(new[] {
                typeof(Food),
                typeof(Pasty),
                typeof(MysteryMeat)
            });
            Category.FOOD.SetProbs(new float[] { 4, 1, 0 });

            Category.RING.SetClasses(new[] {
                typeof(RingOfAccuracy),
                typeof(RingOfEvasion),
                typeof(RingOfElements),
                typeof(RingOfForce),
                typeof(RingOfFuror),
                typeof(RingOfHaste),
                typeof(RingOfEnergy),
                typeof(RingOfMight),
                typeof(RingOfSharpshooting),
                typeof(RingOfTenacity),
                typeof(RingOfWealth)
            });
            Category.RING.SetProbs(new float[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });

            Category.ARTIFACT.SetClasses(new[]{
                typeof(CapeOfThorns),
                typeof(ChaliceOfBlood),
                typeof(CloakOfShadows),
                typeof(HornOfPlenty),
                typeof(MasterThievesArmband),
                typeof(SandalsOfNature),
                typeof(TalismanOfForesight),
                typeof(TimekeepersHourglass),
                typeof(UnstableSpellbook),
                typeof(AlchemistsToolkit),
                typeof(DriedRose),
                typeof(LloydsBeacon),
                typeof(EtherealChains)
            });

            var artifactDefaultProbs = new float[] { 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1 };
            Category.ARTIFACT.SetDefaultProbs(artifactDefaultProbs);
            Category.ARTIFACT.SetProbs((float[])artifactDefaultProbs.Clone());

            floorSetTierProbs[0] = new float[] { 0, 70, 20, 8, 2 };
            floorSetTierProbs[1] = new float[] { 0, 25, 50, 20, 5 };
            floorSetTierProbs[2] = new float[] { 0, 0, 40, 50, 10 };
            floorSetTierProbs[3] = new float[] { 0, 0, 20, 40, 40 };
            floorSetTierProbs[4] = new float[] { 0, 0, 0, 20, 80 };
        }

        private static float[][] floorSetTierProbs = new float[5][];

        private static OrderedDictionary<Category, float> categoryProbs = new OrderedDictionary<Category, float>();

        public static void FullReset()
        {
            GeneralReset();

            var values = Enum.GetValues(typeof(Category));
            foreach (Category cat in values)
                Reset(cat);
        }

        public static void GeneralReset()
        {
            var values = Enum.GetValues(typeof(Category));

            foreach (Category cat in values)
                categoryProbs[cat] = cat.GetProb();
        }

        public static void Reset(Category cat)
        {
            var defaultProbs = cat.GetDefaultProbs();
            if (defaultProbs != null)
            {
                cat.SetProbs((float[])defaultProbs.Clone());
            }
        }

        public static Item Random()
        {
            Category? catTemp = Rnd.Chances(categoryProbs);
            if (catTemp == null)
            {
                GeneralReset();
                catTemp = Rnd.Chances(categoryProbs);
            }

            Category cat = catTemp.Value;

            var value = categoryProbs[cat] - 1;
            categoryProbs[cat] = value;
            return Random(cat);
        }

        public static Item Random(Category cat)
        {
            switch (cat)
            {
                case Category.ARMOR:
                    return RandomArmor();
                case Category.WEAPON:
                    return RandomWeapon();
                case Category.MISSILE:
                    return RandomMissile();
                case Category.ARTIFACT:
                    Item item = RandomArtifact();
                    //if we're out of artifacts, return a ring instead.
                    return item != null ? item : Random(Category.RING);
                default:
                    {
                        int i = Rnd.Chances(cat.GetProbs());
                        if (i == -1)
                        {
                            Reset(cat);
                            i = Rnd.Chances(cat.GetProbs());
                        }
                        if (cat.GetDefaultProbs() != null)
                        {
                            var probs = cat.GetProbs();
                            --probs[i];
                        }

                        var classes = cat.GetClasses();
                        return ((Item)Reflection.NewInstance(classes[i])).Random();
                    }
            }
        }

        //overrides any deck systems and always uses default probs
        public static Item RandomUsingDefaults(Category cat)
        {
            if (cat.GetDefaultProbs() == null)
            {
                return Random(cat); //currently covers weapons/armor/missiles
            }
            else
            {
                int i = Rnd.Chances(cat.GetDefaultProbs());

                var classes = cat.GetClasses();
                return ((Item)Reflection.NewInstance(classes[i])).Random();
            }
        }

        public static Item Random(Type cl)
        {
            Item item = (Item)Reflection.NewInstance(cl);
            return item.Random();
        }

        public static Armor RandomArmor()
        {
            return RandomArmor(Dungeon.depth / 5);
        }

        public static Armor RandomArmor(int floorSet)
        {
            floorSet = (int)GameMath.Gate(0, floorSet, floorSetTierProbs.Length - 1);

            //Armor a = (Armor)Reflection.newInstance(Category.ARMOR.classes[Random.chances(floorSetTierProbs[floorSet])]);
            int index = Rnd.Chances(floorSetTierProbs[floorSet]);
            var classes = Category.ARMOR.GetClasses();
            Armor a = (Armor)Reflection.NewInstance(classes[index]);

            a.Random();
            return a;
        }

        public static Category[] wepTiers = new Category[]{
            Category.WEP_T1,
            Category.WEP_T2,
            Category.WEP_T3,
            Category.WEP_T4,
            Category.WEP_T5
        };

        public static MeleeWeapon RandomWeapon()
        {
            return RandomWeapon(Dungeon.depth / 5);
        }

        public static MeleeWeapon RandomWeapon(int floorSet)
        {
            floorSet = (int)GameMath.Gate(0, floorSet, floorSetTierProbs.Length - 1);

            int index = Rnd.Chances(floorSetTierProbs[floorSet]);

            Category c = wepTiers[index];

            //MeleeWeapon w = (MeleeWeapon)Reflection.newInstance(c.classes[Random.chances(c.probs)]);
            index = Rnd.Chances(c.GetProbs());
            var classes = c.GetClasses();
            MeleeWeapon w = (MeleeWeapon)Reflection.NewInstance(classes[index]);

            w.Random();
            return w;
        }

        public static Category[] misTiers = new Category[]{
            Category.MIS_T1,
            Category.MIS_T2,
            Category.MIS_T3,
            Category.MIS_T4,
            Category.MIS_T5
        };

        public static MissileWeapon RandomMissile()
        {
            return RandomMissile(Dungeon.depth / 5);
        }

        public static MissileWeapon RandomMissile(int floorSet)
        {
            floorSet = (int)GameMath.Gate(0, floorSet, floorSetTierProbs.Length - 1);

            int index = Rnd.Chances(floorSetTierProbs[floorSet]);

            Category c = misTiers[index];

            //MissileWeapon w = (MissileWeapon)Reflection.newInstance(c.classes[Random.chances(c.probs)]);
            index = Rnd.Chances(c.GetProbs());
            var classes = c.GetClasses();
            MissileWeapon w = (MissileWeapon)Reflection.NewInstance(classes[index]);

            w.Random();
            return w;
        }

        //enforces uniqueness of artifacts throughout a run.
        public static Artifact RandomArtifact()
        {
            Category cat = Category.ARTIFACT;
            int i = Rnd.Chances(cat.GetProbs());

            //if no artifacts are left, return null
            if (i == -1)
                return null;

            var probs = cat.GetProbs();
            --probs[i];

            var classes = cat.GetClasses();
            var artifact = (Artifact)Reflection.NewInstance(classes[i]);
            return (Artifact)artifact.Random();
        }

        public static bool RemoveArtifact(Type artifact)
        {
            Category cat = Category.ARTIFACT;
            var classes = cat.GetClasses();
            var probs = cat.GetProbs();

            for (int i = 0; i < classes.Length; ++i)
            {
                if (classes[i].Equals(artifact))
                {
                    probs[i] = 0;
                    return true;
                }
            }
            return false;
        }

        private const string GENERAL_PROBS = "general_probs";
        private const string CATEGORY_PROBS = "_probs";

        public static void StoreInBundle(Bundle bundle)
        {
            float[] genProbs = categoryProbs.Values.ToArray();
            float[] storeProbs = new float[genProbs.Length];
            for (int i = 0; i < storeProbs.Length; ++i)
            {
                storeProbs[i] = genProbs[i];
            }
            bundle.Put(GENERAL_PROBS, storeProbs);

            var values = Enum.GetValues(typeof(Category));
            foreach (Category cat in values)
            {
                if (cat.GetDefaultProbs() == null)
                    continue;

                var probs = cat.GetProbs();
                var defaultProbs = cat.GetDefaultProbs();

                bool needsStore = false;
                for (int i = 0; i < probs.Length; ++i)
                {
                    if (probs[i] != defaultProbs[i])
                    {
                        needsStore = true;
                        break;
                    }
                }

                if (needsStore)
                {
                    string name = cat.ToString().ToLowerInvariant();
                    var key = name + CATEGORY_PROBS;
                    bundle.Put(key, probs);
                }
            }
        }

        public static void RestoreFromBundle(Bundle bundle)
        {
            FullReset();

            var values = (Category[])Enum.GetValues(typeof(Category));

            if (bundle.Contains(GENERAL_PROBS))
            {
                float[] probs = bundle.GetFloatArray(GENERAL_PROBS);

                for (int i = 0; i < probs.Length; ++i)
                {
                    Category cat = values[i];
                    categoryProbs[cat] = probs[i];
                }
            }

            foreach (Category cat in values)
            {
                var name = cat.ToString().ToLowerInvariant();
                var key = name + CATEGORY_PROBS;
                if (bundle.Contains(key))
                {
                    float[] probs = bundle.GetFloatArray(key);
                    if (cat.GetDefaultProbs() != null && probs.Length == cat.GetDefaultProbs().Length)
                    {
                        cat.SetProbs(probs);
                    }
                }
            }

            ////pre-0.8.1
            //if (bundle.contains("spawned_artifacts"))
            //{
            //    for (Class <? extends Artifact > artifact : bundle.getClassArray("spawned_artifacts"))
            //    {
            //        Category cat = Category.ARTIFACT;
            //        for (int i = 0; i < cat.classes.length; ++i)
            //        {
            //            if (cat.classes[i].equals(artifact))
            //            {
            //                cat.probs[i] = 0;
            //            }
            //        }
            //    }
            //}
        }
    }
}
