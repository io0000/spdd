using System;
using spdd.actors;
using spdd.actors.mobs;
using spdd.sprites;

namespace spdd.items.weapon.missiles
{
    public class FishingSpear : MissileWeapon
    {
        public FishingSpear()
        {
            image = ItemSpriteSheet.FISHING_SPEAR;
            hitSound = Assets.Sounds.HIT_STAB;
            hitSoundPitch = 1.1f;

            tier = 2;
        }

        public override int Proc(Character attacker, Character defender, int damage)
        {
            if (defender is Piranha)
            {
                damage = Math.Max(damage, defender.HP / 2);
            }
            return base.Proc(attacker, defender, damage);
        }
    }
}