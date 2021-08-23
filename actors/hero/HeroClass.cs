using watabou.utils;
using spdd.items;
using spdd.items.armor;
using spdd.items.food;
using spdd.items.potions;
using spdd.items.scrolls;
using spdd.items.wands;
using spdd.items.weapon;
using spdd.items.weapon.melee;
using spdd.items.weapon.missiles;
using spdd.items.artifacts;
using spdd.items.bags;
using spdd.messages;

// tt
using spdd.items.scrolls.exotic;
using spdd.items.weapon.missiles.darts;
using spdd.items.stones;
using spdd.items.potions.brews;
using spdd.items.potions.elixirs;
using spdd.items.potions.exotic;
using spdd.items.spells;
using spdd.items.bombs;
using spdd.plants;
using spdd.items.rings;
using System;
//

namespace spdd.actors.hero
{
    public enum HeroClass
    {
        WARRIOR,
        MAGE,
        ROGUE,
        HUNTRESS
    }

    public static class HeroClassExtensions
    {
        private class HeroClassUtil
        {
            internal string title;
            internal int ordinal;
            internal HeroSubClass[] subClasses;
            internal string spritesheet;
            internal string splash;
            internal Badges.Badge badge;
            //internal string[] perks;

            public HeroClassUtil(string title, int ordinal, HeroSubClass[] subClasses, string spritesheet, string splash, Badges.Badge badge)
            {
                this.title = title;
                this.ordinal = ordinal;
                this.subClasses = subClasses;
                this.spritesheet = spritesheet;
                this.splash = splash;
                this.badge = badge;
                //this.perks = perks;
            }
        }

        private static HeroClassUtil[] table = new HeroClassUtil[]
        {
            new HeroClassUtil("warrior",
                0,
                new HeroSubClass[] { HeroSubClass.BERSERKER, HeroSubClass.GLADIATOR },
                Assets.Sprites.WARRIOR,
                Assets.Splashes.WARRIOR,
                Badges.Badge.MASTERY_WARRIOR),

            new HeroClassUtil("mage",
                1,
                new HeroSubClass[] { HeroSubClass.BATTLEMAGE, HeroSubClass.WARLOCK },
                Assets.Sprites.MAGE,
                Assets.Splashes.MAGE,
                Badges.Badge.MASTERY_MAGE),

            new HeroClassUtil("rogue",
                2,
                new HeroSubClass[] { HeroSubClass.ASSASSIN, HeroSubClass.FREERUNNER },
                Assets.Sprites.ROGUE,
                Assets.Splashes.ROGUE,
                Badges.Badge.MASTERY_ROGUE),

            new HeroClassUtil("huntress",
                3,
                new HeroSubClass[] { HeroSubClass.SNIPER, HeroSubClass.WARDEN },
                Assets.Sprites.HUNTRESS,
                Assets.Splashes.HUNTRESS,
                Badges.Badge.MASTERY_HUNTRESS)
        };


        public static void InitHero(this HeroClass heroClass, Hero hero)
        {
            hero.heroClass = heroClass;

            InitCommon(hero);

            switch (heroClass)
            {
                case HeroClass.WARRIOR:
                    InitWarrior(hero);
                    break;

                case HeroClass.MAGE:
                    InitMage(hero);
                    break;

                case HeroClass.ROGUE:
                    InitRogue(hero);
                    break;

                case HeroClass.HUNTRESS:
                    InitHuntress(hero);
                    break;
            }
        }

        private static void InitCommon(Hero hero)
        {
            Item i = new ClothArmor().Identify();
            if (!Challenges.IsItemBlocked(i))
                hero.belongings.armor = (ClothArmor)i;

            i = new Food();
            if (!Challenges.IsItemBlocked(i))
                i.Collect();

            if (Dungeon.IsChallenged(Challenges.NO_FOOD))
            {
                var sr = new SmallRation();
                sr.Collect();
            }

            var si = new ScrollOfIdentify();
            si.Identify();
        }

