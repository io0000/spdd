using System;
using watabou.utils;
using watabou.noosa;
using spdd.ui;
using spdd.utils;
using spdd.messages;
using spdd.actors.hero;

namespace spdd.actors.buffs
{
    public class Corrosion : Buff, Hero.IDoom
    {
        private float damage = 1.0f;
        private float left;

        private const string DAMAGE = "damage";
        private const string LEFT = "left";

        public Corrosion()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(DAMAGE, damage);
            bundle.Put(LEFT, left);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            damage = bundle.GetFloat(DAMAGE);
            left = bundle.GetFloat(LEFT);
        }

        public void Set(float duration, int damage)
        {
            this.left = Math.Max(duration, left);
            if (this.damage < damage) 
                this.damage = damage;
        }

        public override int Icon()
        {
            return BuffIndicator.POISON;
        }

        public override void TintIcon(Image icon)
        {
            icon.Hardlight(1f, 0.5f, 0f);
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string HeroMessage()
        {
            return Messages.Get(this, "heromsg");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", DispTurns(left), (int)damage);    //  %1$s %2$d.
        }

        public override bool Act()
        {
            if (target.IsAlive())
            {
                target.Damage((int)damage, this);
                if (damage < (Dungeon.depth / 2) + 2)
                {
                    ++damage;
                }
                else
                {
                    damage += 0.5f;
                }

                Spend(TICK);
                if ((left -= TICK) <= 0)
                    Detach();
            }
            else
            {
                Detach();
            }

            return true;
        }

        // Hero.IDoom
        public void OnDeath()
        {
            Dungeon.Fail(GetType());
            GLog.Negative(Messages.Get(this, "ondeath"));
        }
    }
}