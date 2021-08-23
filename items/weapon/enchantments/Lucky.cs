using System;
using watabou.noosa;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.items.rings;
using spdd.sprites;

namespace spdd.items.weapon.enchantments
{
    public class Lucky : Weapon.Enchantment
    {
        private static ItemSprite.Glowing GREEN = new ItemSprite.Glowing(new Color(0x00, 0xFF, 0x00, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            int level = Math.Max(0, weapon.BuffedLvl());

            // lvl 0 - 10%
            // lvl 1 ~ 12%
            // lvl 2 ~ 14%
            if (defender.HP <= damage && Rnd.Int(level + 40) >= 36)
            {
                Buff.Affect<LuckProc>(defender);
            }

            return damage;
        }

        public static Item GenLoot()
        {
            //80% common, 20% uncommon, 0% rare
            return RingOfWealth.GenConsumableDrop(-5);
        }

        public static void ShowFlare(Visual vis)
        {
            RingOfWealth.ShowFlareForBonusDrop(vis);
        }

        public override ItemSprite.Glowing Glowing()
        {
            return GREEN;
        }

        //used to keep track of whether a luck proc is incoming. see Mob.die()
        [SPDStatic]
        public class LuckProc : Buff
        {
            public override bool Act()
            {
                Detach();
                return true;
            }
        }
    }
}