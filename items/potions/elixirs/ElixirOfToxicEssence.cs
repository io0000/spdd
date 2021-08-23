using System;
using watabou.utils;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.sprites;
using spdd.effects.particles;

namespace spdd.items.potions.elixirs
{
    public class ElixirOfToxicEssence : Elixir
    {
        public ElixirOfToxicEssence()
        {
            //TODO finish visuals
            image = ItemSpriteSheet.ELIXIR_TOXIC;
        }

        public override void Apply(Hero hero)
        {
            Buff.Affect<ToxicImbue>(hero).Set(ToxicImbue.DURATION);
            hero.sprite.Emitter().Burst(PoisonParticle.Factory, 10);
        }

        protected override Color SplashColor()
        {
            return new Color(0x00, 0xB3, 0x4A, 0xFF);
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (30 + 40);
        }

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(PotionOfToxicGas), typeof(AlchemicalCatalyst) };
                inQuantity = new int[] { 1, 1 };

                cost = 6;

                output = typeof(ElixirOfToxicEssence);
                outQuantity = 1;
            }
        }
    }
}