using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.actors.blobs;
using spdd.journal;
using spdd.scenes;
using spdd.levels;
using spdd.plants;
using spdd.items.bags;
using spdd.items.potions.exotic;
using spdd.items.potions.elixirs;
using spdd.sprites;
using spdd.utils;
using spdd.windows;
using spdd.messages;

namespace spdd.items.potions
{
    public class Potion : Item
    {
        public const string AC_DRINK = "DRINK";

        //used internally for potions that can be drunk or thrown
        public const string AC_CHOOSE = "CHOOSE";

        private const float TIME_TO_DRINK = 1f;

        private static Type[] potions = {
            typeof(PotionOfHealing),
            typeof(PotionOfExperience),
            typeof(PotionOfToxicGas),
            typeof(PotionOfLiquidFlame),
            typeof(PotionOfStrength),
            typeof(PotionOfParalyticGas),
            typeof(PotionOfLevitation),
            typeof(PotionOfMindVision),
            typeof(PotionOfPurity),
            typeof(PotionOfInvisibility),
            typeof(PotionOfHaste),
            typeof(PotionOfFrost)
        };

        static Dictionary<string, int> colors = new Dictionary<string, int>()
        {
            { "crimson",ItemSpriteSheet.POTION_CRIMSON },
            { "amber", ItemSpriteSheet.POTION_AMBER},
            { "golden", ItemSpriteSheet.POTION_GOLDEN},
            { "jade", ItemSpriteSheet.POTION_JADE},
            { "turquoise", ItemSpriteSheet.POTION_TURQUOISE},
            { "azure", ItemSpriteSheet.POTION_AZURE},
            { "indigo", ItemSpriteSheet.POTION_INDIGO},
            { "magenta", ItemSpriteSheet.POTION_MAGENTA},
            { "bistre", ItemSpriteSheet.POTION_BISTRE},
            { "charcoal", ItemSpriteSheet.POTION_CHARCOAL},
            { "silver", ItemSpriteSheet.POTION_SILVER},
            { "ivory", ItemSpriteSheet.POTION_IVORY}
        };

        static Type[] arrMustThrowPots =
        {
            typeof(PotionOfToxicGas),
            typeof(PotionOfLiquidFlame),
            typeof(PotionOfParalyticGas),
            typeof(PotionOfFrost),

            //exotic
            typeof(PotionOfCorrosiveGas),
            typeof(PotionOfSnapFreeze),
            typeof(PotionOfShroudingFog),
            typeof(PotionOfStormClouds)

            //also all brews, hardcoded
        };

        private static HashSet<Type> mustThrowPots = new HashSet<Type>(arrMustThrowPots);

        static Type[] arrCanThrowPots =
        {
            typeof(AlchemicalCatalyst),

            typeof(PotionOfPurity),
            typeof(PotionOfLevitation),

            //exotic
            typeof(PotionOfCleansing),

            //elixirs
            typeof(ElixirOfHoneyedHealing)
        };

        private static HashSet<Type> canThrowPots = new HashSet<Type>(arrCanThrowPots);

        protected static ItemStatusHandler handler;

        protected string color;

        private void InitInstance()
        {
            stackable = true;
            defaultAction = AC_DRINK;
        }

        public static void InitColors()
        {
            handler = new ItemStatusHandler(potions, colors);
        }

        public static void Save(Bundle bundle)
        {
            handler.Save(bundle);
        }

        public static void SaveSelectively(Bundle bundle, List<Item> items)
        {
            List<Type> classes = new List<Type>();
            foreach (Item i in items)
            {
                Type t = i.GetType();

                if (i is ExoticPotion)
                {
                    if (!classes.Contains(ExoticPotion.exoToReg[t]))
                    {
                        classes.Add(ExoticPotion.exoToReg[t]);
                    }
                }
                else if (i is Potion)
                {
                    if (!classes.Contains(t))
                    {
                        classes.Add(t);
                    }
                }
            }

            handler.SaveClassesSelectively(bundle, classes);
        }

        public static void Restore(Bundle bundle)
        {
            handler = new ItemStatusHandler(potions, colors, bundle);
        }

        public Potion()
        {
            InitInstance();

            Reset();
        }

        //anonymous potions are always IDed, do not affect ID status,
        //and their sprite is replaced by a placeholder if they are not known,
        //useful for items that appear in UIs, or which are only spawned for their effects
        protected bool anonymous;

        public void Anonymize()
        {
            if (!IsKnown())
                image = ItemSpriteSheet.POTION_HOLDER;
            anonymous = true;
        }

        public override void Reset()
        {
            base.Reset();

            var type = GetType();

            if (handler != null && handler.Contains(type))
            {
                image = handler.Image(type);
                color = handler.Label(type);
            }
            SetAction();
        }

