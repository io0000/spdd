using watabou.utils;
using spdd.levels.painters;
using spdd.items;
using spdd.items.bombs;
using spdd.actors.mobs;

namespace spdd.levels.rooms.secret
{
    public class SecretHoneypotRoom : SecretRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            Point brokenPotPos = Center();

            brokenPotPos.x = (brokenPotPos.x + Entrance().x) / 2;
            brokenPotPos.y = (brokenPotPos.y + Entrance().y) / 2;

            Honeypot.ShatteredPot pot = new Honeypot.ShatteredPot();
            level.Drop(pot, level.PointToCell(brokenPotPos));

            Bee bee = new Bee();
            bee.Spawn(Dungeon.depth);
            bee.HP = bee.HT;
            bee.pos = level.PointToCell(brokenPotPos);
            level.mobs.Add(bee);

            bee.SetPotInfo(level.PointToCell(brokenPotPos), null);

            PlaceItem(new Honeypot(), level);

            PlaceItem((new Bomb()).Random(), level);

            Entrance().Set(Door.Type.HIDDEN);
        }

        private void PlaceItem(Item item, Level level)
        {
            int itemPos;
            do
            {
                itemPos = level.PointToCell(Random());
            }
            while (level.heaps[itemPos] != null);

            level.Drop(item, itemPos);
        }
    }
}