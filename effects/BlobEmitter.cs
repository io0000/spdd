using watabou.utils;
using watabou.noosa.particles;
using spdd.actors.blobs;
using spdd.tiles;

namespace spdd.effects
{
    public class BlobEmitter : Emitter
    {
        private Blob blob;

        public BlobEmitter(Blob blob)
        {
            this.blob = blob;
            blob.Use(this);
        }

        public RectF bound = new RectF(0, 0, 1, 1);

        public override void Emit(int index)
        {
            if (blob.volume <= 0)
                return;

            if (blob.area.IsEmpty())
                blob.SetupArea();

            var map = blob.cur;
            float size = DungeonTilemap.SIZE;

            int cell;
            for (int i = blob.area.left; i < blob.area.right; ++i)
            {
                for (int j = blob.area.top; j < blob.area.bottom; ++j)
                {
                    cell = i + j * Dungeon.level.Width();

                    if (cell < Dungeon.level.heroFOV.Length &&
                        (Dungeon.level.heroFOV[cell] || blob.alwaysVisible) &&
                        map[cell] > 0)
                    {
                        float x = (i + Rnd.Float(bound.left, bound.right)) * size;
                        float y = (j + Rnd.Float(bound.top, bound.bottom)) * size;
                        factory.Emit(this, index, x, y);
                    }
                }
            }
        }
    }
}