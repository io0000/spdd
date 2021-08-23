using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.items.stones;

namespace spdd.items.scrolls.exotic
{
    public abstract class ExoticScroll : Scroll
    {
        public static HashMap<Type, Type> regToExo = new HashMap<Type, Type>();
        public static HashMap<Type, Type> exoToReg = new HashMap<Type, Type>();

        static ExoticScroll()
        {
            regToExo.Add(typeof(ScrollOfIdentify), typeof(ScrollOfDivination));
            exoToReg.Add(typeof(ScrollOfDivination), typeof(ScrollOfIdentify));

            regToExo.Add(typeof(ScrollOfUpgrade), typeof(ScrollOfEnchantment));
            exoToReg.Add(typeof(ScrollOfEnchantment), typeof(ScrollOfUpgrade));

            regToExo.Add(typeof(ScrollOfTerror), typeof(ScrollOfPetrification));
            exoToReg.Add(typeof(ScrollOfPetrification), typeof(ScrollOfTerror));

            regToExo.Add(typeof(ScrollOfRemoveCurse), typeof(ScrollOfAntiMagic));
            exoToReg.Add(typeof(ScrollOfAntiMagic), typeof(ScrollOfRemoveCurse));

            regToExo.Add(typeof(ScrollOfLullaby), typeof(ScrollOfAffection));
            exoToReg.Add(typeof(ScrollOfAffection), typeof(ScrollOfLullaby));

            regToExo.Add(typeof(ScrollOfRage), typeof(ScrollOfConfusion));
            exoToReg.Add(typeof(ScrollOfConfusion), typeof(ScrollOfRage));

            // 동일한 키를 사용하는 항목이 이미 추가되었습니다.
            //regToExo.Add(typeof(ScrollOfTerror), typeof(ScrollOfPetrification));
            //exoToReg.Add(typeof(ScrollOfPetrification), typeof(ScrollOfTerror));

            regToExo.Add(typeof(ScrollOfRecharging), typeof(ScrollOfMysticalEnergy));
            exoToReg.Add(typeof(ScrollOfMysticalEnergy), typeof(ScrollOfRecharging));

            regToExo.Add(typeof(ScrollOfMagicMapping), typeof(ScrollOfForesight));
            exoToReg.Add(typeof(ScrollOfForesight), typeof(ScrollOfMagicMapping));

            regToExo.Add(typeof(ScrollOfTeleportation), typeof(ScrollOfPassage));
            exoToReg.Add(typeof(ScrollOfPassage), typeof(ScrollOfTeleportation));

            regToExo.Add(typeof(ScrollOfRetribution), typeof(ScrollOfPsionicBlast));
            exoToReg.Add(typeof(ScrollOfPsionicBlast), typeof(ScrollOfRetribution));

            regToExo.Add(typeof(ScrollOfMirrorImage), typeof(ScrollOfPrismaticImage));
            exoToReg.Add(typeof(ScrollOfPrismaticImage), typeof(ScrollOfMirrorImage));

            regToExo.Add(typeof(ScrollOfTransmutation), typeof(ScrollOfPolymorph));
            exoToReg.Add(typeof(ScrollOfPolymorph), typeof(ScrollOfTransmutation));
        }

        public override bool IsKnown()
        {
            return anonymous || (handler != null && handler.IsKnown(exoToReg.Get(GetType())));
        }

        public override void SetKnown()
        {
            if (!IsKnown())
            {
                handler.Know(exoToReg.Get(GetType()));
                UpdateQuickslot();
            }
        }

        public override void Reset()
        {
            base.Reset();

            var reg = exoToReg.Get(GetType());

            if (handler != null && handler.Contains(reg))
            {
                image = handler.Image(reg) + 16;
                rune = handler.Label(reg);
            }
        }

        //20 gold more than its none-exotic equivalent
        public override int Value()
        {
            var reg = exoToReg.Get(GetType());

            var scroll = (Scroll)Reflection.NewInstance(reg);

            return (scroll.Value() + 20) * quantity;
        }

        public class ScrollToExotic : Recipe
        {
            public override bool TestIngredients(List<Item> ingredients)
            {
                int r = 0;
                Scroll s = null;

                foreach (Item i in ingredients)
                {
                    if (i is Runestone)
                    {
                        ++r;
                    }
                    else if (regToExo.ContainsKey(i.GetType()))
                    {
                        s = (Scroll)i;
                    }
                }

                return s != null && r == 2;
            }

            public override int Cost(List<Item> ingredients)
            {
                return 0;
            }

            public override Item Brew(List<Item> ingredients)
            {
                Item result = null;

                foreach (Item i in ingredients)
                {
                    i.Quantity(i.Quantity() - 1);

                    var type = i.GetType();
                    if (regToExo.ContainsKey(type))
                    {
                        result = (Item)Reflection.NewInstance(regToExo[type]);
                    }
                }
                return result;
            }

            public override Item SampleOutput(List<Item> ingredients)
            {
                foreach (Item i in ingredients)
                {
                    var type = i.GetType();
                    if (regToExo.ContainsKey(type))
                    {
                        return (Item)Reflection.NewInstance(regToExo[type]);
                    }
                }
                return null;
            }
        }
    }
}