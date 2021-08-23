using System;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.sprites;

namespace spdd.items.potions.elixirs
{
    public class ElixirOfHoneyedHealing : Elixir
    {
        public ElixirOfHoneyedHealing()
        {
            image = ItemSpriteSheet.ELIXIR_HONEY;
        }

        public override void Apply(Hero hero)
        {
            Buff.Affect<Healing>(hero).SetHeal((int)(0.8f * hero.HT + 14), 0.25f, 0);
            PotionOfHealing.Cure(hero);
            Buff.Affect<Hunger>(hero).Satisfy(Hunger.STARVING / 5f);
        }

        public override void Shatter(int cell)
        {
            if (Dungeon.level.heroFOV[cell])
            {
                Sample.Instance.Play(Assets.Sounds.SHATTER);
                Splash(cell);
            }

            Character ch = Actor.FindChar(cell);
            if (ch != null)
            {
                Buff.Affect<Healing>(ch).SetHeal((int)(0.8f * ch.HT + 14), 0.25f, 0);
                PotionOfHealing.Cure(ch);
                if (ch is Bee && ch.alignment != curUser.alignment)
                {
                    ch.alignment = Character.Alignment.ALLY;
                    ((Bee)ch).SetPotInfo(-1, null);
                }
            }
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (30 + 5);
        }

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(PotionOfHealing), typeof(Honeypot.ShatteredPot) };
                inQuantity = new int[] { 1, 1 };

                cost = 4;

                output = typeof(ElixirOfHoneyedHealing);
                outQuantity = 1;
            }
        }
    }
}