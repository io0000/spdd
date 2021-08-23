using System;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.items.potions.exotic;
using spdd.sprites;
using spdd.effects.particles;

namespace spdd.items.potions.elixirs
{
    public class ElixirOfDragonsBlood : Elixir
    {
        public ElixirOfDragonsBlood()
        {
            //TODO finish visuals
            image = ItemSpriteSheet.ELIXIR_DRAGON;
        }

        public override void Apply(Hero hero)
        {
            Buff.Affect<FireImbue>(hero).Set(FireImbue.DURATION);
            Sample.Instance.Play(Assets.Sounds.BURNING);
            hero.sprite.Emitter().Burst(FlameParticle.Factory, 10);
        }

        protected override Color SplashColor()
        {
            return new Color(0xFF, 0x00, 0x2A, 0xFF);
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
                inputs = new Type[] { typeof(PotionOfDragonsBreath), typeof(AlchemicalCatalyst) };
                inQuantity = new int[] { 1, 1 };

                cost = 6;

                output = typeof(ElixirOfDragonsBlood);
                outQuantity = 1;
            }
        }
    }
}