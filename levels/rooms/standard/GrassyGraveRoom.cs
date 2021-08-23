using System;
using watabou.utils;
using spdd.levels.painters;
using spdd.items;

namespace spdd.levels.rooms.standard
{
    public class GrassyGraveRoom : StandardRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }

            Painter.Fill(level, this, 1, Terrain.GRASS);

            int w = Width() - 2;
            int h = Height() - 2;
            int nGraves = Math.Max(w, h) / 2;

            int index = Rnd.Int(nGraves);

            int shift = Rnd.Int(2);
            for (int i = 0; i < nGraves; ++i)
            {
                int pos = w > h ?
                    left + 1 + shift + i * 2 + (top + 2 + Rnd.Int(h - 2)) * level.Width() :
                    (left + 2 + Rnd.Int(w - 2)) + (top + 1 + shift + i * 2) * level.Width();
                level.Drop(i == index ? Generator.Random() : (new Gold()).Random(), pos).type = Heap.Type.TOMB;
            }
        }
    }
}