        public static Badges.Badge MasteryBadge(this HeroClass heroClass)
        {
            var index = (int)heroClass;
            return table[index].badge;
        }

        private static void InitWarrior(Hero hero)
        {
            (hero.belongings.weapon = new WornShortsword()).Identify();
            ThrowingStone stones = new ThrowingStone();
            stones.Quantity(3).Collect();
            Dungeon.quickslot.SetSlot(0, stones);

            if (hero.belongings.armor != null)
            {
                hero.belongings.armor.AffixSeal(new BrokenSeal());
            }

            new PotionBandolier().Collect();
            Dungeon.LimitedDrops.POTION_BANDOLIER.Drop();

            new PotionOfHealing().Identify();
            new ScrollOfRage().Identify();

            //Test(HeroClass.WARRIOR);  //tt
        }

        private static void Test(HeroClass heroClass)
        {
            {
                // bag
                {
                    if (heroClass != HeroClass.MAGE)
                    {
                        new ScrollHolder().Collect();
                        Dungeon.LimitedDrops.SCROLL_HOLDER.Drop();
                    }

                    if (heroClass != HeroClass.HUNTRESS && heroClass != HeroClass.ROGUE)
                    {
                        new VelvetPouch().Collect();
                        Dungeon.LimitedDrops.VELVET_POUCH.Drop();
                    }

                    if (heroClass != HeroClass.WARRIOR)
                    {
                        new PotionBandolier().Collect();
                        Dungeon.LimitedDrops.POTION_BANDOLIER.Drop();
                    }

                    new MagicalHolster().Collect();
                    Dungeon.LimitedDrops.MAGICAL_HOLSTER.Drop();
                }

                {
                    new TomeOfMastery().Collect();
                    new ArmorKit().Collect();

                    new PotionOfExperience().Quantity(99).Identify().Collect();
                    new PotionOfHealing().Quantity(99).Identify().Collect();
                    new PotionOfStrength().Quantity(99).Identify().Collect();
                    new PotionOfLevitation().Quantity(99).Identify().Collect();

                    //new MailArmor().Identify().Collect();
                    new Honeypot().Collect();
                    new MerchantsBeacon().Quantity(99).Collect();

                    new StoneOfAugmentation().Identify().Quantity(99).Collect();
                    new Bomb().Quantity(99).Collect();
                    new MysteryMeat().Quantity(99).Collect();

                    new CurseInfusion().Quantity(99).Collect();

                    new ScrollOfIdentify().Quantity(99).Identify().Collect();
                    new ScrollOfMagicMapping().Quantity(99).Identify().Collect();
                    new ScrollOfUpgrade().Quantity(99).Identify().Collect();
                    new ScrollOfEnchantment().Quantity(99).Identify().Collect();
                    new ScrollOfLullaby().Quantity(99).Identify().Collect();
                    
                    //new ElixirOfMight().Quantity(99).Identify().Collect();

                    new StoneOfDisarming().Quantity(99).Identify().Collect();

                    //new DriedRose().Identify().Collect();
                    new EtherealChains().Identify().Collect();
                    new ChaliceOfBlood().Identify().Collect();
                }

                // bomb
                {
                    //var types = new System.Type[]
                    //{
                    //    typeof(ArcaneBomb),
                    //    typeof(Firebomb),
                    //    typeof(Flashbang),
                    //    typeof(FrostBomb),
                    //    typeof(HolyBomb),
                    //    typeof(Noisemaker),
                    //    typeof(RegrowthBomb),
                    //    typeof(ShockBomb),
                    //    typeof(ShrapnelBomb),
                    //    typeof(WoollyBomb)
                    //};
                    //
                    //foreach (var type in types)
                    //{
                    //    var bomb = (Item)Activator.CreateInstance(type);
                    //    bomb.Quantity(99);
                    //    bomb.Collect();
                    //}
                }

                // ring
                {
                    var classes = Generator.Category.RING.GetClasses();
                    foreach (var type in classes)
                    {
                        var ring = (Item)Activator.CreateInstance(type);
                        ring.Identify().Collect();
                    }

                    {
                        //    for (int i = 0; i < 20; ++i)
                        //    {
                        //        //var item = RingOfWealth.GenEquipmentDrop(i);
                        //        //item.Collect();
                        //        //var item2 = RingOfWealth.GenLowValueConsumable();
                        //        //item2.Collect();
                        //        //var item3 = RingOfWealth.GenMidValueConsumable();
                        //        //item3.Collect();
                        //        var item4 = RingOfWealth.GenHighValueConsumable();
                        //        item4.Collect();
                        //    }
                    }

                }

                // job, armor
                {
                    //new TomeOfMastery().Collect();
                    //new ArmorKit().Collect();
                    //
                    //new ClothArmor().Identify().Collect();
                    //new LeatherArmor().Identify().Collect();
                    //new MailArmor().Identify().Collect();
                    //new PlateArmor().Identify().Collect();
                    //new ScaleArmor().Identify().Collect();
                }

                // food
                {
                    //new ChargrilledMeat().Quantity(99).Collect();
                    //new FrozenCarpaccio().Quantity(99).Collect();
                    //new MeatPie().Quantity(99).Collect();
                    //new MysteryMeat().Quantity(99).Collect();
                    //new Pasty().Quantity(99).Collect();
                    //new SmallRation().Quantity(99).Collect();
                    //new StewedMeat().Quantity(99).Collect();
                }

                // wand
                {
                    var wand = new WandOfFireblast();
                    wand.Identify().Collect();

                    var wand2 = new WandOfFrost();
                    wand2.Identify().Collect();

                    var wand3 = new WandOfBlastWave();
                    wand3.Identify().Collect();

                    var wand4 = new WandOfRegrowth();
                    wand4.Identify().Collect();
                }

                // scroll
                {
                    //new ScrollOfMirrorImage().Quantity(99).Identify().Collect();
                    //new ScrollOfIdentify().Quantity(99).Identify().Collect();
                    //new ScrollOfLullaby().Quantity(99).Identify().Collect();
                    //new ScrollOfMagicMapping().Quantity(99).Identify().Collect();
                    //new ScrollOfRage().Quantity(99).Identify().Collect();
                    //new ScrollOfRecharging().Quantity(99).Identify().Collect();
                    //new ScrollOfRemoveCurse().Quantity(99).Identify().Collect();                    
                    //new ScrollOfRetribution().Quantity(99).Identify().Collect();
                    //new ScrollOfTeleportation().Quantity(99).Identify().Collect();
                    //new ScrollOfTerror().Quantity(99).Identify().Collect();
                    //new ScrollOfTransmutation().Quantity(99).Identify().Collect();
                    //new ScrollOfUpgrade().Quantity(99).Identify().Collect();

                    //new ScrollOfAffection().Identify().Collect();
                    //new ScrollOfAntiMagic().Identify().Collect();
                    //new ScrollOfConfusion().Identify().Collect();
                    //new ScrollOfDivination().Identify().Collect();
                    //new ScrollOfEnchantment().Quantity(99).Identify().Collect();
                    //new ScrollOfForesight().Identify().Collect();
                    //new ScrollOfMysticalEnergy().Identify().Collect();
                    //new ScrollOfPassage().Identify().Collect();
                    //new ScrollOfPetrification().Identify().Collect();
                    //new ScrollOfPolymorph().Identify().Collect();
                    //new ScrollOfPrismaticImage().Quantity(99).Identify().Collect();
                    //new ScrollOfPsionicBlast().Identify().Collect();
                }

                // stone
                {
                    //var classes = Generator.Category.STONE.GetClasses();
                    //foreach (var type in classes)
                    //{
                    //    var stone = (Runestone)Activator.CreateInstance(type);
                    //    stone.Quantity(99);
                    //    stone.Identify().Collect();
                    //}
                }

                // artifact
                {
                    //var classes = Generator.Category.ARTIFACT.GetClasses();
                    //foreach (var type in classes)
                    //{
                    //    var artifact = (Item)Activator.CreateInstance(type);
                    //    artifact.Identify().Collect();
                    //}
                }

                // potion
                {
                    // brew
                    {
                        //var types = new System.Type[]
                        //{
                        //    typeof(BlizzardBrew),
                        //    typeof(CausticBrew),
                        //    typeof(InfernalBrew),
                        //    typeof(ShockingBrew)
                        //};
                        //
                        //foreach (var type in types)
                        //{
                        //    var brew = (Item)Activator.CreateInstance(type);
                        //    brew.Quantity(99);
                        //    brew.Collect();
                        //}
                    }

                    // elixir
                    {
                        //    var types = new System.Type[]
                        //    {
                        //        typeof(ElixirOfAquaticRejuvenation),
                        //        typeof(ElixirOfArcaneArmor),
                        //        typeof(ElixirOfDragonsBlood),
                        //        typeof(ElixirOfHoneyedHealing),
                        //        typeof(ElixirOfIcyTouch),
                        //        typeof(ElixirOfMight),
                        //        typeof(ElixirOfToxicEssence)
                        //    };
                        //
                        //    foreach (var type in types)
                        //    {
                        //        var elixir = (Item)Activator.CreateInstance(type);
                        //        elixir.Quantity(99);
                        //        elixir.Collect();
                        //    }
                        //
                        //    new Honeypot().Quantity(99).Collect();
                    }

                    // ExoticPotion
                    {
                        //var types = new System.Type[]
                        //{
                        //    typeof(PotionOfAdrenalineSurge),
                        //    typeof(PotionOfCleansing),
                        //    typeof(PotionOfCorrosiveGas),
                        //    typeof(PotionOfDragonsBreath),
                        //    typeof(PotionOfEarthenArmor),
                        //    typeof(PotionOfHolyFuror),
                        //    typeof(PotionOfMagicalSight),
                        //    typeof(PotionOfShielding),
                        //    typeof(PotionOfShroudingFog),
                        //    typeof(PotionOfSnapFreeze),
                        //    typeof(PotionOfStamina),
                        //    typeof(PotionOfStormClouds)
                        //};
                        //
                        //foreach (var type in types)
                        //{
                        //    var exoticPotion = (Item)Activator.CreateInstance(type);
                        //    exoticPotion.Quantity(99);
                        //    exoticPotion.Identify();
                        //    exoticPotion.Collect();
                        //}
                        //
                        //new PotionOfHealing().Quantity(99).Identify().Collect();
                    }

                    // normal
                    {
                        //var classes = Generator.Category.POTION.GetClasses();
                        //foreach (var type in classes)
                        //{
                        //    var potion = (Item)Activator.CreateInstance(type);
                        //    potion.Quantity(99);
                        //    potion.Identify().Collect();
                        //    //potion.Collect();
                        //}
                        //
                        //var catalyst = new AlchemicalCatalyst();
                        //catalyst.Quantity(99).Collect();                        
                    }
                }

                // seed
                {
                    //var types = new System.Type[]
                    //{
                    //    typeof(BlandfruitBush.Seed),
                    //    typeof(Blindweed.Seed),
                    //    typeof(Dreamfoil.Seed),
                    //    typeof(Earthroot.Seed),
                    //    typeof(Fadeleaf.Seed),
                    //    typeof(Firebloom.Seed),
                    //    typeof(Icecap.Seed),
                    //    typeof(Rotberry.Seed),
                    //    typeof(Sorrowmoss.Seed),
                    //    typeof(Starflower.Seed),
                    //    typeof(Stormvine.Seed),
                    //    typeof(Sungrass.Seed),
                    //    typeof(Swiftthistle.Seed)
                    //};
                    //
                    //foreach (var type in types)
                    //{
                    //    var seed = (Item)Activator.CreateInstance(type);
                    //    seed.Quantity(99);
                    //    seed.Collect();
                    //}
                }

                // spells
                {
                    //    var types = new System.Type[]
                    //    {
                    //        typeof(Alchemize),
                    //        typeof(AquaBlast),
                    //        typeof(ArcaneCatalyst),
                    //        typeof(BeaconOfReturning),
                    //        typeof(CurseInfusion),
                    //        typeof(FeatherFall),
                    //        typeof(MagicalInfusion),
                    //        typeof(MagicalPorter),
                    //        typeof(PhaseShift),
                    //        typeof(ReclaimTrap),
                    //        typeof(Recycle),
                    //        typeof(WildEnergy)
                    //    };
                    //    foreach (var type in types)
                    //    {
                    //        var spell = (Item)Activator.CreateInstance(type);
                    //        spell.Quantity(99);
                    //        spell.Collect();
                    //    }
                }

                // weapon
                {
                    // melee
                    {
                        //var types = new System.Type[]
                        //{
                        //    //typeof(AssassinsBlade),
                        //    //typeof(BattleAxe),
                        //    //typeof(Crossbow),
                        //    //typeof(Dagger),
                        //    //typeof(Dirk),
                        //    //typeof(Flail),
                        //    //typeof(Gauntlet),
                        //    //typeof(Glaive),
                        //    //typeof(Gloves),
                        //    //typeof(Greataxe),
                        //
                        //    typeof(Quarterstaff),
                        //    typeof(RoundShield),
                        //    typeof(RunicBlade),
                        //    typeof(Sai),
                        //    typeof(Scimitar),
                        //    typeof(Shortsword),
                        //    typeof(Spear),
                        //    typeof(Sword),
                        //    typeof(WarHammer),
                        //    typeof(Whip),
                        //    typeof(WornShortsword)
                        //};
                        //foreach (var type in types)
                        //{
                        //    var weapon = (Item)Activator.CreateInstance(type);
                        //    weapon.Identify().Collect();
                        //}
                    }

                    // missiles
                    {
                        //// darts
                        //{
                        //    new Crossbow().Identify().Collect();
                        //
                        //    var types = new System.Type[]
                        //    {
                        //        typeof(AdrenalineDart),
                        //        typeof(BlindingDart),
                        //        typeof(ChillingDart),
                        //        typeof(DisplacingDart),
                        //        typeof(HealingDart),
                        //        typeof(HolyDart),
                        //        typeof(IncendiaryDart),
                        //        typeof(ParalyticDart),
                        //        typeof(PoisonDart),
                        //        typeof(RotDart),
                        //        typeof(ShockingDart),
                        //        typeof(SleepDart)
                        //    };
                        //    foreach (var type in types)
                        //    {
                        //        var dart = (Item)Activator.CreateInstance(type);
                        //        dart.Identify().Quantity(99).Collect();
                        //    }
                        //}
                    }

                    {
                        var types = new System.Type[]
                        {
                            typeof(Bolas),
                            typeof(FishingSpear),
                            typeof(ForceCube),
                            typeof(HeavyBoomerang),
                            typeof(Javelin),
                            typeof(Kunai),
                            typeof(Shuriken),
                            typeof(ThrowingClub),
                            typeof(ThrowingHammer),
                            typeof(ThrowingKnife),
                            typeof(ThrowingSpear),
                            typeof(ThrowingStone),
                            typeof(Tomahawk),
                            typeof(Trident)
                        };
                        foreach (var type in types)
                        {
                            var missileWeapon = (Item)Activator.CreateInstance(type);
                            missileWeapon.Identify().Quantity(99).Collect();
                        }
                    }
                }
            }
        }

