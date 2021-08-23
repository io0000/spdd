using System;
using watabou.utils;
using watabou.noosa.audio;
using spdd.sprites;
using spdd.scenes;
using spdd.mechanics;
using spdd.effects;
using spdd.items.scrolls;

namespace spdd.items.stones
{
    public class StoneOfClairvoyance : Runestone
    {
        private const int DIST = 12;

        public StoneOfClairvoyance()
        {
            image = ItemSpriteSheet.STONE_CLAIRVOYANCE;
        }

        protected override void Activate(int cell)
        {
            Point c = Dungeon.level.CellToPoint(cell);

            int[] rounding = ShadowCaster.rounding[DIST];

            int left, right;
            int curr;
            bool noticed = false;

            int w = Dungeon.level.Width();
            int h = Dungeon.level.Height();

            for (int y = Math.Max(0, c.y - DIST); y <= Math.Min(h - 1, c.y + DIST); ++y)
            {
                int value = rounding[Math.Abs(c.y - y)];
                if (value < Math.Abs(c.y - y))
                {
                    left = c.x - value;
                }
                else
                {
                    left = DIST;
                    while (rounding[left] < value)
                        --left;

                    left = c.x - left;
                }

                right = Math.Min(w - 1, c.x + c.x - left);
                left = Math.Max(0, left);

                for (curr = left + y * w; curr <= right + y * w; ++curr)
                {
                    GameScene.EffectOverFog(new CheckedCell(curr, cell));
                    Dungeon.level.mapped[curr] = true;

                    if (Dungeon.level.secret[curr])
                    {
                        Dungeon.level.Discover(curr);

                        if (Dungeon.level.heroFOV[curr])
                        {
                            GameScene.DiscoverTile(curr, Dungeon.level.map[curr]);
                            ScrollOfMagicMapping.Discover(curr);
                            noticed = true;
                        }
                    }
                }
            }

            if (noticed)
            {
                Sample.Instance.Play(Assets.Sounds.SECRET);
            }

            Sample.Instance.Play(Assets.Sounds.TELEPORT);
            GameScene.UpdateFog();
        }
    }
}