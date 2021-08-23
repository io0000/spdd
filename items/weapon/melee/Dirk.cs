using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class Dirk : MeleeWeapon
    {
        public Dirk()
        {
            image = ItemSpriteSheet.DIRK;
            hitSound = Assets.Sounds.HIT_STAB;
            hitSoundPitch = 1f;

            tier = 2;
        }

        public override int Max(int lvl)
        {
            return 4 * (tier + 1) +     //12 base, down from 15
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
                    //deals 67% toward max to max on surprise, instead of min to max.
                    int diff = Max() - Min();
                    int damage = augment.DamageFactor(Rnd.NormalIntRange(
                            Min() + (int)Math.Round(diff * 0.67f, MidpointRounding.AwayFromZero),
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