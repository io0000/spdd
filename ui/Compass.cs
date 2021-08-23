using System;
using watabou.noosa;
using watabou.utils;
using spdd.tiles;

namespace spdd.ui
{
    public class Compass : Image
    {
        private const float RAD_2_G = 180f / 3.1415926f;
        private const float RADIUS = 12;

        private int cell;
        private PointF cellCenter;

        private PointF lastScroll = new PointF();

        public Compass(int cell)
        {
            Copy(Icons.COMPASS.Get());
            origin.Set(width / 2, RADIUS);

            this.cell = cell;
            cellCenter = DungeonTilemap.TileCenterToWorld(cell);
            visible = false;
        }

        public override void Update()
        {
            base.Update();

            if (cell < 0 || cell >= Dungeon.level.Length())
            {
                visible = false;
                return;
            }

            if (!visible)
            {
                visible = Dungeon.level.visited[cell] || Dungeon.level.mapped[cell];
            }

            if (visible)
            {
                PointF scroll = Camera.main.scroll;
                if (!scroll.Equals(lastScroll))
                {
                    lastScroll.Set(scroll);
                    PointF center = Camera.main.Center().Offset(scroll);
                    angle = (float)Math.Atan2(cellCenter.x - center.x, center.y - cellCenter.y) * RAD_2_G;
                }
            }
        }
    }
}