using System.Collections.Generic;
using watabou.noosa;
using watabou.utils;
using spdd.actors.mobs;
using spdd.items;
using spdd.items.potions;
using spdd.items.quest;
using spdd.levels.painters;
using spdd.messages;
using spdd.tiles;

namespace spdd.levels.rooms.special
{
    public class MassGraveRoom : SpecialRoom
    {
        public override int MinWidth()
        {
            return 7;
        }
        public override int MinHeight()
        {
            return 7;
        }

        public override void Paint(Level level)
        {
            Door entrance = Entrance();
            entrance.Set(Door.Type.BARRICADE);
            level.AddItemToSpawn(new PotionOfLiquidFlame());

            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY_SP);

            Bones b = new Bones();

            b.SetRect(left + 1, top, Width() - 2, Height() - 1);
            level.customTiles.Add(b);

            //50% 1 skeleton, 50% 2 skeletons
            for (int i = 0; i <= Rnd.Int(2); ++i)
            {
                Skeleton skele = new Skeleton();

                int pos;
                do
                {
                    pos = level.PointToCell(Random());
                }
                while (level.map[pos] != Terrain.EMPTY_SP || level.FindMob(pos) != null);

                skele.pos = pos;
                level.mobs.Add(skele);
            }

            List<Item> items = new List<Item>();
            //100% corpse dust, 2x100% 1 coin, 2x30% coins, 1x60% random item, 1x30% armor
            items.Add(new CorpseDust());
            items.Add(new Gold(1));
            items.Add(new Gold(1));
            if (Rnd.Float() <= 0.3f)
                items.Add(new Gold());

            if (Rnd.Float() <= 0.3f)
                items.Add(new Gold());

            if (Rnd.Float() <= 0.6f)
                items.Add(Generator.Random());

            if (Rnd.Float() <= 0.3f)
                items.Add(Generator.RandomArmor());

            foreach (Item item in items)
            {
                int pos;
                do
                {
                    pos = level.PointToCell(Random());
                }
                while (level.map[pos] != Terrain.EMPTY_SP || level.heaps[pos] != null);

                Heap h = level.Drop(item, pos);
                h.SetHauntedIfCursed();
                h.type = Heap.Type.SKELETON;
            }
        }

        [SPDStatic]
        public class Bones : CustomTilemap
        {
            private const int WALL_OVERLAP = 3;
            private const int FLOOR = 7;

            public Bones()
            {
                texture = Assets.Environment.PRISON_QUEST;
            }

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                int[] data = new int[tileW * tileH];
                for (int i = 0; i < data.Length; ++i)
                {
                    if (i < tileW)
                        data[i] = WALL_OVERLAP;
                    else
                        data[i] = FLOOR;
                }
                v.Map(data, tileW);
                return v;
            }

            public override Image Image(int tileX, int tileY)
            {
                if (tileY == 0)
                {
                    return null;
                }
                else
                {
                    return base.Image(tileX, tileY);
                }
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