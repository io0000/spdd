using System;
using watabou.utils;
using watabou.noosa.audio;
using spdd.scenes;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.mobs;

namespace spdd.levels.traps
{
    public class FlashingTrap : Trap
    {
        public FlashingTrap()
        {
            color = GREY;
            shape = STARS;
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
            if (c != null)
            {
                int damage = Math.Max(0, (4 + Dungeon.depth / 2) - c.DrRoll() / 2);
                Buff.Affect<Bleeding>(c).Set(damage);
                Buff.Prolong<Blindness>(c, Blindness.DURATION);
                Buff.Prolong<Cripple>(c, Cripple.DURATION * 2f);

                if (c is Mob)
                {
                    if (((Mob)c).state == ((Mob)c).HUNTING)
                        ((Mob)c).state = ((Mob)c).WANDERING;

                    ((Mob)c).Beckon(Dungeon.level.RandomDestination(c));
                }
            }

            if (Dungeon.level.heroFOV[pos])
            {
                GameScene.Flash(new Color(0xFF, 0xFF, 0xFF, 0xFF));
                Sample.Instance.Play(Assets.Sounds.BLAST);
            }
        }
    }
}