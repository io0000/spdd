using spdd.actors.buffs;
using spdd.effects;
using spdd.effects.particles;
using spdd.levels;
using spdd.messages;

namespace spdd.actors.blobs
{
    public class Web : Blob
    {
        public Web()
        {
            //acts before the hero, to ensure terrain is adjusted correctly
            actPriority = HERO_PRIO + 1;
        }

        protected override void Evolve()
        {
            int cell;
            Level l = Dungeon.level;

            for (int i = area.left; i < area.right; ++i)
            {
                for (int j = area.top; j < area.bottom; ++j)
                {
                    cell = i + j * l.Width();
                    off[cell] = cur[cell] > 0 ? cur[cell] - 1 : 0;

                    volume += off[cell];

                    l.solid[cell] = off[cell] > 0 || (Terrain.flags[l.map[cell]] & Terrain.SOLID) != 0;
                }
            }
        }

        public override void Seed(Level level, int cell, int amount)
        {
            base.Seed(level, cell, amount);

            level.solid[cell] = cur[cell] > 0 || (Terrain.flags[level.map[cell]] & Terrain.SOLID) != 0;
        }

        //affects characters as they step on it. See Level.OccupyCell and Level.PressCell
        public static void AffectChar(Character ch)
        {
            Buff.Prolong<Roots>(ch, Roots.DURATION);
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);

            emitter.Pour(WebParticle.Factory, 0.25f);
        }

        public override void Clear(int cell)
        {
            base.Clear(cell);
            if (cur == null) 
                return;

            Level l = Dungeon.level;
            l.solid[cell] = cur[cell] > 0 || (Terrain.flags[l.map[cell]] & Terrain.SOLID) != 0;
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