using watabou.noosa;
using spdd.actors;
using spdd.tiles;

namespace spdd.effects
{
    public class Surprise : Image
    {
        private const float TIME_TO_FADE = 0.8f;
        private float time;

        public Surprise()
            : base(Effects.Get(Effects.Type.EXCLAMATION))
        {
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
                float p = time / TIME_TO_FADE;
                Alpha(p);
                scale.y = 1 + p / 2;
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
                var s = ch.sprite.parent.Recycle<Surprise>();
                ch.sprite.parent.BringToFront(s);
                s.Reset(ch.sprite);
                s.angle = angle;
            }
        }

        //public static void Hit(int pos)
        //{
        //    Hit(pos, 0);
        //}

        public static void Hit(int pos, float angle)
        {
            Group parent = Dungeon.hero.sprite.parent;
            var s = parent.Recycle<Surprise>();
            parent.BringToFront(s);
            s.Reset(pos);
            s.angle = angle;
        }
    }
}