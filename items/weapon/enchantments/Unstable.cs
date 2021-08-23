using System;
using watabou.utils;
using spdd.actors;
using spdd.sprites;

namespace spdd.items.weapon.enchantments
{
    public class Unstable : Weapon.Enchantment
    {
        private static ItemSprite.Glowing GREY = new ItemSprite.Glowing(new Color(0x99, 0x99, 0x99, 0xFF));

        private static Type[] randomEnchants = new Type[] {
            typeof(Blazing),
            typeof(Blocking),
            typeof(Blooming),
            typeof(Chilling),
            typeof(Kinetic),
            typeof(Corrupting),
            typeof(Elastic),
            typeof(Grim),
            typeof(Lucky), 
            //projecting not included, no on-hit effect
            typeof(Shocking),
            typeof(Vampiric)
        };

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            int conservedDamage = 0;
            var buff = attacker.FindBuff<Kinetic.ConservedDamage>();
            if (buff != null)
            {
                conservedDamage = buff.DamageBonus();
                buff.Detach();
            }

            var enchant = (Weapon.Enchantment)Reflection.NewInstance(Rnd.OneOf(randomEnchants));
            damage = enchant.Proc(weapon, attacker, defender, damage);

            return damage + conservedDamage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return GREY;
        }
    }
}