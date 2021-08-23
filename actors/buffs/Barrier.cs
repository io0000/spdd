using watabou.noosa;
using spdd.ui;
using spdd.sprites;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Barrier : ShieldBuff
    {
        public Barrier()
        {
            type = BuffType.POSITIVE;
        }

        public override bool Act()
        {
            AbsorbDamage(1);

            if (Shielding() <= 0)
                Detach();

            Spend(TICK);

            return true;
        }

        public override void Fx(bool on)
        {
            if (on)
                target.sprite.Add(CharSprite.State.SHIELDED);
            else
                target.sprite.Remove(CharSprite.State.SHIELDED);
        }

        public override int Icon()
        {
            return BuffIndicator.ARMOR;
        }

        public override void TintIcon(Image icon)
        {
            icon.Hardlight(0.5f, 1.0f, 2.0f);
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", Shielding());
        }
    }
}