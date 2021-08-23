using System.Collections.Generic;
using watabou.utils;
using spdd.actors.mobs;
using spdd.items;
using spdd.items.keys;
using spdd.levels.painters;

namespace spdd.levels.rooms.special
{
    public class VaultRoom : SpecialRoom
    {
        //size is reduced slightly to remove rare AI issues with crystal mimics
        public override int MaxHeight()
        {
            return 8;
        }

        public override int MaxWidth()
        {
            return 8;
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY_SP);
            Painter.Fill(level, this, 2, Terrain.EMPTY);

            int cx = (left + right) / 2;
            int cy = (top + bottom) / 2;
            int c = cx + cy * level.Width();

            Rnd.Shuffle(prizeClasses);

            Item i1, i2;
            i1 = Prize(level);
            i2 = Prize(level);
            level.Drop(i1, c).type = Heap.Type.CRYSTAL_CHEST;

            //level.mobs.Add(Mimic.SpawnAt(c + PathFinder.NEIGHBORS8[pdsharp.utils.Random.Int(8)], i2, typeof(CrystalMimic)));
            if (Rnd.Int(10) == 0)
            {
                level.mobs.Add(Mimic.SpawnAt(c + PathFinder.NEIGHBORS8[Rnd.Int(8)], i2, typeof(CrystalMimic)));
            }
            else
            {
                level.Drop(i2, c + PathFinder.NEIGHBORS8[Rnd.Int(8)]).type = Heap.Type.CRYSTAL_CHEST;
            }
            level.AddItemToSpawn(new CrystalKey(Dungeon.depth));

            Entrance().Set(Door.Type.LOCKED);
            level.AddItemToSpawn(new IronKey(Dungeon.depth));
        }

        private Item Prize(Level level)
        {
            var toRemove = prizeClasses[0];
            prizeClasses.RemoveAt(0);
            Generator.Category cat = toRemove;

            Item prize = null;
            do
            {
                prize = Generator.Random(cat);
            }
            while (prize == null || Challenges.IsItemBlocked(prize));

            return prize;
        }

        private List<Generator.Category> prizeClasses = new List<Generator.Category> {
            Generator.Category.WAND,
            Generator.Category.RING,
            Generator.Category.ARTIFACT
        };
    }
}