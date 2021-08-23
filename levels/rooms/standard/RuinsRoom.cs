using spdd.levels.painters;

namespace spdd.levels.rooms.standard
{
    public class RuinsRoom : PatchRoom
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

            //fill scales from ~10% at 4x4, to ~25% at 18x18
            // normal   ~20% to ~25%
            // large    ~25% to ~30%
            // giant    ~30% to ~35%
            float fill = .2f + (Width() * Height()) / 2048f;

            SetupPatch(level, fill, 0, true);
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