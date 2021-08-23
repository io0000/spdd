using spdd.effects;
using spdd.levels;
using spdd.messages;

namespace spdd.actors.blobs
{
    public class SmokeScreen : Blob
    {
        protected override void Evolve()
        {
            base.Evolve();

            int cell;
            Level l = Dungeon.level;

            for (int i = area.left; i < area.right; ++i)
            {
                for (int j = area.top; j < area.bottom; ++j)
                {
                    cell = i + j * l.Width();

                    l.losBlocking[cell] = off[cell] > 0 || (Terrain.flags[l.map[cell]] & Terrain.LOS_BLOCKING) != 0;
                }
            }
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);

            emitter.Pour(Speck.Factory(Speck.SMOKE), 0.1f);
        }

        public override void Clear(int cell)
        {
            base.Clear(cell);
            
            Level l = Dungeon.level;
            l.losBlocking[cell] = cur[cell] > 0 || (Terrain.flags[l.map[cell]] & Terrain.LOS_BLOCKING) != 0;
        }

        public override void FullyClear()
        {
            base.FullyClear();
            Dungeon.level.BuildFlagMaps();
        }

        public override string TileDesc()
        {
            return Messages.Get(this, "desc");
        }
    }
}