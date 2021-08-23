using System;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.effects;
using spdd.sprites;
using spdd.levels.traps;
using spdd.mechanics;

namespace spdd.items.stones
{
    public class StoneOfDisarming : Runestone
    {
        private const int DIST = 8;

        public StoneOfDisarming()
        {
            image = ItemSpriteSheet.STONE_DISARM;
        }

        protected override void Activate(int cell)
        {
            bool[] FOV = new bool[Dungeon.level.Length()];
            Point c = Dungeon.level.CellToPoint(cell);
            ShadowCaster.CastShadow(c.x, c.y, FOV, Dungeon.level.losBlocking, DIST);

            int sX = Math.Max(0, c.x - DIST);
            int eX = Math.Min(Dungeon.level.Width() - 1, c.x + DIST);

            int sY = Math.Max(0, c.y - DIST);
            int eY = Math.Min(Dungeon.level.Height() - 1, c.y + DIST);

            List<Trap> disarmCandidates = new List<Trap>();

            int w = Dungeon.level.Width();

            for (int y = sY; y <= eY; ++y)
            {
                int curr = y * w + sX;
                for (int x = sX; x <= eX; ++x)
                {
                    if (FOV[curr])
                    {
                        Trap t = Dungeon.level.traps[curr];
                        if (t != null && t.active)
                        {
                            disarmCandidates.Add(t);
                        }
                    }
                    ++curr;
                }
            }

            disarmCandidates.Sort(new Comparer(this, cell));

            //disarms at most nine traps
            while (disarmCandidates.Count > 9)
            {
                disarmCandidates.RemoveAt(9);
            }

            foreach (Trap t in disarmCandidates)
            {
                t.Reveal();
                t.Disarm();
                CellEmitter.Get(t.pos).Burst(Speck.Factory(Speck.STEAM), 6);
            }

            Sample.Instance.Play(Assets.Sounds.TELEPORT);
        }

        private class Comparer : IComparer<Trap>
        {
            private StoneOfDisarming stone;
            private int cell;

            public Comparer(StoneOfDisarming stone, int cell)
            {
                this.stone = stone;
                this.cell = cell;
            }

            public int Compare(Trap o1, Trap o2)
            {
                float diff = Dungeon.level.TrueDistance(cell, o1.pos) - Dungeon.level.TrueDistance(cell, o2.pos);
                if (diff < 0)
                {
                    return -1;
                }
                else if (diff == 0)
                {
                    return Rnd.Int(2) == 0 ? -1 : 1;
                }
                else
                {
                    return 1;
                }
            }
        }
    }
}