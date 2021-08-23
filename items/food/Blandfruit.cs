using System.Collections.Generic;
using watabou.utils;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.levels;
using spdd.utils;
using spdd.plants;
using spdd.items.potions;
using spdd.actors.hero;
using spdd.messages;

namespace spdd.items.food
{
    public class Blandfruit : Food
    {
        public Potion potionAttrib = null;
        public ItemSprite.Glowing potionGlow = null;

        public Blandfruit()
        {
            stackable = true;
            image = ItemSpriteSheet.BLANDFRUIT;

            //only applies when blandfruit is cooked
            energy = Hunger.STARVING;

            bones = true;
        }

        public override bool IsSimilar(Item item)
        {
            if (base.IsSimilar(item))
            {
                Blandfruit other = (Blandfruit)item;
                if (potionAttrib == null && other.potionAttrib == null)
                {
                    return true;
                }
                else if (potionAttrib != null && 
                    other.potionAttrib != null && 
                    potionAttrib.IsSimilar(other.potionAttrib))
                {
                    return true;
                }
            }
            return false;
        }

        public override void Execute(Hero hero, string action)
        {
            if (action.Equals(AC_EAT) && potionAttrib == null)
            {
                GLog.Warning(Messages.Get(this, "raw"));
                return;
            }

            base.Execute(hero, action);

            if (action.Equals(AC_EAT) && potionAttrib != null)
            {
                potionAttrib.Apply(hero);
            }
        }

        public override string Name()
        {
            if (potionAttrib is PotionOfHealing)
                return Messages.Get(this, "sunfruit");
            if (potionAttrib is PotionOfStrength)
                return Messages.Get(this, "rotfruit");
            if (potionAttrib is PotionOfParalyticGas)
                return Messages.Get(this, "earthfruit");
            if (potionAttrib is PotionOfInvisibility)
                return Messages.Get(this, "blindfruit");
            if (potionAttrib is PotionOfLiquidFlame)
                return Messages.Get(this, "firefruit");
            if (potionAttrib is PotionOfFrost)
                return Messages.Get(this, "icefruit");
            if (potionAttrib is PotionOfMindVision)
                return Messages.Get(this, "fadefruit");
            if (potionAttrib is PotionOfToxicGas)
                return Messages.Get(this, "sorrowfruit");
            if (potionAttrib is PotionOfLevitation)
                return Messages.Get(this, "stormfruit");
            if (potionAttrib is PotionOfPurity)
                return Messages.Get(this, "dreamfruit");
            if (potionAttrib is PotionOfExperience)
                return Messages.Get(this, "starfruit");
            if (potionAttrib is PotionOfHaste)
                return Messages.Get(this, "swiftfruit");
            return base.Name();
        }

        public override string Desc()
        {
            if (potionAttrib == null)
            {
                return base.Desc();
            }
            else
            {
                string desc = Messages.Get(this, "desc_cooked") + "\n\n";
                if (potionAttrib is PotionOfFrost ||
                    potionAttrib is PotionOfLiquidFlame ||
                    potionAttrib is PotionOfToxicGas ||
                    potionAttrib is PotionOfParalyticGas)
                {
                    desc += Messages.Get(this, "desc_throw");
                }
                else
                {
                    desc += Messages.Get(this, "desc_eat");
                }
                return desc;
            }
        }

        public override int Value()
        {
            return 20 * quantity;
        }

        public Item Cook(Plant.Seed seed)
        {
            var type = seed.GetType();
            var item = (Potion)Reflection.NewInstance(Potion.SeedToPotion.types[type]);
            return ImbuePotion(item);
        }