        public override bool Collect(Bag container)
        {
            if (base.Collect(container))
            {
                SetAction();
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual void SetAction()
        {
            var type = GetType();

            if (IsKnown() && mustThrowPots.Contains(type))
            {
                defaultAction = AC_THROW;
            }
            else if (IsKnown() && canThrowPots.Contains(type))
            {
                defaultAction = AC_CHOOSE;
            }
            else
            {
                defaultAction = AC_DRINK;
            }
        }

        public override List<string> Actions(Hero hero)
        {
            var actions = base.Actions(hero);
            actions.Add(AC_DRINK);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_CHOOSE))
            {
                GameScene.Show(new WndUseItem(null, this));
            }
            else if (action.Equals(AC_DRINK))
            {
                if (IsKnown() && mustThrowPots.Contains(GetType()))
                {
                    var t = typeof(Potion);
                    var wnd = new WndOptions(
                        Messages.Get(t, "harmful"),
                        Messages.Get(t, "sure_drink"),
                        Messages.Get(t, "yes"),
                        Messages.Get(t, "no"));

                    wnd.selectAction = (index) =>
                    {
                        if (index == 0)
                            Drink(hero);
                    };
                    GameScene.Show(wnd);
                }
                else
                {
                    Drink(hero);
                }
            }
        }

        public override void DoThrow(Hero hero)
        {
            if (IsKnown() &&
                !mustThrowPots.Contains(GetType()) &&
                !canThrowPots.Contains(GetType()))
            {
                var t = typeof(Potion);
                var wnd = new WndOptions(
                    Messages.Get(t, "beneficial"),
                    Messages.Get(t, "sure_throw"),
                    Messages.Get(t, "yes"),
                    Messages.Get(t, "no"));
                wnd.selectAction = (index) =>
                {
                    if (index == 0)
                        base.DoThrow(hero);
                };
                GameScene.Show(wnd);
            }
            else
            {
                base.DoThrow(hero);
            }
        }

        protected virtual void Drink(Hero hero)
        {
            Detach(hero.belongings.backpack);

            hero.Spend(TIME_TO_DRINK);
            hero.Busy();
            Apply(hero);

            Sample.Instance.Play(Assets.Sounds.DRINK);

            hero.sprite.Operate(hero.pos);
        }

        public override void OnThrow(int cell)
        {
            if (Dungeon.level.map[cell] == Terrain.WELL || Dungeon.level.pit[cell])
            {
                base.OnThrow(cell);
            }
            else
            {
                Dungeon.level.PressCell(cell);
                Shatter(cell);
            }
        }

        public virtual void Apply(Hero hero)
        {
            Shatter(hero.pos);
        }

        public virtual void Shatter(int cell)
        {
            if (Dungeon.level.heroFOV[cell])
            {
                GLog.Information(Messages.Get(typeof(Potion), "shatter"));
                Sample.Instance.Play(Assets.Sounds.SHATTER);
                Splash(cell);
            }
        }

        public override void Cast(Hero user, int dst)
        {
            base.Cast(user, dst);
        }

        public virtual bool IsKnown()
        {
            return anonymous || (handler != null && handler.IsKnown(GetType()));
        }

        public virtual void SetKnown()
        {
            if (!anonymous)
            {
                if (!IsKnown())
                {
                    handler.Know(GetType());
                    UpdateQuickslot();
                    var p = (Potion)Dungeon.hero.belongings.GetItem(GetType());
                    if (p != null)
                        p.SetAction();

                    if (ExoticPotion.regToExo.TryGetValue(GetType(), out Type t))
                    {
                        p = (Potion)Dungeon.hero.belongings.GetItem(t);
                        if (p != null)
                            p.SetAction();
                    }
                }

                if (Dungeon.hero.IsAlive())
                    CatalogExtensions.SetSeen(GetType());
            }
        }

        public override Item Identify()
        {
            SetKnown();
            return base.Identify();
        }

        public override string Name()
        {
            return IsKnown() ? base.Name() : Messages.Get(this, color);
        }

        public override string Info()
        {
            return IsKnown() ? Desc() : Messages.Get(this, "unknown_desc");
        }

