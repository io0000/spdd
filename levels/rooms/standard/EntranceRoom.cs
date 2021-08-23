using System;
using watabou.utils;
using spdd.items.journal;
using spdd.journal;
using spdd.levels.painters;

namespace spdd.levels.rooms.standard
{
    public class EntranceRoom : StandardRoom
    {
        public override int MinWidth()
        {
            return Math.Max(base.MinWidth(), 5);
        }

        public override int MinHeight()
        {
            return Math.Max(base.MinHeight(), 5);
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            foreach (Room.Door door in connected.Values)
            {
                door.Set(Room.Door.Type.REGULAR);
            }

            do
            {
                level.entrance = level.PointToCell(Random(2));
            }
            while (level.FindMob(level.entrance) != null);

            Painter.Set(level, level.entrance, Terrain.ENTRANCE);

            //use a separate generator here so meta progression doesn't affect levelgen
            Rnd.PushGenerator();

            //places the first guidebook page on floor 1
            if (Dungeon.depth == 1 && !Document.ADVENTURERS_GUIDE.HasPage(Document.GUIDE_INTRO_PAGE))
            {
                int pos;
                do
                {
                    //can't be on bottom row of tiles
                    pos = level.PointToCell(new Point(Rnd.IntRange(left + 1, right - 1),
                        Rnd.IntRange(top + 1, bottom - 2)));
                }
                while (pos == level.entrance || level.FindMob(level.entrance) != null);

                GuidePage p = new GuidePage();
                p.Page(Document.GUIDE_INTRO_PAGE);
                level.Drop(p, pos);
            }

            //places the third guidebook page on floor 2
            if (Dungeon.depth == 2 && !Document.ADVENTURERS_GUIDE.HasPage(Document.GUIDE_SEARCH_PAGE))
            {
                int pos;
                do
                {
                    //can't be on bottom row of tiles
                    pos = level.PointToCell(new Point(Rnd.IntRange(left + 1, right - 1),
                        Rnd.IntRange(top + 1, bottom - 2)));
                }
                while (pos == level.entrance || level.FindMob(level.entrance) != null);

                GuidePage p = new GuidePage();
                p.Page(Document.GUIDE_SEARCH_PAGE);
                level.Drop(p, pos);
            }

            Rnd.PopGenerator();
        }
    }
}