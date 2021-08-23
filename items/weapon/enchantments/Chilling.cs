using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;
using spdd.sprites;

namespace spdd.items.weapon.enchantments
{
    public class Chilling : Weapon.Enchantment
    {
        private static ItemSprite.Glowing TEAL = new ItemSprite.Glowing(new Color(0x00, 0xFF, 0xFF, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            // lvl 0 - 25%
            // lvl 1 - 40%
            // lvl 2 - 50%
            int level = Math.Max(0, weapon.BuffedLvl());

            if (Rnd.Int(level + 4) >= 3)
            {
                //adds 3 turns of chill per proc, with a cap of 6 turns
                float durationToAdd = 3f;
                Chill existing = defender.FindBuff<Chill>();
                if (existing != null)
                {
                    durationToAdd = Math.Min(durationToAdd, 6f - existing.Cooldown());
                }

                Buff.Affect<Chill>(defender, durationToAdd);
                Splash.At(defender.sprite.Center(), new Color(0xB2, 0xD6, 0xFF, 0xFF), 5);
            }

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return TEAL;
        }
    }
}