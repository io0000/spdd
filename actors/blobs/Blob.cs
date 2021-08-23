using System;
using watabou.utils;
using spdd.effects;
using spdd.levels;

namespace spdd.actors.blobs
{
    public class Blob : Actor
    {
        public Blob()
        {
            actPriority = BLOB_PRIO;
        }

        public int volume;

        public int[] cur;
        protected int[] off;

        public BlobEmitter emitter;

        public Rect area = new Rect();

        public bool alwaysVisible;

        private const string CUR = "cur";
        private const string START = "start";
        private const string LENGTH = "length";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);

            if (volume > 0)
            {
                int length = Dungeon.level.Length();

                int start;
                for (start = 0; start < length; ++start)
                {
                    if (cur[start] > 0)
                        break;
                }
                int end;
                for (end = length - 1; end > start; --end)
                {
                    if (cur[end] > 0)
                        break;
                }

                bundle.Put(START, start);
                bundle.Put(LENGTH, cur.Length);
                bundle.Put(CUR, Trim(start, end + 1));
            }
        }

        private int[] Trim(int start, int end)
        {
            var len = end - start;
            var copy = new int[len];
            Array.Copy(cur, start, copy, 0, len);
            return copy;
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);

            if (bundle.Contains(CUR))
            {
                cur = new int[bundle.GetInt(LENGTH)];
                off = new int[cur.Length];

                var data = bundle.GetIntArray(CUR);
                var start = bundle.GetInt(START);
                for (int i = 0; i < data.Length; ++i)
                {
                    cur[i + start] = data[i];
                    volume += data[i];
                }
            }
        }

        public override bool Act()
        {
            Spend(TICK);

            if (volume > 0)
            {
                if (area.IsEmpty())
                    SetupArea();

                volume = 0;

                Evolve();
                var tmp = off;
                off = cur;
                cur = tmp;
            }
            else
            { 
                if (!area.IsEmpty())
                {
                    area.SetEmpty();
                    //clear any values remaining in off
                    Array.Copy(cur, 0, off, 0, cur.Length);
                }
            }

            return true;
        }

        public void SetupArea()
        {
            for (int cell = 0; cell < cur.Length; ++cell)
            {
                if (cur[cell] != 0)
                {
                    area.Union(cell % Dungeon.level.Width(), cell / Dungeon.level.Width());
                }
            }
        }

        public virtual void Use(BlobEmitter emitter)
        {
            this.emitter = emitter;
        }

        protected virtual void Evolve()
        {
            bool[] blocking = Dungeon.level.solid;
            int cell;

            int width = Dungeon.level.Width();

            for (int i = area.top - 1; i <= area.bottom; ++i)
            {
                for (int j = area.left - 1; j <= area.right; ++j)
                {
                    cell = j + i * width;
                    if (Dungeon.level.InsideMap(cell))
                    {
                        if (!blocking[cell])
                        {
                            int count = 1;
                            int sum = cur[cell];

                            if (j > area.left && !blocking[cell - 1])
                            {
                                sum += cur[cell - 1];
                                ++count;
                            }
                            if (j < area.right && !blocking[cell + 1])
                            {
                                sum += cur[cell + 1];
                                ++count;
                            }
                            if (i > area.top && !blocking[cell - width])
                            {
                                sum += cur[cell - width];
                                ++count;
                            }
                            if (i < area.bottom && !blocking[cell + width])
                            {
                                sum += cur[cell + width];
                                ++count;
                            }

                            int value = sum >= count ? (sum / count) - 1 : 0;
                            off[cell] = value;

                            if (value > 0)
                            {
                                if (i < area.top)
                                    area.top = i;
                                else if (i >= area.bottom)
                                    area.bottom = i + 1;
                                if (j < area.left)
                                    area.left = j;
                                else if (j >= area.right)
                                    area.right = j + 1;
                            }

                            volume += value;
                        }
                        else
                        {
                            off[cell] = 0;
                        }
                    }
                }
            }
        }

        public virtual void Seed(Level level, int cell, int amount)
        {
            if (cur == null) 
                cur = new int[level.Length()];
            if (off == null) 
                off = new int[cur.Length];
            
            cur[cell] += amount;
            volume += amount;
            
            area.Union(cell % level.Width(), cell / level.Width());
        }

        public virtual void Clear(int cell)
        {
            if (volume == 0) 
                return;

            volume -= cur[cell];
            cur[cell] = 0;
        }

        public virtual void FullyClear()
        {
            volume = 0;
            area.SetEmpty();
            cur = new int[Dungeon.level.Length()];
            off = new int[Dungeon.level.Length()];
        }

        public virtual string TileDesc()
        {
            return null;
        }

        public static Blob Seed(int cell, int amount, Type type)
        {
            return Seed(cell, amount, type, Dungeon.level);
        }

        public static Blob Seed(int cell, int amount, Type type, Level level)
        {
            Blob gas = level.GetBlob(type);

            if (gas == null)
            {
                gas = (Blob)Activator.CreateInstance(type);
            }

            if (gas != null)
            {
                level.blobs[type] = gas;
                gas.Seed(level, cell, amount);
            }

            return gas;
        }

        public static int VolumeAt(int cell, Type type)
        {
            Blob gas = Dungeon.level.GetBlob(type);

            if (gas == null || gas.volume == 0)
            {
                return 0;
            }
            else
            {
                return gas.cur[cell];
            }
        }
    }
}