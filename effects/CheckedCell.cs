using System;
using watabou.gltextures;
using watabou.noosa;
using watabou.utils;
using spdd.tiles;

namespace spdd.effects
{
    public class CheckedCell : Image
    {
        private float alpha;
        private float delay;

        public CheckedCell(int pos)
            : base(TextureCache.CreateSolid(new Color(0x55, 0xAA, 0xFF, 0xFF)))
        {
            origin.Set(0.5f);

            Point(DungeonTilemap.TileToWorld(pos).Offset(
                DungeonTilemap.SIZE / 2,
                DungeonTilemap.SIZE / 2));

            alpha = 0.8f;
        }

        public CheckedCell(int pos, int visSource)
            : this(pos)
        {
            delay = (Dungeon.level.TrueDistance(pos, visSource) - 1f);
            //steadily accelerates as distance increases
            if (delay > 0)
                delay = (float)Math.Pow(delay, 0.67f) / 10f;
        }

        public override void Update()
        {
            if ((delay -= Game.elapsed) > 0)
            {
                Alpha(0);
            }
            else if ((alpha -= Game.elapsed) > 0)
            {
                Alpha(alpha);
                scale.Set(DungeonTilemap.SIZE * alpha);
            }
            else
            {
                KillAndErase();
            }
        }
    }
}