using System;
using watabou.utils;
using spdd.actors.buffs;
using spdd.effects;
using spdd.effects.particles;
using spdd.items;
using spdd.items.wands;
using spdd.items.weapon.melee;
using spdd.messages;
using spdd.utils;

namespace spdd.actors.blobs
{
    public class Electricity : Blob
    {
        public Electricity()
        {
            //acts after mobs, to give them a chance to resist paralysis
            actPriority = MOB_PRIO - 1;
        }

        private bool[] water;

        protected override void Evolve()
        {
            water = Dungeon.level.water;
            int cell;

            //spread first..
            for (int i = area.left - 1; i <= area.right; ++i)
            {
                for (int j = area.top - 1; j <= area.bottom; ++j)
                {
                    cell = i + j * Dungeon.level.Width();

                    if (cur[cell] > 0)
                        SpreadFromCell(cell, cur[cell]);
                }
            }

            //..then decrement/shock
            for (int i = area.left - 1; i <= area.right; ++i)
            {
                for (int j = area.top - 1; j <= area.bottom; ++j)
                {
                    cell = i + j * Dungeon.level.Width();

                    if (cur[cell] > 0)
                    {
                        Character ch = Actor.FindChar(cell);
                        if (ch != null && !ch.IsImmune(GetType()))
                        {
                            Buff.Prolong<Paralysis>(ch, 1f);
                            if (cur[cell] % 2 == 1)
                            {
                                ch.Damage((int)Math.Round(Rnd.Float(2 + Dungeon.depth / 5f), MidpointRounding.AwayFromZero), this);
                                if (!ch.IsAlive() && ch == Dungeon.hero)
                                {
                                    Dungeon.Fail(GetType());
                                    GLog.Negative(Messages.Get(this, "ondeath"));
                                }
                            }
                        }

                        Heap h = Dungeon.level.heaps[cell];
                        if (h != null)
                        {
                            Item toShock = h.Peek();
                            if (toShock is Wand)
                            {
                                ((Wand)toShock).GainCharge(0.333f);
                            }
                            else if (toShock is MagesStaff)
                            {
                                ((MagesStaff)toShock).GainCharge(0.333f);
                            }
                        }

                        off[cell] = cur[cell] - 1;
                        volume += off[cell];
                    }
                    else
                    {
                        off[cell] = 0;
                    }
                }
            }
        }

        private void SpreadFromCell(int cell, int power)
        {
            if (cur[cell] == 0)
            {
                area.Union(cell % Dungeon.level.Width(), cell / Dungeon.level.Width());
            }

            cur[cell] = Math.Max(cur[cell], power);

            foreach (int c in PathFinder.NEIGHBORS4)
            {
                if (water[cell + c] && cur[cell + c] < power)
                {
                    SpreadFromCell(cell + c, power);
                }
            }
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);
            emitter.Start(SparkParticle.Factory, 0.05f, 0);
        }

        public override string TileDesc()
        {
            return Messages.Get(this, "desc");
        }
    }
}