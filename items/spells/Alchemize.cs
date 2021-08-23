using System;
using spdd.actors.hero;
using spdd.scenes;
using spdd.sprites;
using spdd.utils;
using spdd.items.potions;
using spdd.messages;

namespace spdd.items.spells
{
    public class Alchemize : Spell, AlchemyScene.IAlchemyProvider
    {
        public Alchemize()
        {
            image = ItemSpriteSheet.ALCHEMIZE;
        }

        protected override void OnCast(Hero hero)
        {
            if (hero.VisibleEnemies() > hero.mindVisionEnemies.Count)
            {
                GLog.Information(Messages.Get(this, "enemy_near"));
                return;
            }
            Detach(curUser.belongings.backpack);
            UpdateQuickslot();
            AlchemyScene.SetProvider(this);
            ShatteredPixelDungeonDash.SwitchScene(typeof(AlchemyScene));
        }

        // AlchemyScene.AlchemyProvider
        public int GetEnergy()
        {
            return 0;
        }

        // AlchemyScene.AlchemyProvider
        public void SpendEnergy(int reduction)
        {
            //do nothing
        }

        public override int Value()
        {
            //prices of ingredients, divided by output quantity
            return (int)Math.Round(quantity * ((40 + 40) / 4f), MidpointRounding.AwayFromZero);
        }

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(ArcaneCatalyst), typeof(AlchemicalCatalyst) };
                inQuantity = new int[] { 1, 1 };

                cost = 6;

                output = typeof(Alchemize);
                outQuantity = 4;
            }
        }
    }
}