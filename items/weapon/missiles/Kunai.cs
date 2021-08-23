using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.sprites;

namespace spdd.items.weapon.missiles
{
    public class Kunai : MissileWeapon
    {
        public Kunai()
        {
            image = ItemSpriteSheet.KUNAI;
            hitSound = Assets.Sounds.HIT_STAB;
            hitSoundPitch = 1.1f;

            tier = 3;
            baseUses = 5;
        }

        private Character enemy;

        public override void OnThrow(int cell)
        {
            enemy = Actor.FindChar(cell);
            base.OnThrow(cell);
        }

        public override int DamageRoll(Character owner)
        {
            if (owner is Hero)
            {
                Hero hero = (Hero)owner;
                if (enemy is Mob && ((Mob)enemy).SurprisedBy(hero))
                {
                    //deals 60% toward max to max on surprise, instead of min to max.
                    int diff = Max() - Min();
                    int damage = augment.DamageFactor(Rnd.NormalIntRange(
                            Min() + (int)Math.Round(diff * 0.6f, MidpointRounding.AwayFromZero),
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