        public override bool IsIdentified()
        {
            return IsKnown();
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public static HashSet<Type> GetKnown()
        {
            return handler.Known();
        }

        public static HashSet<Type> GetUnknown()
        {
            return handler.Unknown();
        }

        //public static bool AllKnown()
        //{
        //    return handler.Known().Count == potions.Length;
        //}

        protected virtual Color SplashColor()
        {
            return anonymous ? new Color(0x00, 0xAA, 0xFF, 0xFF) : ItemSprite.Pick(image, 5, 9);
        }

        public void Splash(int cell)
        {
            Fire fire = (Fire)Dungeon.level.GetBlob(typeof(Fire));
            if (fire != null)
                fire.Clear(cell);

            var color = SplashColor();

            var ch = Actor.FindChar(cell);
            if (ch != null && ch.alignment == Character.Alignment.ALLY)
            {
                Buff.Detach<Burning>(ch);
                Buff.Detach<Ooze>(ch);
                effects.Splash.At(ch.sprite.Center(), color, 5);
            }
            else
            {
                effects.Splash.At(cell, color, 5);
            }
        }

        public override int Value()
        {
            return 30 * quantity;
        }

        [SPDStatic]
        public class PlaceHolder : Potion
        {
            public PlaceHolder()
            {
                image = ItemSpriteSheet.POTION_HOLDER;
            }

            public override bool IsSimilar(Item item)
            {
                return ExoticPotion.regToExo.ContainsKey(item.GetType())
                        || ExoticPotion.regToExo.ContainsValue(item.GetType());
            }

            public override string Info()
            {
                return "";
            }
        }

        public class SeedToPotion : Recipe
        {
            public static Dictionary<Type, Type> types = new Dictionary<Type, Type>();

            static SeedToPotion()
            {
                types.Add(typeof(Blindweed.Seed), typeof(PotionOfInvisibility));
                types.Add(typeof(Dreamfoil.Seed), typeof(PotionOfPurity));
                types.Add(typeof(Earthroot.Seed), typeof(PotionOfParalyticGas));
                types.Add(typeof(Fadeleaf.Seed), typeof(PotionOfMindVision));
                types.Add(typeof(Firebloom.Seed), typeof(PotionOfLiquidFlame));
                types.Add(typeof(Icecap.Seed), typeof(PotionOfFrost));
                types.Add(typeof(Rotberry.Seed), typeof(PotionOfStrength));
                types.Add(typeof(Sorrowmoss.Seed), typeof(PotionOfToxicGas));
                types.Add(typeof(Starflower.Seed), typeof(PotionOfExperience));
                types.Add(typeof(Stormvine.Seed), typeof(PotionOfLevitation));
                types.Add(typeof(Sungrass.Seed), typeof(PotionOfHealing));
                types.Add(typeof(Swiftthistle.Seed), typeof(PotionOfHaste));
            }

            public override bool TestIngredients(List<Item> ingredients)
            {
                if (ingredients.Count != 3)
                {
                    return false;
                }

                foreach (Item ingredient in ingredients)
                {
                    if (!(ingredient is Plant.Seed &&
                        ingredient.Quantity() >= 1 &&
                        types.ContainsKey(ingredient.GetType())))
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int Cost(List<Item> ingredients)
            {
                return 0;
            }

            public override Item Brew(List<Item> ingredients)
            {
                if (!TestIngredients(ingredients))
                    return null;

                foreach (Item ingredient in ingredients)
                {
                    ingredient.Quantity(ingredient.Quantity() - 1);
                }

                List<Type> seeds = new List<Type>();
                foreach (Item i in ingredients)
                {
                    if (!seeds.Contains(i.GetType()))
                        seeds.Add(i.GetType());
                }

                Item result;

                if ((seeds.Count == 2 && Rnd.Int(4) == 0) ||
                    (seeds.Count == 3 && Rnd.Int(2) == 0))
                {
                    result = Generator.RandomUsingDefaults(Generator.Category.POTION);
                }
                else
                {
                    var t = Rnd.Element(ingredients).GetType();
                    var new_t = types[t];

                    result = (Potion)Reflection.NewInstance(new_t);
                }

                if (seeds.Count == 1)
                    result.Identify();

                while (result is PotionOfHealing &&
                      (Dungeon.IsChallenged(Challenges.NO_HEALING) ||
                       Rnd.Int(10) < Dungeon.LimitedDrops.COOKING_HP.count))
                {
                    result = Generator.RandomUsingDefaults(Generator.Category.POTION);
                }

                if (result is PotionOfHealing)
                    ++Dungeon.LimitedDrops.COOKING_HP.count;

                ++Statistics.potionsCooked;
                BadgesExtensions.ValidatePotionsCooked();

                return result;
            }

            public override Item SampleOutput(List<Item> ingredients)
            {
                return new SeedToPotionPlaceHolder(ItemSpriteSheet.POTION_HOLDER);
            }

            class SeedToPotionPlaceHolder : WndBag.Placeholder
            {
                public SeedToPotionPlaceHolder(int image)
                    : base(image)
                { }

                public override string Name()
                {
                    return Messages.Get(typeof(Potion.SeedToPotion), "name");
                }

                public override string Info()
                {
                    return "";
                }
            }
        }
    }
}