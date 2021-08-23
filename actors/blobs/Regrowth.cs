using spdd.actors.buffs;
using spdd.effects;
using spdd.effects.particles;
using spdd.levels;
using spdd.scenes;

namespace spdd.actors.blobs
{
    public class Regrowth : Blob
    {
        protected override void Evolve()
        {
            base.Evolve();

            if (volume <= 0)
                return;

            int cell;

            for (int i = area.left; i < area.right; ++i)
            {
                for (int j = area.top; j < area.bottom; ++j)
                {
                    cell = i + j * Dungeon.level.Width();

                    if (off[cell] > 0)
                    {
                        int c = Dungeon.level.map[cell];
                        int c1 = c;
                        if (c == Terrain.EMPTY || c == Terrain.EMBERS || c == Terrain.EMPTY_DECO)
                        {
                            c1 = (cur[cell] > 9 && Actor.FindChar(cell) == null) ? 
                                Terrain.HIGH_GRASS : Terrain.GRASS;
                        }
                        else if ((c == Terrain.GRASS || c == Terrain.FURROWED_GRASS) && 
                            cur[cell] > 9 &&
                            Dungeon.level.plants[cell] == null &&
                            Actor.FindChar(cell) == null)
                        {
                            c1 = Terrain.HIGH_GRASS;
                        }

                        if (c1 != c)
                        {
                            Level.Set(cell, c1);
                            GameScene.UpdateMap(cell);
                        }

                        var ch = Actor.FindChar(cell);
                        if (ch != null &&
                            !ch.IsImmune(GetType()) &&
                            off[cell] > 1)
                        {
                            Buff.Prolong<Roots>(ch, TICK);
                        }
                    }
                }
            }

            Dungeon.Observe();
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);

            emitter.Start(LeafParticle.LevelSpecific, 0.2f, 0);
        }
    }
}