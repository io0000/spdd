using watabou.utils;
using watabou.noosa;
using spdd.ui;
using spdd.tiles;

namespace spdd.effects
{
    public class TargetedCell : Image
    {
        private float alpha;

        public TargetedCell(int pos, Color color)
            : base(Icons.TARGET.Get())
        {
            Hardlight(color);

            origin.Set(width / 2f);

            Point(DungeonTilemap.TileToWorld(pos));

            alpha = 1f;
        }

        public override void Update()
        {
            if ((alpha -= Game.elapsed / 2f) > 0)
            {
                Alpha(alpha);
                scale.Set(alpha);
            }
            else
            {
                KillAndErase();
            }
        }
    }
}