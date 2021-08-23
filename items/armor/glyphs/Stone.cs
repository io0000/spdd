using System;
using watabou.utils;
using spdd.actors;
using spdd.sprites;

namespace spdd.items.armor.glyphs
{
    public class Stone : Armor.Glyph
    {
        private static readonly ItemSprite.Glowing GREY = new ItemSprite.Glowing(new Color(0x22, 0x22, 0x22, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            testing = true;
            float evasion = defender.DefenseSkill(attacker);
            float accuracy = attacker.AttackSkill(defender);
            testing = false;

            float hitChance;
            if (evasion >= accuracy)
            {
                hitChance = (accuracy / evasion) / 2f;
            }
            else
            {
                hitChance = 1f - (evasion / accuracy) / 2f;
            }

            //75% of dodge chance is applied as damage reduction
            // we clamp in case accuracy or evasion were negative
            hitChance = GameMath.Gate(0.25f, (1f + 3f * hitChance) / 4f, 1f);

            damage = (int)Math.Ceiling(damage * hitChance);

            return damage;
        }

        private bool testing;

        public bool TestingEvasion()
        {
            return testing;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return GREY;
        }
    }
}