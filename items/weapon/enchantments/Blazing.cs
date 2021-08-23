using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects.particles;
using spdd.sprites;

namespace spdd.items.weapon.enchantments
{
    public class Blazing : Weapon.Enchantment
    {
        private static ItemSprite.Glowing ORANGE = new ItemSprite.Glowing(new Color(0xFF, 0x44, 0x00, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            // lvl 0 - 33%
            // lvl 1 - 50%
            // lvl 2 - 60%
            int level = Math.Max(0, weapon.BuffedLvl());

            if (Rnd.Int(level + 3) >= 2)
            {
                if (defender.FindBuff<Burning>() != null)
                {
                    Buff.Affect<Burning>(defender).Reignite(defender, 8f);
                    int burnDamage = Rnd.NormalIntRange(1, 3 + Dungeon.depth / 4);
                    defender.Damage((int)Math.Round(burnDamage * 0.67f, MidpointRounding.AwayFromZero), this);
                }
                else
                {
                    Buff.Affect<Burning>(defender).Reignite(defender, 8f);
                }

                defender.sprite.Emitter().Burst(FlameParticle.Factory, level + 1);
            }

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return ORANGE;
        }
    }
}