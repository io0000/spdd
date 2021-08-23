using System;
using watabou.utils;
using spdd.items;
using spdd.items.potions;
using spdd.levels.painters;
using spdd.levels.traps;

namespace spdd.levels.rooms.special
{
    public class TrapsRoom : SpecialRoom
    {
        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);

            Type trapClass;
            switch (Rnd.Int(4))
            {
                case 0:
                    trapClass = null;
                    break;
                default:
                    trapClass = Rnd.OneOf(levelTraps[Dungeon.depth / 5]);
                    break;
            }

            if (trapClass == null)
            {
                Painter.Fill(level, this, 1, Terrain.CHASM);
            }
            else
            {
                Painter.Fill(level, this, 1, Terrain.TRAP);
            }

            Door door = Entrance();
            door.Set(Door.Type.REGULAR);

            int lastRow = level.map[left + 1 + (top + 1) * level.Width()] == Terrain.CHASM ? Terrain.CHASM : Terrain.EMPTY;

            int x = -1;
            int y = -1;
            if (door.x == left)
            {
                x = right - 1;
                y = top + Height() / 2;
                Painter.Fill(level, x, top + 1, 1, Height() - 2, lastRow);
            }
            else if (door.x == right)
            {
                x = left + 1;
                y = top + Height() / 2;
                Painter.Fill(level, x, top + 1, 1, Height() - 2, lastRow);
            }
            else if (door.y == top)
            {
                x = left + Width() / 2;
                y = bottom - 1;
                Painter.Fill(level, left + 1, y, Width() - 2, 1, lastRow);
            }
            else if (door.y == bottom)
            {
                x = left + Width() / 2;
                y = top + 1;
                Painter.Fill(level, left + 1, y, Width() - 2, 1, lastRow);
            }

            foreach (Point p in GetPoints())
            {
                int cell = level.PointToCell(p);
                if (level.map[cell] == Terrain.TRAP)
                {
                    var trap = (Trap)Reflection.NewInstance(trapClass);
                    level.SetTrap(trap.Reveal(), cell);
                }
            }

            int pos = x + y * level.Width();
            if (Rnd.Int(3) == 0)
            {
                if (lastRow == Terrain.CHASM)
                {
                    Painter.Set(level, pos, Terrain.EMPTY);
                }
                level.Drop(Prize(level), pos).type = Heap.Type.CHEST;
            }
            else
            {
                Painter.Set(level, pos, Terrain.PEDESTAL);
                level.Drop(Prize(level), pos);
            }

            level.AddItemToSpawn(new PotionOfLevitation());
        }

        private static Item Prize(Level level)
        {
            Item prize;

            if (Rnd.Int(3) != 0)
            {
                prize = level.FindPrizeItem();
                if (prize != null)
                {
                    return prize;
                }
            }

            //1 floor set higher in probability, never cursed
            do
            {
                if (Rnd.Int(2) == 0)
                {
                    prize = Generator.RandomWeapon((Dungeon.depth / 5) + 1);
                }
                else
                {
                    prize = Generator.RandomArmor((Dungeon.depth / 5) + 1);
                }
            }
            while (prize.cursed || Challenges.IsItemBlocked(prize));

            prize.cursedKnown = true;

            //33% chance for an extra update.
            if (Rnd.Int(3) == 0)
            {
                prize.Upgrade();
            }

            return prize;
        }

        private static Type[][] levelTraps = new Type[][]
        {
            //sewers
            new Type[] { typeof(GrippingTrap), typeof(TeleportationTrap), typeof(FlockTrap) },
            //prison
            new Type[] { typeof(PoisonDartTrap), typeof(GrippingTrap), typeof(ExplosiveTrap) },
            //caves
            new Type[] { typeof(PoisonDartTrap), typeof(FlashingTrap), typeof(ExplosiveTrap) },
            //city
            new Type[] { typeof(WarpingTrap), typeof(FlashingTrap), typeof(DisintegrationTrap) },
            //halls, muahahahaha
            new Type[] { typeof(GrimTrap) }
        };
    }
}