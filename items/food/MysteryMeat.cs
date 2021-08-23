using watabou.utils;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.sprites;
using spdd.utils;
using spdd.messages;

namespace spdd.items.food
{
    public class MysteryMeat : Food
    {
        public MysteryMeat()
        {
            image = ItemSpriteSheet.MEAT;
            energy = Hunger.HUNGRY / 2f;
        }

        protected override void Satisfy(Hero hero)
        {
            base.Satisfy(hero);
            Effect(hero);
        }

        public override int Value()
        {
            return 5 * quantity;
        }

        public static void Effect(Hero hero)
        {
            switch (Rnd.Int(5))
            {
                case 0:
                    GLog.Information(Messages.Get(typeof(MysteryMeat), "hot"));
                    Buff.Affect<Burning>(hero).Reignite(hero);
                    break;
                case 1:
                    GLog.Information(Messages.Get(typeof(MysteryMeat), "legs"));
                    Buff.Prolong<Roots>(hero, Roots.DURATION * 2f);
                    break;
                case 2:
                    GLog.Information(Messages.Get(typeof(MysteryMeat), "not_well"));
                    Buff.Affect<Poison>(hero).Set(hero.HT / 5);
                    break;
                case 3:
                    GLog.Information(Messages.Get(typeof(MysteryMeat), "stuffed"));
                    Buff.Prolong<Slow>(hero, Slow.DURATION);
                    break;
            }
        }

        [SPDStatic]
        public class PlaceHolder : MysteryMeat
        {
            public PlaceHolder()
            {
                image = ItemSpriteSheet.FOOD_HOLDER;
            }

            public override bool IsSimilar(Item item)
            {
                return item is MysteryMeat || 
                    item is StewedMeat || 
                    item is ChargrilledMeat || 
                    item is FrozenCarpaccio;
            }

            public override string Info()
            {
                return "";
            }
        }
    }
}