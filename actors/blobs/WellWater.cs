using System;
using watabou.utils;
using spdd.actors.hero;
using spdd.items;
using spdd.levels;
using spdd.scenes;
using spdd.journal;

namespace spdd.actors.blobs
{
    // 우물
    public abstract class WellWater : Blob
    {
        protected override void Evolve()
        {
            int cell;
            bool seen = false;

            for (int i = area.top - 1; i <= area.bottom; ++i)
            {
                for (int j = area.left - 1; j <= area.right; ++j)
                {
                    cell = j + i * Dungeon.level.Width();

                    if (Dungeon.level.InsideMap(cell))
                    {
                        off[cell] = cur[cell];
                        volume += off[cell];
                        if (off[cell] > 0 && Dungeon.level.visited[cell])
                        {
                            seen = true;
                        }
                    }
                }
            }

            if (seen)
                Notes.Add(Record());
            else
                Notes.Remove(Record());
        }

        protected bool Affect(int pos)
        {
            Heap heap;

            if (pos == Dungeon.hero.pos && AffectHero(Dungeon.hero))
            {
                cur[pos] = 0;
                return true;
            }
            else if ((heap = Dungeon.level.heaps[pos]) != null)
            {
                var oldItem = heap.Peek();
                var newItem = AffectItem(oldItem, pos);

                if (newItem != null)
                {
                    if (newItem == oldItem)
                    {
                        ; // 의도적
                    }
                    else if (oldItem.Quantity() > 1)
                    {
                        oldItem.Quantity(oldItem.Quantity() - 1);
                        heap.Drop(newItem);
                    }
                    else
                    {
                        heap.Replace(oldItem, newItem);
                    }

                    heap.sprite.Link();
                    cur[pos] = 0;

                    return true;
                }
                else
                {
                    int newPlace;
                    do
                    {
                        newPlace = pos + PathFinder.NEIGHBORS8[Rnd.Int(8)];
                    }
                    while (!Dungeon.level.passable[newPlace] && !Dungeon.level.avoid[newPlace]);

                    Dungeon.level.Drop(heap.PickUp(), newPlace).sprite.Drop(pos);

                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        protected abstract bool AffectHero(Hero hero);

        protected abstract Item AffectItem(Item item, int pos);

        protected abstract Notes.Landmark Record();

        public static void AffectCell(int cell)
        {
            Type[] waters = { typeof(WaterOfHealth), typeof(WaterOfAwareness), typeof(WaterOfTransmutation) };

            foreach (var waterClass in waters)
            {
                WellWater water = (WellWater)Dungeon.level.GetBlob(waterClass);

                if (water != null &&
                    water.volume > 0 &&
                    water.cur[cell] > 0 &&
                    water.Affect(cell))
                {
                    Level.Set(cell, Terrain.EMPTY_WELL);
                    GameScene.UpdateMap(cell);
                    return;
                }
            }
        }
    }
}