using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors.blobs;
using spdd.items;
using spdd.items.journal;
using spdd.items.keys;
using spdd.items.potions;
using spdd.journal;
using spdd.levels.painters;

namespace spdd.levels.rooms.special
{
    public class LaboratoryRoom : SpecialRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY_SP);

            Door entrance = this.Entrance();

            Point pot = null;
            if (entrance.x == left)
            {
                pot = new Point(right - 1, Rnd.Int(2) == 0 ? top + 1 : bottom - 1);
            }
            else if (entrance.x == right)
            {
                pot = new Point(left + 1, Rnd.Int(2) == 0 ? top + 1 : bottom - 1);
            }
            else if (entrance.y == top)
            {
                pot = new Point(Rnd.Int(2) == 0 ? left + 1 : right - 1, bottom - 1);
            }
            else if (entrance.y == bottom)
            {
                pot = new Point(Rnd.Int(2) == 0 ? left + 1 : right - 1, top + 1);
            }
            Painter.Set(level, pot, Terrain.ALCHEMY);

            int chapter = 1 + Dungeon.depth / 5;
            Blob.Seed(pot.x + level.Width() * pot.y, 1 + chapter * 10 + Rnd.NormalIntRange(0, 10), typeof(Alchemy), level);

            int n = Rnd.NormalIntRange(1, 3);
            for (int i = 0; i < n; ++i)
            {
                int pos;
                do
                {
                    pos = level.PointToCell(Random());
                }
                while (level.map[pos] != Terrain.EMPTY_SP ||
                       level.heaps[pos] != null);

                level.Drop(Prize(level), pos);
            }

            //guide pages
            List<string> allPages = Document.ALCHEMY_GUIDE.Pages();
            List<string> missingPages = new List<string>();
            foreach (string page in allPages)
            {
                if (!Document.ALCHEMY_GUIDE.HasPage(page))
                {
                    missingPages.Add(page);
                }
            }

            //4 pages in sewers, 6 in prison, 9 in caves+
            int chapterTarget;
            if (missingPages.Count <= 3)
            {
                chapterTarget = 3;
            }
            else if (missingPages.Count <= 5)
            {
                chapterTarget = 2;
            }
            else
            {
                chapterTarget = 1;
            }

            if (missingPages.Count > 0 && chapter >= chapterTarget)
            {
                //for each chapter ahead of the target chapter, drop 1 additional page
                int pagesToDrop = Math.Min(missingPages.Count, (chapter - chapterTarget) + 1);

                for (int i = 0; i < pagesToDrop; ++i)
                {
                    AlchemyPage p = new AlchemyPage();

                    var toRemove = missingPages[0];
                    missingPages.RemoveAt(0);

                    p.Page(toRemove);
                    int pos;
                    do
                    {
                        pos = level.PointToCell(Random());
                    }
                    while (level.map[pos] != Terrain.EMPTY_SP || level.heaps[pos] != null);

                    level.Drop(p, pos);
                }
            }

            if (level is RegularLevel && ((RegularLevel)level).HasPitRoom())
            {
                entrance.Set(Door.Type.REGULAR);
            }
            else
            {
                entrance.Set(Door.Type.LOCKED);
                level.AddItemToSpawn(new IronKey(Dungeon.depth));
            }
        }

        private static Item Prize(Level level)
        {
            Item prize = level.FindPrizeItem(typeof(Potion));
            if (prize == null)
            {
                prize = Generator.Random(Rnd.OneOf(Generator.Category.POTION, Generator.Category.STONE));
            }

            return prize;
        }
    }
}