using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.sprites;

namespace spdd.items.weapon.missiles
{
    public class ThrowingKnife : MissileWeapon
    {
        public ThrowingKnife()
        {
            image = ItemSpriteSheet.THROWING_KNIFE;
            hitSound = Assets.Sounds.HIT_SLASH;
            hitSoundPitch = 1.2f;

            bones = false;

            tier = 1;
            baseUses = 5;
        }

        public override int Max(int lvl)
        {
            return 6 * tier +                         //6 base, up from 5
                  (tier == 1 ? 2 * lvl : tier * lvl); //scaling unchanged
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
                    //deals 75% toward max to max on surprise, instead of min to max.
                    int diff = Max() - Min();
                    int damage = augment.DamageFactor(Rnd.NormalIntRange(
                            Min() + (int)Math.Round(diff * 0.75f, MidpointRounding.AwayFromZero),
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