        private static void InitMage(Hero hero)
        {
            var staff = new MagesStaff(new WandOfMagicMissile());

            (hero.belongings.weapon = staff).Identify();
            hero.belongings.weapon.Activate(hero);

            Dungeon.quickslot.SetSlot(0, staff);

            new ScrollHolder().Collect();
            Dungeon.LimitedDrops.SCROLL_HOLDER.Drop();

            new ScrollOfUpgrade().Identify();
            new PotionOfLiquidFlame().Identify();

            //Test(HeroClass.MAGE); //tt
        }

        private static void InitRogue(Hero hero)
        {
            (hero.belongings.weapon = new Dagger()).Identify();

            CloakOfShadows cloak = new CloakOfShadows();
            (hero.belongings.artifact = cloak).Identify();
            hero.belongings.artifact.Activate(hero);

            ThrowingKnife knives = new ThrowingKnife();
            knives.Quantity(3).Collect();

            Dungeon.quickslot.SetSlot(0, cloak);
            Dungeon.quickslot.SetSlot(1, knives);

            new VelvetPouch().Collect();
            Dungeon.LimitedDrops.VELVET_POUCH.Drop();

            new ScrollOfMagicMapping().Identify();
            new PotionOfInvisibility().Identify();

            //Test(HeroClass.ROGUE);    //tt
        }

