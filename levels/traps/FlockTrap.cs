using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.mobs.npcs;
using spdd.effects;
using spdd.scenes;
using spdd.utils;

namespace spdd.levels.traps
{
    public class FlockTrap : Trap
    {
        public FlockTrap()
        {
            color = WHITE;
            shape = WAVES;
        }

        public override void Activate()
        {
            //use an actor as we want to put this on a slight delay so all chars get a chance to act this turn first.
            Actor.Add(new FlockTrapActor(this));
        }

        public class FlockTrapActor : Actor
        {
            FlockTrap trap;
            public FlockTrapActor(FlockTrap trap)
            {
                actPriority = BUFF_PRIO;
                this.trap = trap;
            }

            public override bool Act()
            {
                return trap.Act(this);
            }
        }

        public bool Act(FlockTrapActor actor)
        {
            PathFinder.BuildDistanceMap(pos, BArray.Not(Dungeon.level.solid, null), 2);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                Trap t;
                if (PathFinder.distance[i] < int.MaxValue)
                {
                    if (Dungeon.level.InsideMap(i) &&
                        Actor.FindChar(i) == null &&
                        !Dungeon.level.pit[i])
                    {
                        Sheep sheep = new Sheep();
                        sheep.lifespan = Rnd.NormalIntRange(4, 8);
                        sheep.pos = i;
                        GameScene.Add(sheep);
                        CellEmitter.Get(i).Burst(Speck.Factory(Speck.WOOL), 4);
                        //before the tile is pressed, directly trigger traps to avoid sfx spam
                        if ((t = Dungeon.level.traps[i]) != null && t.active)
                        {
                            t.Disarm();
                            t.Reveal();
                            t.Activate();
                        }
                        Dungeon.level.OccupyCell(sheep);
                    }
                }
            }
            Sample.Instance.Play(Assets.Sounds.PUFF);
            Sample.Instance.Play(Assets.Sounds.SHEEP);
            Actor.Remove(actor);
            return true;
        }
    }
}