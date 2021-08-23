using spdd.ui;
using watabou.utils;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class LockedFloor : Buff
    {
        //the amount of turns remaining before beneficial passive effects turn off
        private float left = 50; //starts at 50 turns

        public override bool Act()
        {
            Spend(TICK);

            if (!Dungeon.level.locked)
                Detach();

            if (left >= 1)
                --left;

            return true;
        }

        public void AddTime(float time)
        {
            left += time;
        }

        public bool RegenOn()
        {
            return left >= 1;
        }

        private const string LEFT = "left";

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

        public override int Icon()
        {
            return BuffIndicator.LOCKED_FLOOR;
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc");
        }
    }
}