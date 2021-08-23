using System;
using watabou.utils;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.items.potions.exotic;
using spdd.sprites;
using spdd.effects.particles;

namespace spdd.items.potions.elixirs
{
    public class ElixirOfIcyTouch : Elixir
    {
        public ElixirOfIcyTouch()
        {
            //TODO finish visuals
            image = ItemSpriteSheet.ELIXIR_ICY;
        }

        public override void Apply(Hero hero)
        {
            Buff.Affect<FrostImbue>(hero, FrostImbue.DURATION);
            hero.sprite.Emitter().Burst(SnowParticle.Factory, 5);
        }

        protected override Color SplashColor()
        {
            return new Color(0x18, 0xC3, 0xE6, 0xFF);
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
                inputs = new Type[] { typeof(PotionOfSnapFreeze), typeof(AlchemicalCatalyst) };
                inQuantity = new int[] { 1, 1 };

                cost = 6;

                output = typeof(ElixirOfIcyTouch);
                outQuantity = 1;
            }
        }
    }
}