using watabou.utils;
using spdd.utils;

namespace spdd.levels.rooms.standard
{
    //This room type uses the patch system to fill itself in in some manner
    //it's still up to the specific room to implement paint, but utility methods are provided
    public abstract class PatchRoom : StandardRoom
    {
        protected bool[] patch;

        protected void SetupPatch(Level level, float fill, int clustering, bool ensurePath)
        {
            if (ensurePath)
            {
                PathFinder.SetMapSize(Width() - 2, Height() - 2);
                bool valid;
                do
                {
                    patch = Patch.Generate(Width() - 2, Height() - 2, fill, clustering, true);
                    int startPoint = 0;
                    foreach (Door door in connected.Values)
                    {
                        if (door.x == left)
                        {
                            startPoint = XYToPatchCoords(door.x + 1, door.y);
                            patch[XYToPatchCoords(door.x + 1, door.y)] = false;
                            patch[XYToPatchCoords(door.x + 2, door.y)] = false;
                        }
                        else if (door.x == right)
                        {
                            startPoint = XYToPatchCoords(door.x - 1, door.y);
                            patch[XYToPatchCoords(door.x - 1, door.y)] = false;
                            patch[XYToPatchCoords(door.x - 2, door.y)] = false;
                        }
                        else if (door.y == top)
                        {
                            startPoint = XYToPatchCoords(door.x, door.y + 1);
                            patch[XYToPatchCoords(door.x, door.y + 1)] = false;
                            patch[XYToPatchCoords(door.x, door.y + 2)] = false;
                        }
                        else if (door.y == bottom)
                        {
                            startPoint = XYToPatchCoords(door.x, door.y - 1);
                            patch[XYToPatchCoords(door.x, door.y - 1)] = false;
                            patch[XYToPatchCoords(door.x, door.y - 2)] = false;
                        }
                    }

                    PathFinder.BuildDistanceMap(startPoint, BArray.Not(patch, null));

                    valid = true;
                    for (int i = 0; i < patch.Length; ++i)
                    {
                        if (!patch[i] && PathFinder.distance[i] == int.MaxValue)
                        {
                            valid = false;
                            break;
                        }
                    }
                }
                while (!valid);

                PathFinder.SetMapSize(level.Width(), level.Height());
            }
            else
            {
                patch = Patch.Generate(Width() - 2, Height() - 2, fill, clustering, true);
            }
        }

        //removes all diagonal-only adjacent filled patch areas, handy for making things look cleaner
        //note that this will reduce the fill rate very slightly
        protected void CleanDiagonalEdges()
        {
            if (patch == null)
                return;

            int pWidth = Width() - 2;

            for (int i = 0; i < patch.Length - pWidth; ++i)
            {
                if (!patch[i])
                    continue;

                //we don't need to check above because we are either at the top
                // or have already dealt with those tiles

                //down-left
                if (i % pWidth != 0)
                {
                    if (patch[i - 1 + pWidth] && !(patch[i - 1] || patch[i + pWidth]))
                    {
                        patch[i - 1 + pWidth] = false;
                    }
                }

                //down-right
                if ((i + 1) % pWidth != 0)
                {
                    if (patch[i + 1 + pWidth] && !(patch[i + 1] || patch[i + pWidth]))
                    {
                        patch[i + 1 + pWidth] = false;
                    }
                }
            }
        }

        protected int XYToPatchCoords(int x, int y)
        {
            return (x - left - 1) + ((y - top - 1) * (Width() - 2));
        }
    }
}