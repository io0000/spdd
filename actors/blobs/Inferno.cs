using spdd.effects;
using spdd.scenes;
using spdd.messages;

namespace spdd.actors.blobs
{
    public class Inferno : Blob
    {
        protected override void Evolve()
        {
            base.Evolve();

            int cell;
            bool observe = false;

            Fire fire = (Fire)Dungeon.level.GetBlob(typeof(Fire));
            Freezing freeze = (Freezing)Dungeon.level.GetBlob(typeof(Freezing));
            Blizzard bliz = (Blizzard)Dungeon.level.GetBlob(typeof(Blizzard));

            int levelWidth = Dungeon.level.Width();

            for (int i = area.left - 1; i <= area.right; ++i)
            {
                for (int j = area.top - 1; j <= area.bottom; ++j)
                {
                    cell = i + j * Dungeon.level.Width();

                    if (cur[cell] > 0)
                    {
                        if (fire != null)
                            fire.Clear(cell);
                        if (freeze != null)
                            freeze.Clear(cell);

                        if (bliz != null && bliz.volume > 0 && bliz.cur[cell] > 0)
                        {
                            bliz.Clear(cell);
                            off[cell] = cur[cell] = 0;
                            continue;
                        }

                        Fire.Burn(cell);
                    }
                    else if (Dungeon.level.flamable[cell] &&
                        (cur[cell - 1] > 0 || 
                        cur[cell + 1] > 0 || 
                        cur[cell - levelWidth] > 0 || 
                        cur[cell + levelWidth] > 0))
                    {
                        Fire.Burn(cell);
                        Dungeon.level.Destroy(cell);

                        observe = true;
                        GameScene.UpdateMap(cell);
                    }
                }
            }

            if (observe)
                Dungeon.Observe();
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);
            emitter.Pour(Speck.Factory(Speck.INFERNO, true), 0.4f);
        }

        public override string TileDesc()
        {
            return Messages.Get(this, "desc");
        }
    }
}