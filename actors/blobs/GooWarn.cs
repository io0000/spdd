using spdd.effects;
using spdd.sprites;
using spdd.messages;

namespace spdd.actors.blobs
{
    public class GooWarn : Blob
    {
        //cosmetic blob, used to warn noobs that goo's pump up should, infact, be avoided.
        public GooWarn()
        {
            //this one needs to act just before the Goo
            actPriority = MOB_PRIO + 1;
        }

        // protected int pos;

        protected override void Evolve()
        {
            int cell;

            for (int i = area.left; i < area.right; ++i)
            {
                for (int j = area.top; j < area.bottom; ++j)
                {
                    cell = i + j * Dungeon.level.Width();

                    off[cell] = cur[cell] > 0 ? cur[cell] - 1 : 0;

                    if (off[cell] > 0)
                        volume += off[cell];
                }
            }
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);
            emitter.Pour(GooSprite.GooParticle.Factory, 0.03f);
        }

        public override string TileDesc()
        {
            return Messages.Get(this, "desc");
        }
    }
}