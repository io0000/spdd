using spdd.effects;
using spdd.effects.particles;
using spdd.actors;
using spdd.actors.buffs;

namespace spdd.levels.traps
{
    public class WeakeningTrap : Trap
    {
        public WeakeningTrap()
        {
            color = GREEN;
            shape = WAVES;
        }

        public override void Activate()
        {
            if (Dungeon.level.heroFOV[pos])
                CellEmitter.Get(pos).Burst(ShadowParticle.Up, 5);

            Character ch = Actor.FindChar(pos);
            if (ch != null)
            {
                //if (ch.Properties().Contains(Character.Property.BOSS) ||
                //    ch.Properties().Contains(Character.Property.MINIBOSS))
                //{
                //    Buff.Prolong<Weakness>(ch, Weakness.DURATION / 2f);
                //}
                //Buff.Prolong<Weakness>(ch, Weakness.DURATION * 3f);

                // [FIXED]
                float duration = Weakness.DURATION * 3f;

                if (ch.Properties().Contains(Character.Property.BOSS) ||
                    ch.Properties().Contains(Character.Property.MINIBOSS))
                {
                    duration = Weakness.DURATION / 2f;
                }

                Buff.Prolong<Weakness>(ch, duration);
            }
        }
    }
}