using System;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.items.quest;
using spdd.items.potions.exotic;
using spdd.sprites;


namespace spdd.items.potions.elixirs
{
    public class ElixirOfArcaneArmor : Elixir
    {
        public ElixirOfArcaneArmor()
        {
            //TODO finish visuals
            image = ItemSpriteSheet.ELIXIR_ARCANE;
        }

        public override void Apply(Hero hero)
        {
            Buff.Affect<ArcaneArmor>(hero).Set(5 + hero.lvl / 2, 80);
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (50 + 40);
        }

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(PotionOfEarthenArmor), typeof(GooBlob) };
                inQuantity = new int[] { 1, 1 };

                cost = 8;

                output = typeof(ElixirOfArcaneArmor);
                outQuantity = 1;
            }
        }
    }
}