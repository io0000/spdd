using System;
using watabou.utils;
using watabou.noosa;
using spdd.tiles;
using spdd.levels.rooms.standard;
using spdd.levels.painters;

namespace spdd.levels.rooms.sewerboss
{
    public class SewerBossExitRoom : ExitRoom
    {
        public override int MinWidth()
        {
            return Math.Max(base.MinWidth(), 8);
        }

        public override int MinHeight()
        {
            return Math.Max(base.MinHeight(), 8);
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            foreach (Room.Door door in connected.Values)
            {
                door.Set(Room.Door.Type.REGULAR);
            }

            Point c = Center();

            Painter.Fill(level, c.x - 1, c.y - 1, 3, 2, Terrain.WALL);
            Painter.Fill(level, c.x - 1, c.y + 1, 3, 1, Terrain.EMPTY_SP);

            level.exit = level.PointToCell(c);
            Painter.Set(level, level.exit, Terrain.LOCKED_EXIT);

            CustomTilemap vis = new SewerExit();
            vis.Pos(c.x - 1, c.y);
            level.customTiles.Add(vis);

            vis = new SewerExitOverhang();
            vis.Pos(c.x - 1, c.y - 2);
            level.customWalls.Add(vis);
        }

        [SPDStatic]
        public class SewerExit : CustomTilemap
        {
            public SewerExit()
            {
                texture = Assets.Environment.SEWER_BOSS;

                tileW = 3;
                tileH = 3;
            }

            static readonly int[] layout = new int[] {
                21, -1, 22,
                23, 23, 23,
                24, 24, 24
            };

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                v.Map(layout, 3);
                return v;
            }

            public override Image Image(int tileX, int tileY)
            {
                if ((tileX == 1 && tileY == 0) || tileY == 2)
                {
                    return null;
                }
                return base.Image(tileX, tileY);
            }
        }

        [SPDStatic]
        public class SewerExitOverhang : CustomTilemap
        {
            public SewerExitOverhang()
            {
                texture = Assets.Environment.SEWER_BOSS;

                tileW = 3;
                tileH = 2;
            }

            static readonly int[] layout = new int[] {
                16, 17, 18,
                19, -1, 20
            };

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                v.Map(layout, 3);
                return v;
            }

            public override Image Image(int tileX, int tileY)
            {
                return null;
            }
        }
    }
}