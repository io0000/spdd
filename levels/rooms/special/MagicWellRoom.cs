using System;
using watabou.utils;
using spdd.levels.painters;
using spdd.actors.blobs;

namespace spdd.levels.rooms.special
{
    public class MagicWellRoom : SpecialRoom
    {
        private static readonly Type[] WATERS = new Type[] {
            typeof(WaterOfAwareness),
            typeof(WaterOfHealth)
        };

        public Type overrideWater;

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            Point c = Center();
            Painter.Set(level, c.x, c.y, Terrain.WELL);

            Type waterClass =
                overrideWater != null ?
                overrideWater :
                Rnd.Element(WATERS);

            WellWater.Seed(c.x + level.Width() * c.y, 1, waterClass, level);

            Entrance().Set(Door.Type.REGULAR);
        }
    }
}