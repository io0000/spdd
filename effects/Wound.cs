using watabou.noosa;
using spdd.actors;
using spdd.tiles;

namespace spdd.effects
{
    public class Wound : Image
    {
        private const float TIME_TO_FADE = 0.8f;

        private float time;

        public Wound()
            : base(Effects.Get(Effects.Type.WOUND))
        {
            Hardlight(1f, 0f, 0f);
            origin.Set(width / 2, height / 2);
        }

        public void Reset(int p)
        {
            Revive();

            x = (p % Dungeon.level.Width()) * DungeonTilemap.SIZE + (DungeonTilemap.SIZE - width) / 2;
            y = (p / Dungeon.level.Width()) * DungeonTilemap.SIZE + (DungeonTilemap.SIZE - height) / 2;

            time = TIME_TO_FADE;
        }

        public void Reset(Visual v)
        {
            Revive();

            Point(v.Center(this));

            time = TIME_TO_FADE;
        }

        public override void Update()
        {
            base.Update();

            if ((time -= Game.elapsed) <= 0)
            {
                Kill();
            }
            else
            {
                var p = time / TIME_TO_FADE;
                Alpha(p);
                scale.x = 1 + p;
            }
        }

        public static void Hit(Character ch)
        {
            Hit(ch, 0);
        }

        public static void Hit(Character ch, float angle)
        {
            if (ch.sprite.parent != null)
            {
                var w = ch.sprite.parent.Recycle<Wound>();
                ch.sprite.parent.BringToFront(w);
                w.Reset(ch.sprite);
                w.angle = angle;
            }
        }

        public static void Hit(int pos)
        {
            Hit(pos, 0);
        }

        public static void Hit(int pos, float angle)
        {
            var parent = Dungeon.hero.sprite.parent;
            var w = parent.Recycle<Wound>();
            parent.BringToFront(w);
            w.Reset(pos);
            w.angle = angle;
        }
    }
}