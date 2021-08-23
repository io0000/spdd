using watabou.noosa.audio;
using watabou.utils;
using spdd.ui;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Shadows : Invisibility
    {
        protected float left;

        private const string LEFT = "left";

        public Shadows()
        {
            announced = false;
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

        public override bool AttachTo(Character target)
        {
            if (base.AttachTo(target))
            {
                if (Dungeon.level != null)
                    Dungeon.Observe();
                return true;
            }

            return false;
        }

        public override void Detach()
        {
            base.Detach();
            Dungeon.Observe();
        }

        public override bool Act()
        {
            if (target.IsAlive())
            {
                Spend(TICK * 2);

                if (--left <= 0 || Dungeon.hero.VisibleEnemies() > 0)
                    Detach();
            }
            else
            {
                Detach();
            }

            return true;
        }

        public void Prolong()
        {
            left = 2;
        }

        public override int Icon()
        {
            return BuffIndicator.SHADOWS;
        }

        public override float IconFadePercent()
        {
            return 0;
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