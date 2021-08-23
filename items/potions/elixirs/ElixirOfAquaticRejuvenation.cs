using System;
using watabou.utils;
using watabou.noosa;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.items.quest;
using spdd.sprites;
using spdd.effects;
using spdd.ui;
using spdd.messages;

namespace spdd.items.potions.elixirs
{
    public class ElixirOfAquaticRejuvenation : Elixir
    {
        public ElixirOfAquaticRejuvenation()
        {
            //TODO finish visuals
            image = ItemSpriteSheet.ELIXIR_AQUA;
        }

        public override void Apply(Hero hero)
        {
            Buff.Affect<AquaHealing>(hero).Set((int)Math.Round(hero.HT * 1.5f, MidpointRounding.AwayFromZero));
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (30 + 50);
        }

        [SPDStatic]
        public class AquaHealing : Buff
        {
            public AquaHealing()
            {
                type = BuffType.POSITIVE;
                announced = true;
            }

            private int left;

            public void Set(int amount)
            {
                if (amount > left)
                    left = amount;
            }

            public override bool Act()
            {
                if (Dungeon.level.water[target.pos] && target.HP < target.HT)
                {
                    float healAmt = GameMath.Gate(1, target.HT / 50f, left);
                    healAmt = Math.Min(healAmt, target.HT - target.HP);
                    if (Rnd.Float() < (healAmt % 1))
                    {
                        healAmt = (float)Math.Ceiling(healAmt);
                    }
                    else
                    {
                        healAmt = (float)Math.Floor(healAmt);
                    }
                    target.HP += (int)healAmt;
                    left -= (int)healAmt;
                    target.sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 1);
                }

                if (left <= 0)
                {
                    Detach();
                }
                else
                {
                    Spend(TICK);
                }
                return true;
            }

            public override int Icon()
            {
                return BuffIndicator.HEALING;
            }

            public override void TintIcon(Image icon)
            {
                icon.Hardlight(0, 0.75f, 0.75f);
            }

            public override float IconFadePercent()
            {
                float max = (float)Math.Round(target.HT * 1.5f, MidpointRounding.AwayFromZero);
                return Math.Max(0, (max - left) / max);
            }

            public override string ToString()
            {
                return Messages.Get(this, "name");
            }

            public override string Desc()
            {
                return Messages.Get(this, "desc", left);
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

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(PotionOfHealing), typeof(GooBlob) };
                inQuantity = new int[] { 1, 1 };

                cost = 6;

                output = typeof(ElixirOfAquaticRejuvenation);
                outQuantity = 1;
            }
        }
    }
}