using System.Linq;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.effects;

namespace spdd.items.bombs
{
    public class Noisemaker : Bomb
    {
        public Noisemaker()
        {
            image = ItemSpriteSheet.NOISEMAKER;
        }

        public void SetTrigger(int cell)
        {
            Buff.Affect<Trigger>(Dungeon.hero).Set(cell);

            CellEmitter.Center(cell).Start(Speck.Factory(Speck.SCREAM), 0.3f, 3);
            Sample.Instance.Play(Assets.Sounds.ALERT);

            foreach (var mob in Dungeon.level.mobs.ToArray())
                mob.Beckon(cell);
        }

        [SPDStatic]
        public class Trigger : Buff
        {
            int cell;
            int floor;
            int left;

            public void Set(int cell)
            {
                floor = Dungeon.depth;
                this.cell = cell;
                left = 6;
            }

            public override bool Act()
            {
                if (Dungeon.depth != floor)
                {
                    Spend(TICK);
                    return true;
                }

                Noisemaker bomb = null;
                Heap heap = Dungeon.level.heaps[cell];

                if (heap != null)
                {
                    foreach (Item i in heap.items)
                    {
                        if (i is Noisemaker)
                        {
                            bomb = (Noisemaker)i;
                            break;
                        }
                    }
                }

                if (bomb == null)
                {
                    Detach();
                }
                else if (Actor.FindChar(cell) != null)
                {
                    heap.items.Remove(bomb);
                    if (heap.items.Count == 0)
                        heap.Destroy();

                    Detach();
                    bomb.Explode(cell);
                }
                else
                {
                    Spend(TICK);

                    --left;

                    if (left <= 0)
                    {
                        CellEmitter.Center(cell).Start(Speck.Factory(Speck.SCREAM), 0.3f, 3);
                        Sample.Instance.Play(Assets.Sounds.ALERT);

                        foreach (var mob in Dungeon.level.mobs.ToArray())
                            mob.Beckon(cell);

                        left = 6;
                    }
                }

                return true;
            }

            private const string CELL = "cell";
            private const string FLOOR = "floor";
            private const string LEFT = "left";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(CELL, cell);
                bundle.Put(FLOOR, floor);
                bundle.Put(LEFT, left);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                cell = bundle.GetInt(CELL);
                floor = bundle.GetInt(FLOOR);
                left = bundle.GetInt(LEFT);
            }
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (20 + 40);
        }
    }
}