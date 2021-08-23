using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.messages;

namespace spdd.items.weapon.melee
{
    public class MeleeWeapon : Weapon
    {
        public int tier;

        public override int Min(int lvl)
        {
            return tier +  //base
                   lvl;    //level scaling
        }

        public override int Max(int lvl)
        {
            return 5 * (tier + 1) +    //base
                   lvl * (tier + 1);   //level scaling
        }

        public override int STRReq(int lvl)
        {
            lvl = Math.Max(0, lvl);
            //strength req decreases at +1,+3,+6,+10,etc.
            return (8 + tier * 2) - (int)(Math.Sqrt(8 * lvl + 1) - 1) / 2;
        }

        public override int DamageRoll(Character owner)
        {
            int damage = augment.DamageFactor(base.DamageRoll(owner));

            if (owner is Hero)
            {
                int exStr = ((Hero)owner).GetSTR() - STRReq();
                if (exStr > 0)
                {
                    damage += Rnd.IntRange(0, exStr);
                }
            }

            return damage;
        }

        public override string Info()
        {
            string info = Desc();

            if (levelKnown)
            {
                info += "\n\n" + Messages.Get(typeof(MeleeWeapon), "stats_known", tier, augment.DamageFactor(Min()), augment.DamageFactor(Max()), STRReq());
                if (STRReq() > Dungeon.hero.GetSTR())
                {
                    info += " " + Messages.Get(typeof(Weapon), "too_heavy");
                }
                else if (Dungeon.hero.GetSTR() > STRReq())
                {
                    info += " " + Messages.Get(typeof(Weapon), "excess_str", Dungeon.hero.GetSTR() - STRReq());
                }
            }
            else
            {
                info += "\n\n" + Messages.Get(typeof(MeleeWeapon), "stats_unknown", tier, Min(0), Max(0), STRReq(0));
                if (STRReq(0) > Dungeon.hero.GetSTR())
                {
                    info += " " + Messages.Get(typeof(MeleeWeapon), "probably_too_heavy");
                }
            }

            string statsInfo = StatsInfo();
            if (!statsInfo.Equals("")) 
                info += "\n\n" + statsInfo;

            switch (augment)
            {
                case Augment.SPEED:
                    info += "\n\n" + Messages.Get(typeof(Weapon), "faster");
                    break;
                case Augment.DAMAGE:
                    info += "\n\n" + Messages.Get(typeof(Weapon), "stronger");
                    break;
                case Augment.NONE:
                    break;
            }

            if (enchantment != null && (cursedKnown || !enchantment.Curse()))
            {
                info += "\n\n" + Messages.Get(typeof(Weapon), "enchanted", enchantment.Name());
                info += " " + Messages.Get(enchantment, "desc");
            }

            if (cursed && IsEquipped(Dungeon.hero))
            {
                info += "\n\n" + Messages.Get(typeof(Weapon), "cursed_worn");
            }
            else if (cursedKnown && cursed)
            {
                info += "\n\n" + Messages.Get(typeof(Weapon), "cursed");
            }
            else if (!IsIdentified() && cursedKnown)
            {
                info += "\n\n" + Messages.Get(typeof(Weapon), "not_cursed");
            }

            return info;
        }

        public virtual string StatsInfo()
        {
            return Messages.Get(this, "stats_desc");
        }

        public override int Value()
        {
            int price = 20 * tier;

            if (HasGoodEnchant())
                price = (int)(price * 1.5f);

            if (cursedKnown && (cursed || HasCurseEnchant()))
                price /= 2;

            if (levelKnown && GetLevel() > 0)
                price *= (GetLevel() + 1);

            if (price < 1)
                price = 1;

            return price;
        }
    }
}