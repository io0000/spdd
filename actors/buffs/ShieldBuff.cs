using watabou.utils;

namespace spdd.actors.buffs
{
    public class ShieldBuff : Buff
    {
        private int shielding;

        public override bool AttachTo(Character target)
        {
            if (base.AttachTo(target))
            {
                target.needsShieldUpdate = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Detach()
        {
            target.needsShieldUpdate = true;
            base.Detach();
        }

        public int Shielding()
        {
            return shielding;
        }

        public void SetShield(int shield)
        {
            this.shielding = shield;
            if (target != null)
                target.needsShieldUpdate = true;
        }

        public void IncShield()
        {
            IncShield(1);
        }

        public void IncShield(int amt)
        {
            shielding += amt;
            if (target != null)
                target.needsShieldUpdate = true;
        }

        public void DecShield()
        {
            DecShield(1);
        }

        public void DecShield(int amt)
        {
            shielding -= amt;
            if (target != null)
                target.needsShieldUpdate = true;
        }

        //returns the amount of damage leftover
        public virtual int AbsorbDamage(int dmg)
        {
            if (shielding >= dmg)
            {
                shielding -= dmg;
                dmg = 0;
            }
            else
            {
                dmg -= shielding;
                shielding = 0;
            }
            if (shielding == 0)
                Detach();

            if (target != null)
                target.needsShieldUpdate = true;
            return dmg;
        }

        private const string SHIELDING = "shielding";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(SHIELDING, shielding);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            shielding = bundle.GetInt(SHIELDING);
        }
    }
}