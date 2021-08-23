using spdd.effects;
using spdd.messages;

namespace spdd.actors.blobs
{
    public class StormCloud : Blob
    {
        protected override void Evolve()
        {
            base.Evolve();

            int cell;

            for (int i = area.left; i < area.right; ++i)
            {
                for (int j = area.top; j < area.bottom; ++j)
                {
                    cell = i + j * Dungeon.level.Width();

                    if (cur[cell] > 0)
                    {
                        Dungeon.level.SetCellToWater(true, cell);
                    }
                }
            }
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);

            emitter.Pour(Speck.Factory(Speck.STORM), 0.4f);
        }

        public override string TileDesc()
        {
            return Messages.Get(this, "desc");
        }
    }
}