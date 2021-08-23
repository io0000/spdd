using System;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;

namespace spdd.levels.traps
{
    public class GrippingTrap : Trap
    {
        public GrippingTrap()
        {
            color = GREY;
            shape = DOTS;
        }

        public override void Trigger()
        {
            if (Dungeon.level.heroFOV[pos])
                Sample.Instance.Play(Assets.Sounds.TRAP);

            //this trap is not disarmed by being triggered
            Reveal();
            Level.Set(pos, Terrain.TRAP);
            Activate();
        }

        public override void Activate()
        {
            var c = Actor.FindChar(pos);

            if (c != null && !c.flying)
            {
                int damage = Math.Max(0, (2 + Dungeon.depth / 2) - c.DrRoll() / 2);
                Buff.Affect<Bleeding>(c).Set(damage);
                Buff.Prolong<Cripple>(c, Cripple.DURATION);
                Wound.Hit(c);
            }
            else
            {
                Wound.Hit(pos);
            }
        }
    }
}