using spdd.levels.painters;

namespace spdd.levels.rooms.standard
{
    public class CaveRoom : PatchRoom
    {
        public override float[] SizeCatProbs()
        {
            return new float[] { 9, 3, 1 };
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);
            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }

            //fill scales from ~25% at 4x4, to ~55% at 18x18
            // normal   ~25% to ~35%
            // large    ~35% to ~45%
            // giant    ~45% to ~55%
            float fill = 0.25f + (Width() * Height()) / 1024f;

            SetupPatch(level, fill, 4, true);
            CleanDiagonalEdges();

            for (int i = top + 1; i < bottom; ++i)
            {
                for (int j = left + 1; j < right; ++j)
                {
                    if (patch[XYToPatchCoords(j, i)])
                    {
                        int cell = i * level.Width() + j;
                        level.map[cell] = Terrain.WALL;
                    }
                }
            }
        }
    }
}