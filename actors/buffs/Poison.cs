using System;
using watabou.utils;
using watabou.noosa;
using spdd.actors.hero;
using spdd.ui;
using spdd.utils;
using spdd.effects;
using spdd.effects.particles;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Poison : Buff, Hero.IDoom
    {
        protected float left;

        private const string LEFT = "left";

        public Poison()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(LEFT, left);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            left = bundle.GetFloat(LEFT);
        }

        public void Set(float duration)
        {
            this.left = Math.Max(duration, left);
        }

        //public void Extend(float duration)
        //{
        //    this.left += duration;
        //}

        public override int Icon()
        {
            return BuffIndicator.POISON;
        }

        public override void TintIcon(Image icon)
        {
            icon.Hardlight(0.6f, 0.2f, 0.6f);
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
            return Messages.Get(this, "desc", DispTurns(left));
        }

        public override bool AttachTo(Character target)
        {
            if (base.AttachTo(target) && target.sprite != null)
            {
                CellEmitter.Center(target.pos).Burst(PoisonParticle.Splash, 5);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool Act()
        {
            if (target.IsAlive())
            {
                target.Damage((int)(left / 3) + 1, this);
                Spend(TICK);

                if ((left -= TICK) <= 0)
                {
                    Detach();
                }
            }
            else
            {
                Detach();
            }

            return true;
        }

        public void OnDeath()
        {
            BadgesExtensions.ValidateDeathFromPoison();

            Dungeon.Fail(GetType());
            GLog.Negative(Messages.Get(this, "ondeath"));
        }
    }
}