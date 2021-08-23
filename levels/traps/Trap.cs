using watabou.noosa.audio;
using watabou.utils;
using spdd.scenes;
using spdd.messages;

namespace spdd.levels.traps
{
    public abstract class Trap : IBundlable
    {
        //trap colors
        public const int RED = 0;
        public const int ORANGE = 1;
        public const int YELLOW = 2;
        public const int GREEN = 3;
        public const int TEAL = 4;
        public const int VIOLET = 5;
        public const int WHITE = 6;
        public const int GREY = 7;
        public const int BLACK = 8;

        //trap shapes
        public const int DOTS = 0;
        public const int WAVES = 1;
        public const int GRILL = 2;
        public const int STARS = 3;
        public const int DIAMOND = 4;
        public const int CROSSHAIR = 5;
        public const int LARGE_DOT = 6;

        public string name;  // = Messages.get(this, "name");

        public int color;   // (RED, ORANGE, ..., BLACK)
        public int shape;

        public int pos;

        public bool visible;
        public bool active = true;

        public bool canBeHidden = true;
        public bool canBeSearched = true;

        public Trap()
        {
            name = Messages.Get(this, "name");
        }

        public Trap Set(int pos)
        {
            this.pos = pos;
            return this;
        }

        public Trap Reveal()
        {
            visible = true;
            GameScene.UpdateMap(pos);
            return this;
        }

        public Trap Hide()
        {
            if (canBeHidden)
            {
                visible = false;
                GameScene.UpdateMap(pos);
                return this;
            }
            else
            {
                return Reveal();
            }
        }

        public virtual void Trigger()
        {
            if (active)
            {
                if (Dungeon.level.heroFOV[pos])
                    Sample.Instance.Play(Assets.Sounds.TRAP);

                Disarm();
                Reveal();
                Activate();
            }
        }

        public abstract void Activate();

        public void Disarm()
        {
            active = false;
            Dungeon.level.DisarmTrap(pos);
        }

        private const string POS = "pos";
        private const string VISIBLE = "visible";
        private const string ACTIVE = "active";

        public void RestoreFromBundle(Bundle bundle)
        {
            pos = bundle.GetInt(POS);
            visible = bundle.GetBoolean(VISIBLE);
            if (bundle.Contains(ACTIVE))
                active = bundle.GetBoolean(ACTIVE);
        }

        public void StoreInBundle(Bundle bundle)
        {
            bundle.Put(POS, pos);
            bundle.Put(VISIBLE, visible);
            bundle.Put(ACTIVE, active);
        }

        public string Desc()
        {
            return Messages.Get(this, "desc");
        }
    }
}