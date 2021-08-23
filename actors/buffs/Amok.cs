using spdd.ui;
using spdd.actors.mobs;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Amok : FlavourBuff
    {
        public Amok()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        public override int Icon()
        {
            return BuffIndicator.AMOK;
        }

        public override void Detach()
        {
            base.Detach();
            if (target is Mob && target.IsAlive()) {
                ((Mob)target).Aggro(null);
            }
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", DispTurns());
        }
    }
}