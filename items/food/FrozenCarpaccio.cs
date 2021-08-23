using System;
using watabou.utils;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.effects;
using spdd.sprites;
using spdd.items.potions;
using spdd.utils;
using spdd.messages;

namespace spdd.items.food
{
    public class FrozenCarpaccio : Food
    {
        public FrozenCarpaccio()
        {
            image = ItemSpriteSheet.CARPACCIO;
            energy = Hunger.HUNGRY / 2f;
        }

        protected override void Satisfy(Hero hero)
        {
            base.Satisfy(hero);
            Effect(hero);
        }

        public override int Value()
        {
            return 10 * quantity;
        }

        public static void Effect(Hero hero)
        {
            switch (Rnd.Int(5))
            {
                case 0:
                    GLog.Information(Messages.Get(typeof(FrozenCarpaccio), "invis"));
                    Buff.Affect<Invisibility>(hero, Invisibility.DURATION);
                    break;
                case 1:
                    GLog.Information(Messages.Get(typeof(FrozenCarpaccio), "hard"));
                    Buff.Affect<Barkskin>(hero).Set(hero.HT / 4, 1);
                    break;
                case 2:
                    GLog.Information(Messages.Get(typeof(FrozenCarpaccio), "refresh"));
                    PotionOfHealing.Cure(hero);
                    break;
                case 3:
                    GLog.Information(Messages.Get(typeof(FrozenCarpaccio), "better"));
                    if (hero.HP < hero.HT)
                    {
                        hero.HP = Math.Min(hero.HP + hero.HT / 4, hero.HT);
                        hero.sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 1);
                    }
                    break;
            }
        }

        public static Food Cook(MysteryMeat ingredient)
        {
            var result = new FrozenCarpaccio();
            result.quantity = ingredient.Quantity();
            return result;
        }
    }
}