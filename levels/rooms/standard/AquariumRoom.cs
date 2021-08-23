using System;
using spdd.actors.mobs;
using spdd.levels.painters;

namespace spdd.levels.rooms.standard
{
    public class AquariumRoom : StandardRoom
    {
        public override int MinWidth()
        {
            return Math.Max(base.MinWidth(), 7);
        }

        public override int MinHeight()
        {
            return Math.Max(base.MinHeight(), 7);
        }

        public override float[] SizeCatProbs()
        {
            return new float[] { 3, 1, 0 };
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);
            Painter.Fill(level, this, 2, Terrain.EMPTY_SP);
            Painter.Fill(level, this, 3, Terrain.WATER);

            int minDim = Math.Min(Width(), Height());
            int numFish = (minDim - 4) / 3; //1-3 fish, depending on room size

            for (int i = 0; i < numFish; ++i)
            {
                Piranha piranha = new Piranha();
                do
                {
                    piranha.pos = level.PointToCell(Random(3));
                }
                while (level.map[piranha.pos] != Terrain.WATER || level.FindMob(piranha.pos) != null);

                level.mobs.Add(piranha);
            }

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }
        }
    }
}