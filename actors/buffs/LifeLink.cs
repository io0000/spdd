using watabou.utils;
using spdd.ui;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class LifeLink : FlavourBuff
    {
        public int obj;
        private const string OBJECT = "object";

        public LifeLink()
        {
            type = BuffType.POSITIVE;
            announced = true;
        }

        public override void Detach()
        {
            base.Detach();
            var ch = (Character)Actor.FindById(obj);
            if (!target.IsAlive() && ch != null)
            {
                foreach (LifeLink l in ch.Buffs<LifeLink>())
                {
                    if (l.obj == target.Id())
                        l.Detach();
                }
            }
        }

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(OBJECT, obj);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            obj = bundle.GetInt(OBJECT);
        }

        public override int Icon()
        {
            return BuffIndicator.HEART;
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