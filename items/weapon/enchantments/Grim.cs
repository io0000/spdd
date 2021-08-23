using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.effects.particles;
using spdd.sprites;


namespace spdd.items.weapon.enchantments
{
    public class Grim : Weapon.Enchantment
    {
        private static ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x00, 0x00, 0x00, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            int level = Math.Max(0, weapon.BuffedLvl());

            int enemyHealth = defender.HP - damage;
            if (enemyHealth <= 0)
                return damage; //no point in proccing if they're already dead.

            //scales from 0 - 50% based on how low hp the enemy is, plus 5% per level
            float maxChance = 0.5f + .05f * level;
            float chanceMulti = (float)Math.Pow(((defender.HT - enemyHealth) / (float)defender.HT), 2);
            float chance = maxChance * chanceMulti;

            if (Rnd.Float() < chance)
            {
                defender.Damage(defender.HP, this);
                defender.sprite.Emitter().Burst(ShadowParticle.Up, 5);

                if (!defender.IsAlive() &&
                    attacker is Hero &&
                    weapon.HasEnchant(typeof(Grim), attacker))
                {
                    BadgesExtensions.ValidateGrimWeapon();
                }
            }

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return BLACK;
        }
    }
}