using System;
using watabou.utils;
using spdd.levels.painters;
using spdd.levels.traps;

namespace spdd.levels.rooms.standard
{
    public class BurnedRoom : PatchRoom
    {
        public override float[] SizeCatProbs()
        {
            return new float[] { 4, 1, 0 };
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);
            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }

            //past 8x8 each point of width/height decreases fill by 3%
            // e.g. a 14x14 burned room has a fill of 54%
            float fill = Math.Min(1f, 1.48f - (Width() + Height()) * 0.03f);
            SetupPatch(level, fill, 2, false);

            for (int i = top + 1; i < bottom; ++i)
            {
                for (int j = left + 1; j < right; ++j)
                {
                    if (!patch[XYToPatchCoords(j, i)])
                        continue;

                    int cell = i * level.Width() + j;
                    int t;
                    switch (Rnd.Int(5))
                    {
                        case 0:
                        default:
                            t = Terrain.EMPTY;
                            break;
                        case 1:
                            t = Terrain.EMBERS;
                            break;
                        case 2:
                            t = Terrain.TRAP;
                            level.SetTrap((new BurningTrap()).Reveal(), cell);
                            break;
                        case 3:
                            t = Terrain.SECRET_TRAP;
                            level.SetTrap((new BurningTrap()).Hide(), cell);
                            break;
                        case 4:
                            t = Terrain.INACTIVE_TRAP;
                            BurningTrap trap = new BurningTrap();
                            trap.Reveal().active = false;
                            level.SetTrap(trap, cell);
                            break;
                    }
                    level.map[cell] = t;
                }
            }
        }

        public override bool CanPlaceWater(Point p)
        {
            return base.CanPlaceWater(p) && !patch[XYToPatchCoords(p.x, p.y)];
        }

        public override bool CanPlaceGrass(Point p)
        {
            return base.CanPlaceGrass(p) && !patch[XYToPatchCoords(p.x, p.y)];
        }

        public override bool CanPlaceTrap(Point p)
        {
            return base.CanPlaceTrap(p) && !patch[XYToPatchCoords(p.x, p.y)];
        }
    }
}