using spdd.actors.buffs;
using spdd.effects;
using spdd.messages;

namespace spdd.actors.blobs
{
    public class StenchGas : Blob
    {
        protected override void Evolve()
        {
            base.Evolve();

            Character ch;
            int cell;

            for (int i = area.left; i < area.right; ++i)
            {
                for (int j = area.top; j < area.bottom; ++j)
                {
                    cell = i + j * Dungeon.level.Width();

                    if (cur[cell] > 0 && (ch = Actor.FindChar(cell)) != null)
                    {
                        if (!ch.IsImmune(GetType()))
                            Buff.Prolong<Paralysis>(ch, Paralysis.DURATION / 5);
                    }
                }
            }
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);

            emitter.Pour(Speck.Factory(Speck.STENCH), 0.4f);
        }

        public override string TileDesc()
        {
            return Messages.Get(this, "desc");
        }
    }
}