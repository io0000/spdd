using System;
using watabou.utils;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.effects;
using spdd.actors.hero;
using spdd.items.scrolls;
using spdd.messages;

namespace spdd.items.food
{
    public class Pasty : Food
    {
        //TODO: implement fun stuff for other holidays
        //TODO: probably should externalize this if I want to add any more festive stuff.
        private enum Holiday
        {
            NONE,
            EASTER, //TBD
            HWEEN,  //2nd week of october though first day of november
            XMAS    //3rd week of december through first week of january
        }

        private static Holiday holiday;

        static Pasty()
        {
            holiday = Holiday.NONE;

            //final Calendar calendar = Calendar.getInstance();
            //switch (calendar.get(Calendar.MONTH))
            //{
            //    case Calendar.JANUARY:
            //        if (calendar.get(Calendar.WEEK_OF_MONTH) == 1)
            //            holiday = Holiday.XMAS;
            //        break;
            //    case Calendar.OCTOBER:
            //        if (calendar.get(Calendar.WEEK_OF_MONTH) >= 2)
            //            holiday = Holiday.HWEEN;
            //        break;
            //    case Calendar.NOVEMBER:
            //        if (calendar.get(Calendar.DAY_OF_MONTH) == 1)
            //            holiday = Holiday.HWEEN;
            //        break;
            //    case Calendar.DECEMBER:
            //        if (calendar.get(Calendar.WEEK_OF_MONTH) >= 3)
            //            holiday = Holiday.XMAS;
            //        break;
            //}
            var now = DateTime.Now;
            var wom = Utils.GetWeekOfMonth(now);
            switch (now.Month)
            {
                case 1:  // JANUARY
                    if (wom == 1)
                        holiday = Holiday.XMAS;
                    break;
                case 10: //OCTOBER:
                    if (wom >= 2)
                        holiday = Holiday.HWEEN;
                    break;
                case 11: // NOVEMBER:
                    if (wom == 1)
                        holiday = Holiday.HWEEN;
                    break;
                case 12: // DECEMBER:
                    if (wom >= 3)
                        holiday = Holiday.XMAS;
                    break;
            }
        }

        public Pasty()
        {
            Reset();

            energy = Hunger.STARVING;

            bones = true;
        }

        public override void Reset()
        {
            base.Reset();
            switch (holiday)
            {
                case Holiday.NONE:
                    image = ItemSpriteSheet.PASTY;
                    break;
                case Holiday.HWEEN:
                    image = ItemSpriteSheet.PUMPKIN_PIE;
                    break;
                case Holiday.XMAS:
                    image = ItemSpriteSheet.CANDY_CANE;
                    break;
            }
        }

        protected override void Satisfy(Hero hero)
        {
            base.Satisfy(hero);

            switch (holiday)
            {
                case Holiday.NONE:
                    break; //do nothing extra
                case Holiday.HWEEN:
                    //heals for 10% max hp
                    hero.HP = Math.Min(hero.HP + hero.HT / 10, hero.HT);
                    hero.sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 1);
                    break;
                case Holiday.XMAS:
                    Buff.Affect<Recharging>(hero, 2f); //half of a charge
                    ScrollOfRecharging.Charge(hero);
                    break;
            }
        }

        public override string Name()
        {
            switch (holiday)
            {
                case Holiday.NONE:
                default:
                    return Messages.Get(this, "pasty");
                case Holiday.HWEEN:
                    return Messages.Get(this, "pie");
                case Holiday.XMAS:
                    return Messages.Get(this, "cane");
            }
        }

        public override string Info()
        {
            switch (holiday)
            {
                case Holiday.NONE:
                default:
                    return Messages.Get(this, "pasty_desc");
                case Holiday.HWEEN:
                    return Messages.Get(this, "pie_desc");
                case Holiday.XMAS:
                    return Messages.Get(this, "cane_desc");
            }
        }

        public override int Value()
        {
            return 20 * quantity;
        }
    }
}