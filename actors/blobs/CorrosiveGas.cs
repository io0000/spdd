using watabou.utils;
using spdd.actors.buffs;
using spdd.effects;
using spdd.messages;

namespace spdd.actors.blobs
{
    public class CorrosiveGas : Blob
    {
        //FIXME should have strength per-cell
        private int strength;

        protected override void Evolve()
        {
            base.Evolve();

            if (volume == 0)
            {
                strength = 0;
            }
            else
            {
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
                                Buff.Affect<Corrosion>(ch).Set(2.0f, strength);
                        }
                    }
                }
            }
        }

        public CorrosiveGas SetStrength(int value)
        {
            if (value > strength)
            {
                strength = value;
            }
            return this;
        }

        private const string STRENGTH = "strength";

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            strength = bundle.GetInt(STRENGTH);
        }

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(STRENGTH, strength);
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);

            emitter.Pour(Speck.Factory(Speck.CONFUSION), 0.4f);
        }

        public override string TileDesc()
        {
            return Messages.Get(this, "desc");
        }
    }
}