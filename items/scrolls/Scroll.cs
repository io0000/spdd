using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.sprites;
using spdd.utils;
using spdd.journal;
using spdd.items.artifacts;
using spdd.items.scrolls.exotic;
using spdd.items.stones;
using spdd.messages;

namespace spdd.items.scrolls
{
    public abstract class Scroll : Item
    {
        public const string AC_READ = "READ";

        public const float TIME_TO_READ = 1f;

        private static readonly Type[] scrolls = {
            typeof(ScrollOfIdentify),
            typeof(ScrollOfMagicMapping),
            typeof(ScrollOfRecharging),
            typeof(ScrollOfRemoveCurse),
            typeof(ScrollOfTeleportation),
            typeof(ScrollOfUpgrade),
            typeof(ScrollOfRage),
            typeof(ScrollOfTerror),
            typeof(ScrollOfLullaby),
            typeof(ScrollOfTransmutation),
            typeof(ScrollOfRetribution),
            typeof(ScrollOfMirrorImage)
        };

        private static Dictionary<string, int> runes = new Dictionary<string, int>()
        {
            { "KAUNAN", ItemSpriteSheet.SCROLL_KAUNAN },
            { "SOWILO", ItemSpriteSheet.SCROLL_SOWILO },
            { "LAGUZ", ItemSpriteSheet.SCROLL_LAGUZ },
            { "YNGVI", ItemSpriteSheet.SCROLL_YNGVI },
            { "GYFU", ItemSpriteSheet.SCROLL_GYFU },
            { "RAIDO", ItemSpriteSheet.SCROLL_RAIDO },
            { "ISAZ", ItemSpriteSheet.SCROLL_ISAZ },
            { "MANNAZ", ItemSpriteSheet.SCROLL_MANNAZ },
            { "NAUDIZ", ItemSpriteSheet.SCROLL_NAUDIZ },
            { "BERKANAN", ItemSpriteSheet.SCROLL_BERKANAN },
            { "ODAL", ItemSpriteSheet.SCROLL_ODAL },
            { "TIWAZ", ItemSpriteSheet.SCROLL_TIWAZ }
        };

        protected static ItemStatusHandler handler;

        protected string rune;

        private void InitInstance()
        {
            stackable = true;
            defaultAction = AC_READ;
        }

        public static void InitLabels()
        {
            handler = new ItemStatusHandler(scrolls, runes);
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
                var itemType = i.GetType();

                if (i is ExoticScroll)
                {
                    var reg = ExoticScroll.exoToReg.Get(itemType);
                    if (!classes.Contains(reg))
                    {
                        classes.Add(reg);
                    }
                }
                else if (i is Scroll)
                {
                    if (!classes.Contains(itemType))
                    {
                        classes.Add(itemType);
                    }
                }
            }
            handler.SaveClassesSelectively(bundle, classes);
        }

        public static void Restore(Bundle bundle)
        {
            handler = new ItemStatusHandler(scrolls, runes, bundle);
        }

        protected Scroll()
        {
            InitInstance();
            Reset();
        }

        //anonymous scrolls are always IDed, do not affect ID status,
        //and their sprite is replaced by a placeholder if they are not known,
        //useful for items that appear in UIs, or which are only spawned for their effects
        public bool anonymous;

        public void Anonymize()
        {
            if (!IsKnown())
                image = ItemSpriteSheet.SCROLL_HOLDER;
            anonymous = true;
        }

        public override void Reset()
        {
            base.Reset();

            var type = GetType();

            if (handler != null && handler.Contains(type))
            {
                image = handler.Image(type);
                rune = handler.Label(type);
            }
        }

