using watabou.noosa;
using spdd.tiles;

namespace spdd.effects
{
    public class Ripple : Image
    {
        private const float TIME_TO_FADE = 0.5f;

        private float time;

        public Ripple()
            : base(Effects.Get(Effects.Type.RIPPLE))
        { }

        public void Reset(int p)
        {
            Revive();

            x = (p % Dungeon.level.Width()) * DungeonTilemap.SIZE;
            y = (p / Dungeon.level.Width()) * DungeonTilemap.SIZE;

            origin.Set(width / 2, height / 2);
            scale.Set(0);

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
                scale.Set(1 - p);
                Alpha(p);
            }
        }
    }
}