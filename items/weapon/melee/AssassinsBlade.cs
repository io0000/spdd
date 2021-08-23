using System;
using watabou.utils;
using spdd.sprites;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.mobs;

namespace spdd.items.weapon.melee
{
    public class AssassinsBlade : MeleeWeapon
    {
        public AssassinsBlade()
        {
            image = ItemSpriteSheet.ASSASSINS_BLADE;
            hitSound = Assets.Sounds.HIT_STAB;
            hitSoundPitch = 0.9f;

            tier = 4;
        }

        public override int Max(int lvl)
        {
            return 4 * (tier + 1) +     //20 base, down from 25
                   lvl * (tier + 1);    //scaling unchanged
        }

        public override int DamageRoll(Character owner)
        {
            if (owner is Hero)
            {
                Hero hero = (Hero)owner;
                var enemy = hero.Enemy();
                if (enemy is Mob && ((Mob)enemy).SurprisedBy(hero))
                {
                    //deals 50% toward max to max on surprise, instead of min to max.
                    int diff = Max() - Min();
                    int damage = augment.DamageFactor(Rnd.NormalIntRange(
                        Min() + (int)Math.Round(diff * 0.50f, MidpointRounding.AwayFromZero),
                        Max()));
                    int exStr = hero.GetSTR() - STRReq();
                    if (exStr > 0)
                    {
                        damage += Rnd.IntRange(0, exStr);
                    }
                    return damage;
                }
            }

            return base.DamageRoll(owner);
        }
    }
}