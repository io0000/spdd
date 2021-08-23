using System;
using watabou.utils;
using spdd.actors;
using spdd.items.wands;
using spdd.items.weapon.missiles;
using spdd.mechanics;
using spdd.sprites;

namespace spdd.items.weapon.enchantments
{
    public class Elastic : Weapon.Enchantment
    {
        private static ItemSprite.Glowing PINK = new ItemSprite.Glowing(new Color(0xFF, 0x00, 0xFF, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            // lvl 0 - 20%
            // lvl 1 - 33%
            // lvl 2 - 43%
            int level = Math.Max(0, weapon.BuffedLvl());

            if (Rnd.Int(level + 5) >= 4)
            {
                //trace a ballistica to our target (which will also extend past them
                Ballistic trajectory = new Ballistic(attacker.pos, defender.pos, Ballistic.STOP_TARGET);
                //trim it to just be the part that goes past them
                trajectory = new Ballistic(trajectory.collisionPos, trajectory.path[trajectory.path.Count - 1], Ballistic.PROJECTILE);
                //knock them back along that ballistica
                WandOfBlastWave.ThrowChar(defender, trajectory, 2, !(weapon is MissileWeapon));
            }

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return PINK;
        }
    }
}