using System;
using watabou.utils;
using watabou.noosa;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.sprites;
using spdd.utils;
using spdd.ui;
using spdd.messages;

namespace spdd.items.potions.elixirs
{
    public class ElixirOfMight : Elixir
    {
        public ElixirOfMight()
        {
            image = ItemSpriteSheet.ELIXIR_MIGHT;

            unique = true;
        }

        public override void Apply(Hero hero)
        {
            SetKnown();

            ++hero.STR;

            //Buff.Affect<HTBoost>(hero).Reset(); // mod
            HTBoost boost = Buff.Affect<HTBoost>(hero);
            boost.Reset();

            hero.UpdateHT(true);
            hero.sprite.ShowStatus(CharSprite.POSITIVE, Messages.Get(this, "msg_1", boost.Boost()));
            GLog.Positive(Messages.Get(this, "msg_2"));

            BadgesExtensions.ValidateStrengthAttained();
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", HTBoost.Boost(Dungeon.hero.HT));
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
                inputs = new Type[] { typeof(PotionOfStrength), typeof(AlchemicalCatalyst) };
                inQuantity = new int[] { 1, 1 };

                cost = 5;

                output = typeof(ElixirOfMight);
                outQuantity = 1;
            }
        }

        [SPDStatic]
        public class HTBoost : Buff
        {
            public HTBoost()
            {
                type = BuffType.POSITIVE;
            }

            private int left;

            public void Reset()
            {
                left = 5;
            }

            public int Boost()
            {
                return (int)Math.Round(left * Boost(target.HT) / 5f, MidpointRounding.AwayFromZero);
            }

            public static int Boost(int HT)
            {
                return (int)Math.Round(4 + HT / 20f, MidpointRounding.AwayFromZero);
            }

            public void OnLevelUp()
            {
                --left;
                if (left <= 0)
                    Detach();
            }

            public override int Icon()
            {
                return BuffIndicator.HEALING;
            }

            public override void TintIcon(Image icon)
            {
                icon.Hardlight(1f, 0.5f, 0f);
            }

            public override float IconFadePercent()
            {
                return (5f - left) / 5f;
            }

            public override string ToString()
            {
                return Messages.Get(this, "name");
            }

            public override string Desc()
            {
                return Messages.Get(this, "desc", Boost(), left);
            }

            private const string LEFT = "left";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(LEFT, left);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                left = bundle.GetInt(LEFT);
            }
        }
    }
}