        public override List<string> Actions(Hero hero)
        {
            var actions = base.Actions(hero);
            actions.Add(AC_READ);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_READ))
            {
                if (hero.FindBuff<MagicImmune>() != null)
                {
                    GLog.Warning(Messages.Get(this, "no_magic"));
                }
                else if (hero.FindBuff<Blindness>() != null)
                {
                    GLog.Warning(Messages.Get(this, "blinded"));
                }
                else if (hero.FindBuff<UnstableSpellbook.BookRecharge>() != null &&
                    hero.FindBuff<UnstableSpellbook.BookRecharge>().IsCursed() &&
                    !(this is ScrollOfRemoveCurse || this is ScrollOfAntiMagic))
                {
                    GLog.Negative(Messages.Get(this, "cursed"));
                }
                else
                {
                    curUser = hero;
                    curItem = Detach(hero.belongings.backpack);
                    DoRead();
                }
            }
        }

        public abstract void DoRead();

        public void ReadAnimation()
        {
            curUser.Spend(TIME_TO_READ);
            curUser.Busy();
            Invisibility.Dispel();
            ((HeroSprite)curUser.sprite).Read();
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
                }

                if (Dungeon.hero.IsAlive())
                {
                    CatalogExtensions.SetSeen(GetType());
                }
            }
        }

        public override Item Identify()
        {
            SetKnown();
            return base.Identify();
        }

        public override string Name()
        {
            return IsKnown() ? base.Name() : Messages.Get(this, rune);
        }

        public override string Info()
        {
            return IsKnown() ?
                Desc() :
                Messages.Get(this, "unknown_desc");
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return IsKnown();
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
        //    return handler.Known().Count == scrolls.Length;
        //}

        public override int Value()
        {
            return 30 * quantity;
        }

        [SPDStatic]
        public class PlaceHolder : Scroll
        {
            public PlaceHolder()
            {
                image = ItemSpriteSheet.SCROLL_HOLDER;
            }

            public override bool IsSimilar(Item item)
            {
                return ExoticScroll.regToExo.ContainsKey(item.GetType()) ||
                    ExoticScroll.regToExo.ContainsValue(item.GetType());
            }

            public override void DoRead()
            { }

            public override string Info()
            {
                return "";
            }
        }   // PlaceHolder

        public class ScrollToStone : Recipe
        {
            private static Dictionary<Type, Type> stones = new Dictionary<Type, Type>();
            private static Dictionary<Type, int> amnts = new Dictionary<Type, int>();

            static ScrollToStone()
            {
                stones.Add(typeof(ScrollOfIdentify), typeof(StoneOfIntuition));
                amnts.Add(typeof(ScrollOfIdentify), 3);

                stones.Add(typeof(ScrollOfLullaby), typeof(StoneOfDeepenedSleep));
                amnts.Add(typeof(ScrollOfLullaby), 3);

                stones.Add(typeof(ScrollOfMagicMapping), typeof(StoneOfClairvoyance));
                amnts.Add(typeof(ScrollOfMagicMapping), 3);

                stones.Add(typeof(ScrollOfMirrorImage), typeof(StoneOfFlock));
                amnts.Add(typeof(ScrollOfMirrorImage), 3);

                stones.Add(typeof(ScrollOfRetribution), typeof(StoneOfBlast));
                amnts.Add(typeof(ScrollOfRetribution), 2);

                stones.Add(typeof(ScrollOfRage), typeof(StoneOfAggression));
                amnts.Add(typeof(ScrollOfRage), 3);

                stones.Add(typeof(ScrollOfRecharging), typeof(StoneOfShock));
                amnts.Add(typeof(ScrollOfRecharging), 2);

                stones.Add(typeof(ScrollOfRemoveCurse), typeof(StoneOfDisarming));
                amnts.Add(typeof(ScrollOfRemoveCurse), 2);

                stones.Add(typeof(ScrollOfTeleportation), typeof(StoneOfBlink));
                amnts.Add(typeof(ScrollOfTeleportation), 2);

                stones.Add(typeof(ScrollOfTerror), typeof(StoneOfAffection));
                amnts.Add(typeof(ScrollOfTerror), 3);

                stones.Add(typeof(ScrollOfTransmutation), typeof(StoneOfAugmentation));
                amnts.Add(typeof(ScrollOfTransmutation), 2);

                stones.Add(typeof(ScrollOfUpgrade), typeof(StoneOfEnchantment));
                amnts.Add(typeof(ScrollOfUpgrade), 2);
            }

            public override bool TestIngredients(List<Item> ingredients)
            {
                if (ingredients.Count != 1 ||
                    !ingredients[0].IsIdentified() ||
                    !(ingredients[0] is Scroll) ||
                    !stones.ContainsKey(ingredients[0].GetType()))
                {
                    return false;
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

                Scroll s = (Scroll)ingredients[0];

                s.Quantity(s.Quantity() - 1);

                var stoneType = stones[s.GetType()];
                var amount = amnts[s.GetType()];
                var runestone = (Runestone)Reflection.NewInstance(stoneType);
                runestone.Quantity(amount);

                return runestone;
            }

            public override Item SampleOutput(List<Item> ingredients)
            {
                if (!TestIngredients(ingredients))
                    return null;

                Scroll s = (Scroll)ingredients[0];

                var stoneType = stones[s.GetType()];
                var amount = amnts[s.GetType()];
                var runestone = (Runestone)Reflection.NewInstance(stoneType);
                runestone.Quantity(amount);

                return runestone;
            }
        }
    }
}