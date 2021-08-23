using System;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.effects;
using spdd.utils;
using spdd.items.potions;
using spdd.messages;

namespace spdd.items.spells
{
    public class FeatherFall : Spell
    {
        public FeatherFall()
        {
            image = ItemSpriteSheet.FEATHER_FALL;
        }

        protected override void OnCast(Hero hero)
        {
            Buff.Append<FeatherBuff>(hero, 30f);
            hero.sprite.Operate(hero.pos);
            Sample.Instance.Play(Assets.Sounds.READ);
            hero.sprite.Emitter().Burst(Speck.Factory(Speck.JET), 20);

            GLog.Positive(Messages.Get(this, "light"));

            Detach(curUser.belongings.backpack);
            UpdateQuickslot();
            hero.SpendAndNext(1f);
        }

        [SPDStatic]
        public class FeatherBuff : FlavourBuff
        {
            //does nothing, just waits to be triggered by chasm falling
        }

        public override int Value()
        {
            //prices of ingredients, divided by output quantity
            return (int)Math.Round(quantity * ((30 + 40) / 2f), MidpointRounding.AwayFromZero);
        }

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(PotionOfLevitation), typeof(ArcaneCatalyst) };
                inQuantity = new int[] { 1, 1 };

                cost = 6;

                output = typeof(FeatherFall);
                outQuantity = 2;
            }
        }
    }
}