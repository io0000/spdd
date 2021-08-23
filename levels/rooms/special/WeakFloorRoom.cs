using watabou.noosa;
using watabou.utils;
using spdd.levels.painters;
using spdd.messages;
using spdd.tiles;

namespace spdd.levels.rooms.special
{
    public class WeakFloorRoom : SpecialRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.CHASM);

            Door door = Entrance();
            door.Set(Door.Type.REGULAR);

            Point well = null;

            if (door.x == left)
            {
                for (int i = top + 1; i < bottom; ++i)
                {
                    Painter.DrawInside(level, this, new Point(left, i), Rnd.IntRange(1, Width() - 4), Terrain.EMPTY_SP);
                }
                well = new Point(right - 1, Rnd.Int(2) == 0 ? top + 2 : bottom - 1);
            }
            else if (door.x == right)
            {
                for (int i = top + 1; i < bottom; ++i)
                {
                    Painter.DrawInside(level, this, new Point(right, i), Rnd.IntRange(1, Width() - 4), Terrain.EMPTY_SP);
                }
                well = new Point(left + 1, Rnd.Int(2) == 0 ? top + 2 : bottom - 1);
            }
            else if (door.y == top)
            {
                for (int i = left + 1; i < right; ++i)
                {
                    Painter.DrawInside(level, this, new Point(i, top), Rnd.IntRange(1, Height() - 4), Terrain.EMPTY_SP);
                }
                well = new Point(Rnd.Int(2) == 0 ? left + 1 : right - 1, bottom - 1);
            }
            else if (door.y == bottom)
            {
                for (int i = left + 1; i < right; ++i)
                {
                    Painter.DrawInside(level, this, new Point(i, bottom), Rnd.IntRange(1, Height() - 4), Terrain.EMPTY_SP);
                }
                well = new Point(Rnd.Int(2) == 0 ? left + 1 : right - 1, top + 2);
            }

            Painter.Set(level, well, Terrain.CHASM);
            CustomTilemap vis = new HiddenWell();
            vis.Pos(well.x, well.y);
            level.customTiles.Add(vis);
        }

        [SPDStatic]
        public class HiddenWell : CustomTilemap
        {
            public HiddenWell()
            {
                texture = Assets.Environment.WEAK_FLOOR;
                tileW = tileH = 1;
            }

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                v.Map(new int[] { Dungeon.depth / 5 }, 1);
                return v;
            }

            public override string Name(int tileX, int tileY)
            {
                return Messages.Get(this, "name");
            }

            public override string Desc(int tileX, int tileY)
            {
                return Messages.Get(this, "desc");
            }
        }
    }
}