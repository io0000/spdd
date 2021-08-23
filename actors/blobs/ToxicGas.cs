using spdd.actors.hero;
using spdd.effects;
using spdd.utils;
using spdd.messages;

namespace spdd.actors.blobs
{
    public class ToxicGas : Blob, Hero.IDoom
    {
        protected override void Evolve()
        {
            base.Evolve();

            int damage = 1 + Dungeon.depth / 5;

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
                            ch.Damage(damage, this);
                    }
                }
            }
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);

            emitter.Pour(Speck.Factory(Speck.TOXIC), 0.4f);
        }

        public override string TileDesc()
        {
            return Messages.Get(this, "desc");
        }

        // Hero.IDoom
        public void OnDeath()
        {
            BadgesExtensions.ValidateDeathFromGas();

            Dungeon.Fail(GetType());
            GLog.Negative(Messages.Get(this, "ondeath"));
        }
    }
}