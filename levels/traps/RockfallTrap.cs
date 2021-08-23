using System;
using System.Collections.Generic;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;
using spdd.levels.rooms;
using spdd.messages;
using spdd.utils;

namespace spdd.levels.traps
{
    public class RockfallTrap : Trap
    {
        public RockfallTrap()
        {
            color = GREY;
            shape = DIAMOND;

            canBeHidden = false;
        }

        public override void Activate()
        {
            List<int> rockCells = new List<int>();

            //determines if the trap is actually in the world, or if it is being spawned for its effect
            bool onGround = Dungeon.level.traps[pos] == this;

            if (onGround && Dungeon.level is RegularLevel)
            {
                Room r = ((RegularLevel)Dungeon.level).Room(pos);
                int cell;
                foreach (Point p in r.GetPoints())
                {
                    cell = Dungeon.level.PointToCell(p);
                    if (!Dungeon.level.solid[cell])
                        rockCells.Add(cell);
                }
            }
            else
            {
                //if we don't have rooms, then just do 5x5
                PathFinder.BuildDistanceMap(pos, BArray.Not(Dungeon.level.solid, null), 2);
                for (int i = 0; i < PathFinder.distance.Length; ++i)
                {
                    if (PathFinder.distance[i] < int.MaxValue)
                        rockCells.Add(i);
                }
            }

            bool seen = false;
            foreach (int cell in rockCells)
            {
                if (Dungeon.level.heroFOV[cell])
                {
                    CellEmitter.Get(cell - Dungeon.level.Width()).Start(Speck.Factory(Speck.ROCK), 0.07f, 10);
                    seen = true;
                }

                Character ch = Actor.FindChar(cell);

                if (ch != null && ch.IsAlive())
                {
                    int damage = Rnd.NormalIntRange(5 + Dungeon.depth, 10 + Dungeon.depth * 2);
                    damage -= ch.DrRoll();
                    ch.Damage(Math.Max(damage, 0), this);

                    Buff.Prolong<Paralysis>(ch, Paralysis.DURATION);

                    if (!ch.IsAlive() && ch == Dungeon.hero)
                    {
                        Dungeon.Fail(GetType());
                        GLog.Negative(Messages.Get(this, "ondeath"));
                    }
                }
            }

            if (seen)
            {
                Camera.main.Shake(3, 0.7f);
                Sample.Instance.Play(Assets.Sounds.ROCKS);
            }
        }
    }
}