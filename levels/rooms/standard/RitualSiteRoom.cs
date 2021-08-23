using System;
using watabou.utils;
using watabou.noosa;
using spdd.levels.painters;
using spdd.items.quest;
using spdd.tiles;
using spdd.messages;

namespace spdd.levels.rooms.standard
{
    public class RitualSiteRoom : StandardRoom
    {
        public override int MinWidth()
        {
            return Math.Max(base.MinWidth(), 5);
        }

        public override int MinHeight()
        {
            return Math.Max(base.MinHeight(), 5);
        }

        public override void Paint(Level level)
        {
            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }

            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            RitualMarker vis = new RitualMarker();
            Point c = Center();
            vis.Pos(c.x - 1, c.y - 1);

            level.customTiles.Add(vis);

            Painter.Fill(level, c.x - 1, c.y - 1, 3, 3, Terrain.EMPTY_DECO);

            level.AddItemToSpawn(new CeremonialCandle());
            level.AddItemToSpawn(new CeremonialCandle());
            level.AddItemToSpawn(new CeremonialCandle());
            level.AddItemToSpawn(new CeremonialCandle());

            CeremonialCandle.ritualPos = c.x + (level.Width() * c.y);
        }

        [SPDStatic]
        public class RitualMarker : CustomTilemap
        {
            public RitualMarker()
            {
                texture = Assets.Environment.PRISON_QUEST;

                tileW = tileH = 3;
            }

            const int TEX_WIDTH = 64;

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                v.Map(MapSimpleImage(0, 0, TEX_WIDTH), 3);
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