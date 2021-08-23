using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.plants;

namespace spdd.items.potions.exotic
{
    public class ExoticPotion : Potion
    {
        public ExoticPotion()
        {
            //sprite = equivalent potion sprite but one row down
        }

        public static HashMap<Type, Type> regToExo = new HashMap<Type, Type>();
        public static HashMap<Type, Type> exoToReg = new HashMap<Type, Type>();

        static ExoticPotion()
        {
            regToExo.Add(typeof(PotionOfHealing), typeof(PotionOfShielding));
            exoToReg.Add(typeof(PotionOfShielding), typeof(PotionOfHealing));

            regToExo.Add(typeof(PotionOfToxicGas), typeof(PotionOfCorrosiveGas));
            exoToReg.Add(typeof(PotionOfCorrosiveGas), typeof(PotionOfToxicGas));

            regToExo.Add(typeof(PotionOfStrength), typeof(PotionOfAdrenalineSurge));
            exoToReg.Add(typeof(PotionOfAdrenalineSurge), typeof(PotionOfStrength));

            regToExo.Add(typeof(PotionOfFrost), typeof(PotionOfSnapFreeze));
            exoToReg.Add(typeof(PotionOfSnapFreeze), typeof(PotionOfFrost));

            regToExo.Add(typeof(PotionOfHaste), typeof(PotionOfStamina));
            exoToReg.Add(typeof(PotionOfStamina), typeof(PotionOfHaste));

            regToExo.Add(typeof(PotionOfLiquidFlame), typeof(PotionOfDragonsBreath));
            exoToReg.Add(typeof(PotionOfDragonsBreath), typeof(PotionOfLiquidFlame));

            regToExo.Add(typeof(PotionOfInvisibility), typeof(PotionOfShroudingFog));
            exoToReg.Add(typeof(PotionOfShroudingFog), typeof(PotionOfInvisibility));

            regToExo.Add(typeof(PotionOfMindVision), typeof(PotionOfMagicalSight));
            exoToReg.Add(typeof(PotionOfMagicalSight), typeof(PotionOfMindVision));

            regToExo.Add(typeof(PotionOfLevitation), typeof(PotionOfStormClouds));
            exoToReg.Add(typeof(PotionOfStormClouds), typeof(PotionOfLevitation));

            regToExo.Add(typeof(PotionOfExperience), typeof(PotionOfHolyFuror));
            exoToReg.Add(typeof(PotionOfHolyFuror), typeof(PotionOfExperience));

            regToExo.Add(typeof(PotionOfPurity), typeof(PotionOfCleansing));
            exoToReg.Add(typeof(PotionOfCleansing), typeof(PotionOfPurity));

            regToExo.Add(typeof(PotionOfParalyticGas), typeof(PotionOfEarthenArmor));
            exoToReg.Add(typeof(PotionOfEarthenArmor), typeof(PotionOfParalyticGas));
        }

        public override bool IsKnown()
        {
            return anonymous || (handler != null && handler.IsKnown(exoToReg.Get(this.GetType())));
        }

        public override void SetKnown()
        {
            if (!IsKnown())
            {
                var type = GetType();
                var reg = exoToReg.Get(type);
                handler.Know(reg);

                UpdateQuickslot();

                Potion p = (Potion)Dungeon.hero.belongings.GetItem(type);
                if (p != null)
                    p.SetAction();

                p = (Potion)Dungeon.hero.belongings.GetItem(reg);
                if (p != null)
                    p.SetAction();
            }
        }

        public override void Reset()
        {
            base.Reset();

            var type = GetType();
            var reg = exoToReg.Get(type);

            if (handler != null && handler.Contains(reg))
            {
                image = handler.Image(reg) + 16;
                color = handler.Label(reg);
            }
        }

        //20 gold more than its none-exotic equivalent
        public override int Value()
        {
            Type t = this.GetType();

            var reg = exoToReg.Get(t);

            Potion p = (Potion)Reflection.NewInstance(reg);

            return (p.Value() + 20) * quantity;
        }

        public class PotionToExotic : Recipe
        {
            public override bool TestIngredients(List<Item> ingredients)
            {
                int s = 0;
                Potion p = null;

                foreach (Item i in ingredients)
                {
                    if (i is Plant.Seed)
                    {
                        ++s;
                    }
                    else if (regToExo.ContainsKey(i.GetType()))
                    {
                        p = (Potion)i;
                    }
                }

                return p != null && s == 2;
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

                    var t = i.GetType();

                    if (regToExo.ContainsKey(t))
                    {
                        var exo = regToExo[t];
                        result = (Potion)Reflection.NewInstance(exo);
                    }
                }

                return result;
            }

            public override Item SampleOutput(List<Item> ingredients)
            {
                foreach (Item i in ingredients)
                {
                    var t = i.GetType();

                    if (regToExo.ContainsKey(t))
                    {
                        var exo = regToExo[t];
                        return (Potion)Reflection.NewInstance(exo);
                    }
                }

                return null;
            }
        }
    }
}