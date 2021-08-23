using System;
using watabou.utils;
using spdd.levels.painters;
using spdd.levels.traps;
using spdd.items;

namespace spdd.levels.rooms.secret
{
    public class SecretHoardRoom : SecretRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY);

            Type trapClass;
            if (Rnd.Int(2) == 0)
            {
                trapClass = typeof(RockfallTrap);
            }
            else if (Dungeon.depth >= 10)
            {
                trapClass = typeof(DisintegrationTrap);
            }
            else
            {
                trapClass = typeof(PoisonDartTrap);
            }

            int goldPos;
            //half of the internal space of the room
            int totalGold = ((Width() - 2) * (Height() - 2)) / 2;

            //no matter how much gold it drops, roughly equals 8 gold stacks.
            float goldRatio = 8 / (float)totalGold;
            for (int i = 0; i < totalGold; ++i)
            {
                do
                {
                    goldPos = level.PointToCell(Random());
                }
                while (level.heaps[goldPos] != null);

                Item gold = (new Gold()).Random();
                gold.Quantity((int)Math.Round(gold.Quantity() * goldRatio, MidpointRounding.AwayFromZero));
                level.Drop(gold, goldPos);
            }

            foreach (Point p in GetPoints())
            {
                if (Rnd.Int(2) == 0 && level.map[level.PointToCell(p)] == Terrain.EMPTY)
                {
                    var trap = (Trap)Reflection.NewInstance(trapClass);
                    level.SetTrap(trap.Reveal(), level.PointToCell(p));
                    Painter.Set(level, p, Terrain.TRAP);
                }
            }

            Entrance().Set(Door.Type.HIDDEN);
        }

        public override bool CanPlaceTrap(Point p)
        {
            return false;
        }
    }
}