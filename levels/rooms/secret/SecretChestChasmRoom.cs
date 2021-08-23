using watabou.utils;
using spdd.levels.painters;
using spdd.items;
using spdd.items.keys;
using spdd.items.potions;

namespace spdd.levels.rooms.secret
{
    public class SecretChestChasmRoom : SecretRoom
    {
        //width and height are controlled here so that this room always requires 2 levitation potions

        public override int MinWidth()
        {
            return 8;
        }

        public override int MaxWidth()
        {
            return 9;
        }

        public override int MinHeight()
        {
            return 8;
        }

        public override int MaxHeight()
        {
            return 9;
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.CHASM);

            int chests = 0;

            Point p = new Point(left + 3, top + 3);
            Painter.Set(level, p, Terrain.EMPTY_SP);
            level.Drop(Generator.Random(), level.PointToCell(p)).type = Heap.Type.LOCKED_CHEST;
            if (level.heaps[level.PointToCell(p)] != null)
            {
                ++chests;
            }

            p.x = right - 3;
            Painter.Set(level, p, Terrain.EMPTY_SP);
            level.Drop(Generator.Random(), level.PointToCell(p)).type = Heap.Type.LOCKED_CHEST;
            if (level.heaps[level.PointToCell(p)] != null)
            {
                ++chests;
            }

            p.y = bottom - 3;
            Painter.Set(level, p, Terrain.EMPTY_SP);
            level.Drop(Generator.Random(), level.PointToCell(p)).type = Heap.Type.LOCKED_CHEST;
            if (level.heaps[level.PointToCell(p)] != null)
            {
                ++chests;
            }

            p.x = left + 3;
            Painter.Set(level, p, Terrain.EMPTY_SP);
            level.Drop(Generator.Random(), level.PointToCell(p)).type = Heap.Type.LOCKED_CHEST;
            if (level.heaps[level.PointToCell(p)] != null)
            {
                ++chests;
            }

            p = new Point(left + 1, top + 1);
            Painter.Set(level, p, Terrain.EMPTY_SP);
            if (chests > 0)
            {
                level.Drop(new GoldenKey(Dungeon.depth), level.PointToCell(p));
                --chests;
            }

            p.x = right - 1;
            Painter.Set(level, p, Terrain.EMPTY_SP);
            if (chests > 0)
            {
                level.Drop(new GoldenKey(Dungeon.depth), level.PointToCell(p));
                --chests;
            }

            p.y = bottom - 1;
            Painter.Set(level, p, Terrain.EMPTY_SP);
            if (chests > 0)
            {
                level.Drop(new GoldenKey(Dungeon.depth), level.PointToCell(p));
                --chests;
            }

            p.x = left + 1;
            Painter.Set(level, p, Terrain.EMPTY_SP);
            if (chests > 0)
            {
                level.Drop(new GoldenKey(Dungeon.depth), level.PointToCell(p));
                --chests;
            }

            level.AddItemToSpawn(new PotionOfLevitation());

            Entrance().Set(Door.Type.HIDDEN);
        }
    }
}