        private static void InitHuntress(Hero hero)
        {
            (hero.belongings.weapon = new Gloves()).Identify();
            SpiritBow bow = new SpiritBow();
            bow.Identify().Collect();

            Dungeon.quickslot.SetSlot(0, bow);

            new VelvetPouch().Collect();
            Dungeon.LimitedDrops.VELVET_POUCH.Drop();

            new PotionOfMindVision().Identify();
            new ScrollOfLullaby().Identify();

            //Test(HeroClass.HUNTRESS);     //tt
        }

        public static string Title(this HeroClass heroClass)
        {
            var index = (int)heroClass;
            var key = table[index].title;
            return Messages.Get(typeof(HeroClass), key);
        }

        public static HeroSubClass[] SubClasses(this HeroClass heroClass)
        {
            var index = (int)heroClass;
            return table[index].subClasses;
        }

        public static string Spritesheet(this HeroClass heroClass)
        {
            var index = (int)heroClass;
            return table[index].spritesheet;
        }

        public static string SplashArt(this HeroClass heroClass)
        {
            var index = (int)heroClass;
            return table[index].splash;
        }

        //public static string[] Perks(this HeroClass heroClass)
        //{
        //    var index = (int)heroClass;
        //    return table[index].perks;
        //}

        public static bool IsUnlocked(this HeroClass heroClass)
        {
            //always unlock on debug builds
            if (DeviceCompat.IsDebug())
                return true;

            switch (heroClass)
            {
                default:
                case HeroClass.WARRIOR:
                    return true;
                case HeroClass.MAGE:
                    return BadgesExtensions.IsUnlocked(Badges.Badge.UNLOCK_MAGE);
                case HeroClass.ROGUE:
                    return BadgesExtensions.IsUnlocked(Badges.Badge.UNLOCK_ROGUE);
                case HeroClass.HUNTRESS:
                    return BadgesExtensions.IsUnlocked(Badges.Badge.UNLOCK_HUNTRESS);
            }
        }

        public static int Ordinal(this HeroClass heroClass)
        {
            var index = (int)heroClass;
            return table[index].ordinal;
        }

        public static string UnlockMsg(this HeroClass heroClass)
        {
            switch (heroClass)
            {
                default:
                case HeroClass.WARRIOR:
                    return "";
                case HeroClass.MAGE:
                    return Messages.Get(typeof(HeroClass), "mage_unlock");
                case HeroClass.ROGUE:
                    return Messages.Get(typeof(HeroClass), "rogue_unlock");
                case HeroClass.HUNTRESS:
                    return Messages.Get(typeof(HeroClass), "huntress_unlock");
            }
        }

        private const string CLASS = "class";

        public static void StoreInBundle(this HeroClass heroClass, Bundle bundle)
        {
            bundle.Put(CLASS, heroClass.ToString());    // enum
        }

        public static HeroClass RestoreInBundle(Bundle bundle)
        {
            var type = bundle.GetEnum<HeroClass>(CLASS);
            return type;
        }
    }
}