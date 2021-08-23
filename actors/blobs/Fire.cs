using spdd.actors.buffs;
using spdd.effects;
using spdd.effects.particles;
using spdd.scenes;
using spdd.messages;

namespace spdd.actors.blobs
{
    public class Fire : Blob
    {
        protected override void Evolve()
        {
            var flamable = Dungeon.level.flamable;
            int cell;
            int fire;

            Freezing freeze = (Freezing)Dungeon.level.GetBlob(typeof(Freezing));

            var observe = false;

            int levelWidth = Dungeon.level.Width();

            for (int i = area.left - 1; i <= area.right; ++i)
            {
                for (int j = area.top - 1; j <= area.bottom; ++j)
                {
                    cell = i + j * Dungeon.level.Width();
                    if (cur[cell] > 0)
                    {
                        if (freeze != null && freeze.volume > 0 && freeze.cur[cell] > 0)
                        {
                            freeze.Clear(cell);
                            off[cell] = cur[cell] = 0;
                            continue;
                        }

                        Burn(cell);

                        fire = cur[cell] - 1;
                        if (fire <= 0 && flamable[cell])
                        {
                            Dungeon.level.Destroy(cell);

                            observe = true;
                            GameScene.UpdateMap(cell);
                        }
                    }
                    else if (freeze == null || freeze.volume <= 0 || freeze.cur[cell] <= 0)
                    {
                        if (flamable[cell] &&
                            (cur[cell - 1] > 0 ||
                            cur[cell + 1] > 0 ||
                            cur[cell - levelWidth] > 0 ||
                            cur[cell + levelWidth] > 0))
                        {
                            fire = 4;
                            Burn(cell);
                            area.Union(i, j);
                        }
                        else
                        {
                            fire = 0;
                        }
                    }
                    else
                    {
                        fire = 0;
                    }

                    volume += (off[cell] = fire);
                }
            }

            if (observe)
                Dungeon.Observe();
        }

        public static void Burn(int pos)
        {
            var ch = FindChar(pos);
            if (ch != null && !ch.IsImmune(typeof(Fire)))
            {
                Buff.Affect<Burning>(ch).Reignite(ch);
            }

            var heap = Dungeon.level.heaps[pos];
            if (heap != null)
                heap.Burn();

            var plant = Dungeon.level.plants[pos];
            if (plant != null)
                plant.Wither();
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);
            emitter.Pour(FlameParticle.Factory, 0.03f);
        }

        public override string TileDesc()
        {
            return Messages.Get(this, "desc");
        }
    }
}