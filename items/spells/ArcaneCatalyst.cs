using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors.hero;
using spdd.sprites;
using spdd.items.scrolls;
using spdd.items.scrolls.exotic;
using spdd.items.stones;
using spdd.plants;

namespace spdd.items.spells
{
    public class ArcaneCatalyst : Spell
    {
        public ArcaneCatalyst()
        {
            image = ItemSpriteSheet.SCROLL_CATALYST;
        }

        static Dictionary<Type, float> scrollChances = new Dictionary<Type, float>()
        {
            { typeof(ScrollOfIdentify),      3f },
            { typeof(ScrollOfRemoveCurse),   2f },
            { typeof(ScrollOfMagicMapping),  2f },
            { typeof(ScrollOfMirrorImage),   2f },
            { typeof(ScrollOfRecharging),    2f },
            { typeof(ScrollOfLullaby),       2f },
            { typeof(ScrollOfRetribution),   2f },
            { typeof(ScrollOfRage),          2f },
            { typeof(ScrollOfTeleportation), 2f },
            { typeof(ScrollOfTerror),        2f },
            { typeof(ScrollOfTransmutation), 1f }
        };

        protected override void OnCast(Hero hero)
        {
            Detach(curUser.belongings.backpack);
            UpdateQuickslot();

            var s = (Scroll)Reflection.NewInstance(Rnd.Chances(scrollChances));
            s.Anonymize();
            curItem = s;
            s.DoRead();
        }

        public override int Value()
        {
            return 40 * quantity;
        }

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public override bool TestIngredients(List<Item> ingredients)
            {
                bool scroll = false;
                bool secondary = false;

                foreach (Item i in ingredients)
                {
                    var type = i.GetType();
                    if (i is Plant.Seed || i is Runestone)
                    {
                        secondary = true;
                    }
                    //if it is a regular or exotic potion
                    else if (ExoticScroll.regToExo.ContainsKey(type) ||
                        ExoticScroll.regToExo.ContainsValue(type))
                    {
                        scroll = true;
                    }
                }

                return scroll && secondary;
            }

            public override int Cost(List<Item> ingredients)
            {
                foreach (Item i in ingredients)
                {
                    if (i is Plant.Seed)
                    {
                        return 2;
                    }
                    else if (i is Runestone)
                    {
                        return 1;
                    }
                }
                return 1;
            }

            public override Item Brew(List<Item> ingredients)
            {
                foreach (Item i in ingredients)
                {
                    i.Quantity(i.Quantity() - 1);
                }

                return SampleOutput(null);
            }

            public override Item SampleOutput(List<Item> ingredients)
            {
                return new ArcaneCatalyst();
            }
        }
    }
}