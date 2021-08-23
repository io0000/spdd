using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.effects;
using spdd.items.scrolls;
using spdd.sprites;
using spdd.utils;
using spdd.messages;

namespace spdd.items.food
{
    public class Food : Item
    {
        public const float TIME_TO_EAT = 3.0f;

        public const string AC_EAT = "EAT";

        public float energy = Hunger.HUNGRY;

        public string message; // = Messages.Get(this, "eat_msg");

        public Food()
        {
            stackable = true;
            image = ItemSpriteSheet.RATION;

            bones = true;
            message = Messages.Get(this, "eat_msg");
        }

        public override List<string> Actions(Hero hero)
        {
            var actions = base.Actions(hero);
            actions.Add(AC_EAT);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_EAT))
            {
                Detach(hero.belongings.backpack);

                Satisfy(hero);
                GLog.Information(message);

                FoodProc(hero);

                hero.sprite.Operate(hero.pos);
                hero.Busy();
                SpellSprite.Show(hero, SpellSprite.FOOD);
                Sample.Instance.Play(Assets.Sounds.EAT);

                hero.Spend(TIME_TO_EAT);

                ++Statistics.foodEaten;
                BadgesExtensions.ValidateFoodEaten();
            }
        }

        protected virtual void Satisfy(Hero hero)
        {
            Buff.Affect<Hunger>(hero).Satisfy(energy);
        }

        public static void FoodProc(Hero hero)
        {
            switch (hero.heroClass)
            {
                case HeroClass.WARRIOR:
                    if (hero.HP < hero.HT)
                    {
                        hero.HP = Math.Min(hero.HP + 5, hero.HT);
                        hero.sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 1);
                    }
                    break;
                case HeroClass.MAGE:
                    hero.belongings.Charge(1.0f);
                    ScrollOfRecharging.Charge(hero);
                    break;
                case HeroClass.ROGUE:
                case HeroClass.HUNTRESS:
                    break;
            }
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public override int Value()
        {
            return 10 * quantity;
        }
    }
}