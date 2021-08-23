using System;
using spdd.effects;
using spdd.scenes;
using spdd.journal;

namespace spdd.actors.blobs
{
    public class Alchemy : Blob, AlchemyScene.IAlchemyProvider
    {
        protected int pos;

        protected override void Evolve()
        {
            int cell;
            for (int i = area.top - 1; i <= area.bottom; ++i)
            {
                for (int j = area.left - 1; j <= area.right; ++j)
                {
                    cell = j + i * Dungeon.level.Width();
                    if (Dungeon.level.InsideMap(cell))
                    {
                        off[cell] = cur[cell];
                        volume += off[cell];
                        if (off[cell] > 0 && Dungeon.level.heroFOV[cell])
                        {
                            Notes.Add(Notes.Landmark.ALCHEMY);
                        }
                    }
                }
            }
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);
            base.emitter.Start(Speck.Factory(Speck.BUBBLE), 0.33f, 0);
        }

        public static int alchPos;

        //1 volume is kept in reserve

        public int GetEnergy()
        {
            return Math.Max(0, cur[alchPos] - 1);
        }

        public void SpendEnergy(int reduction)
        {
            cur[alchPos] = Math.Max(1, cur[alchPos] - reduction);
        }
    }
}