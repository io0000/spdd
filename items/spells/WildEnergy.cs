using System;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.items.artifacts;
using spdd.items.wands;
using spdd.items.quest;
using spdd.items.scrolls.exotic;
using spdd.mechanics;
using spdd.items.scrolls;


namespace spdd.items.spells
{
    public class WildEnergy : TargetedSpell
    {
        public WildEnergy()
        {
            image = ItemSpriteSheet.WILD_ENERGY;
        }

        //we rely on cursedWand to do fx instead
        protected override void Fx(Ballistic bolt, ICallback callback)
        {
            AffectTarget(bolt, curUser);
        }

        protected override void AffectTarget(Ballistic bolt, Hero hero)
        {
            var callback = new ActionCallback();
            callback.action = () =>
            {
                Sample.Instance.Play(Assets.Sounds.LIGHTNING);
                Sample.Instance.Play(Assets.Sounds.CHARGEUP);
                ScrollOfRecharging.Charge(hero);

                hero.belongings.Charge(1f);
                for (int i = 0; i < 4; ++i)
                {
                    if (hero.belongings.artifact is Artifact)
                        ((Artifact)hero.belongings.artifact).Charge(hero);
                    if (hero.belongings.misc is Artifact)
                        ((Artifact)hero.belongings.misc).Charge(hero);
                }

                Buff.Affect<Recharging>(hero, 8f);
                Buff.Affect<ArtifactRecharge>(hero).Prolong(8);

                Detach(curUser.belongings.backpack);
                UpdateQuickslot();
                curUser.SpendAndNext(1f);
            };

            CursedWand.CursedZap(this, hero, bolt, callback);
        }

        public override int Value()
        {
            //prices of ingredients, divided by output quantity
            return (int)Math.Round(quantity * ((50 + 100) / 5f), MidpointRounding.AwayFromZero);
        }

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(ScrollOfMysticalEnergy), typeof(MetalShard) };
                inQuantity = new int[] { 1, 1 };

                cost = 8;

                output = typeof(WildEnergy);
                outQuantity = 5;
            }
        }
    }
}