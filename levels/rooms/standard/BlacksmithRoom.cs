using System;
using watabou.utils;
using spdd.actors.mobs.npcs;
using spdd.items;
using spdd.levels.painters;
using spdd.levels.traps;

namespace spdd.levels.rooms.standard
{
    public class BlacksmithRoom : StandardRoom
    {
        public override int MinWidth()
        {
            return Math.Max(base.MinWidth(), 6);
        }

        public override int MinHeight()
        {
            return Math.Max(base.MinHeight(), 6);
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.TRAP);
            Painter.Fill(level, this, 2, Terrain.EMPTY_SP);

            for (int i = 0; i < 2; ++i)
            {
                int pos;
                do
                {
                    pos = level.PointToCell(Random());
                }
                while (level.map[pos] != Terrain.EMPTY_SP);

                level.Drop(Generator.Random(Rnd.OneOf(
                    Generator.Category.ARMOR,
                    Generator.Category.WEAPON,
                    Generator.Category.MISSILE)),
                    pos);
            }

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
                Painter.DrawInside(level, this, door, 1, Terrain.EMPTY);
            }

            Blacksmith npc = new Blacksmith();
            do
            {
                npc.pos = level.PointToCell(Random(2));
            }
            while (level.heaps[npc.pos] != null);

            level.mobs.Add(npc);

            foreach (Point p in GetPoints())
            {
                int cell = level.PointToCell(p);
                if (level.map[cell] == Terrain.TRAP)
                {
                    level.SetTrap((new BurningTrap()).Reveal(), cell);
                }
            }
        }
    }
}