        public Item ImbuePotion(Potion potion)
        {
            potionAttrib = potion;
            potionAttrib.Anonymize();

            potionAttrib.image = ItemSpriteSheet.BLANDFRUIT;

            if (potionAttrib is PotionOfHealing)
                potionGlow = new ItemSprite.Glowing(new Color(0x2E, 0xE6, 0x2E, 0xFF));
            if (potionAttrib is PotionOfStrength)
                potionGlow = new ItemSprite.Glowing(new Color(0xCC, 0x00, 0x22, 0xFF));
            if (potionAttrib is PotionOfParalyticGas)
                potionGlow = new ItemSprite.Glowing(new Color(0x67, 0x58, 0x3D, 0xFF));
            if (potionAttrib is PotionOfInvisibility)
                potionGlow = new ItemSprite.Glowing(new Color(0xD9, 0xD9, 0xD9, 0xFF));
            if (potionAttrib is PotionOfLiquidFlame)
                potionGlow = new ItemSprite.Glowing(new Color(0xFF, 0x7F, 0x00, 0xFF));
            if (potionAttrib is PotionOfFrost)
                potionGlow = new ItemSprite.Glowing(new Color(0x66, 0xB3, 0xFF, 0xFF));
            if (potionAttrib is PotionOfMindVision)
                potionGlow = new ItemSprite.Glowing(new Color(0x91, 0x99, 0x99, 0xFF));
            if (potionAttrib is PotionOfToxicGas)
                potionGlow = new ItemSprite.Glowing(new Color(0xA1, 0x5C, 0xE5, 0xFF));
            if (potionAttrib is PotionOfLevitation)
                potionGlow = new ItemSprite.Glowing(new Color(0x1B, 0x5F, 0x79, 0xFF));
            if (potionAttrib is PotionOfPurity)
                potionGlow = new ItemSprite.Glowing(new Color(0xC1, 0x52, 0xAA, 0xFF));
            if (potionAttrib is PotionOfExperience)
                potionGlow = new ItemSprite.Glowing(new Color(0x40, 0x40, 0x40, 0xFF));
            if (potionAttrib is PotionOfHaste)
                potionGlow = new ItemSprite.Glowing(new Color(0xCC, 0xBB, 0x00, 0xFF));

            return this;
        }

        public const string POTIONATTRIB = "potionattrib";

        public override void OnThrow(int cell)
        {
            if (Dungeon.level.map[cell] == Terrain.WELL || Dungeon.level.pit[cell])
            {
                base.OnThrow(cell);
            }
            else if (potionAttrib is PotionOfLiquidFlame ||
                  potionAttrib is PotionOfToxicGas ||
                  potionAttrib is PotionOfParalyticGas ||
                  potionAttrib is PotionOfFrost ||
                  potionAttrib is PotionOfLevitation ||
                  potionAttrib is PotionOfPurity)
            {
                potionAttrib.Shatter(cell);
                Dungeon.level.Drop(new Chunks(), cell).sprite.Drop();
            }
            else
            {
                base.OnThrow(cell);
            }
        }

        public override void Reset()
        {
            if (potionAttrib != null)
                ImbuePotion(potionAttrib);
            else
                base.Reset();
        }

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(POTIONATTRIB, potionAttrib);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            if (bundle.Contains(POTIONATTRIB))
            {
                ImbuePotion((Potion)bundle.Get(POTIONATTRIB));
            }
        }

        public override ItemSprite.Glowing Glowing()
        {
            return potionGlow;
        }

        public class CookFruit : Recipe
        {
            //also sorts ingredients if it can
            public override bool TestIngredients(List<Item> ingredients)
            {
                if (ingredients.Count != 2)
                    return false;

                if (ingredients[0] is Blandfruit)
                {
                    if (!(ingredients[1] is Plant.Seed))
                    {
                        return false;
                    }
                }
                else if (ingredients[0] is Plant.Seed)
                {
                    if (ingredients[1] is Blandfruit)
                    {
                        Item temp = ingredients[0];
                        ingredients[0] = ingredients[1];
                        ingredients[1] = temp;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                var fruit = (Blandfruit)ingredients[0];
                var seed = (Plant.Seed)ingredients[1];

                if (fruit.Quantity() >= 1 &&
                    fruit.potionAttrib == null &&
                    seed.Quantity() >= 1)
                {
                    if (Dungeon.IsChallenged(Challenges.NO_HEALING) &&
                        seed is Sungrass.Seed)
                    {
                        return false;
                    }

                    return true;
                }

                return false;
            }

            public override int Cost(List<Item> ingredients)
            {
                return 3;
            }

            public override Item Brew(List<Item> ingredients)
            {
                if (!TestIngredients(ingredients))
                    return null;

                ingredients[0].Quantity(ingredients[0].Quantity() - 1);
                ingredients[1].Quantity(ingredients[1].Quantity() - 1);

                return new Blandfruit().Cook((Plant.Seed)ingredients[1]);
            }

            public override Item SampleOutput(List<Item> ingredients)
            {
                if (!TestIngredients(ingredients))
                    return null;

                return new Blandfruit().Cook((Plant.Seed)ingredients[1]);
            }
        }

        [SPDStatic]
        public class Chunks : Food
        {
            public Chunks()
            {
                stackable = true;
                image = ItemSpriteSheet.BLAND_CHUNKS;

                energy = Hunger.STARVING;

                bones = true;
            }
        }
    }
}