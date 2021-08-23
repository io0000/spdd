using spdd.actors.buffs;
using spdd.effects;
using spdd.effects.particles;
using spdd.messages;
using spdd.items;

namespace spdd.actors.blobs
{
    public class Freezing : Blob
    {
        protected override void Evolve()
        {
            int cell;

            Fire fire = (Fire)Dungeon.level.GetBlob(typeof(Fire));

            for (int i = area.left - 1; i <= area.right; ++i)
            {
                for (int j = area.top - 1; j <= area.bottom; ++j)
                {
                    cell = i + j * Dungeon.level.Width();

                    if (cur[cell] > 0)
                    {
                        if (fire != null && fire.volume > 0 && fire.cur[cell] > 0)
                        {
                            fire.Clear(cell);
                            off[cell] = cur[cell] = 0;
                            continue;
                        }

                        Freezing.Freeze(cell);

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

        public static void Freeze(int cell)
        {
            var ch = Actor.FindChar(cell);
            if (ch != null && !ch.IsImmune(typeof(Freezing)))
            {
                if (ch.FindBuff<Frost>() != null)
                {
                    Buff.Affect<Frost>(ch, 2.0f);
                }
                else
                {
                    Buff.Affect<Chill>(ch, Dungeon.level.water[cell] ? 5f : 3f);
                    Chill chill = ch.FindBuff<Chill>();
                    if (chill != null && chill.Cooldown() >= Chill.DURATION)
                        Buff.Affect<Frost>(ch, Frost.DURATION);
                }
            }

            Heap heap = Dungeon.level.heaps[cell];
            if (heap != null)
                heap.Freeze();
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);
            emitter.Start(SnowParticle.Factory, 0.05f, 0);
        }

        public override string TileDesc()
        {
            return Messages.Get(this, "desc");
        }

        //legacy functionality from before this was a proper blob. Returns true if this cell is visible
        public static bool Affect(int cell, Fire fire)
        {
            Character ch = Actor.FindChar(cell);
            if (ch != null)
            {
                if (Dungeon.level.water[ch.pos])
                {
                    Buff.Prolong<Frost>(ch, Frost.DURATION * 3);
                }
                else
                {
                    Buff.Prolong<Frost>(ch, Frost.DURATION);
                }
            }

            if (fire != null)
                fire.Clear(cell);

            Heap heap = Dungeon.level.heaps[cell];
            if (heap != null)
                heap.Freeze();

            if (Dungeon.level.heroFOV[cell])
            {
                CellEmitter.Get(cell).Start(SnowParticle.Factory, 0.2f, 6);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}