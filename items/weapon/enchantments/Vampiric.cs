using System;
using watabou.utils;
using spdd.actors;
using spdd.effects;
using spdd.sprites;

namespace spdd.items.weapon.enchantments
{
    public class Vampiric : Weapon.Enchantment
    {
        private static ItemSprite.Glowing RED = new ItemSprite.Glowing(new Color(0x66, 0x00, 0x22, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            //chance to heal scales from 5%-30% based on missing HP
            float missingPercent = (attacker.HT - attacker.HP) / (float)attacker.HT;
            float healChance = 0.05f + .25f * missingPercent;

            if (Rnd.Float() < healChance)
            {
                //heals for 50% of damage dealt
                int healAmt = (int)Math.Round(damage * 0.5f, MidpointRounding.AwayFromZero);
                healAmt = Math.Min(healAmt, attacker.HT - attacker.HP);

                if (healAmt > 0 && attacker.IsAlive())
                {
                    attacker.HP += healAmt;
                    attacker.sprite.Emitter().Start(Speck.Factory(Speck.HEALING), 0.4f, 1);
                    attacker.sprite.ShowStatus(CharSprite.POSITIVE, healAmt.ToString());
                }
            }

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return RED;
        }
    }
}