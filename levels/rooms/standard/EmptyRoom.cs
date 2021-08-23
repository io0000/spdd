using spdd.levels.painters;

namespace spdd.levels.rooms.standard
{
    //other rooms should only extend emptyRoom if they do not add significant terrain
    public class EmptyRoom : StandardRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }
        }
    }
}