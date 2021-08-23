using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using spdd.actors.hero;
using spdd.effects;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.utils;
using spdd.windows;

namespace spdd.items
{
    public class TomeOfMastery : Item
    {
        public const float TIME_TO_READ = 10;

        public const string AC_READ = "READ";

        public TomeOfMastery()
        {
            stackable = false;
            image = ItemSpriteSheet.MASTERY;

            unique = true;
        }

        public override List<string> Actions(Hero hero)
        {
            var actions = base.Actions(hero);
            actions.Add(AC_READ);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_READ))
            {
                curUser = hero;

                HeroSubClass way1 = HeroSubClass.NONE;
                HeroSubClass way2 = HeroSubClass.NONE;
                switch (hero.heroClass)
                {
                    case HeroClass.WARRIOR:
                        way1 = HeroSubClass.GLADIATOR;
                        way2 = HeroSubClass.BERSERKER;
                        break;
                    case HeroClass.MAGE:
                        way1 = HeroSubClass.BATTLEMAGE;
                        way2 = HeroSubClass.WARLOCK;
                        break;
                    case HeroClass.ROGUE:
                        way1 = HeroSubClass.FREERUNNER;
                        way2 = HeroSubClass.ASSASSIN;
                        break;
                    case HeroClass.HUNTRESS:
                        way1 = HeroSubClass.SNIPER;
                        way2 = HeroSubClass.WARDEN;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                GameScene.Show(new WndChooseWay(this, way1, way2));
            }
        }

        public override bool DoPickUp(Hero hero)
        {
            BadgesExtensions.ValidateMastery();
            return base.DoPickUp(hero);
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public void Choose(HeroSubClass way)
        {
            Detach(curUser.belongings.backpack);

            curUser.Spend(TIME_TO_READ);
            curUser.Busy();

            curUser.subClass = way;

            curUser.sprite.Operate(curUser.pos);
            Sample.Instance.Play(Assets.Sounds.MASTERY);

            SpellSprite.Show(curUser, SpellSprite.MASTERY);
            curUser.sprite.Emitter().Burst(Speck.Factory(Speck.MASTERY), 12);
            GLog.Warning(Messages.Get(this, "way", way.Title()));
        }
    }
}