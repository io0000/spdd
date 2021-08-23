using watabou.utils;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.items.food;
using spdd.sprites;

namespace spdd.actors.mobs
{
    public class Piranha : Mob
    {
        public Piranha()
        {
            InitInstance();
                
            spriteClass = typeof(PiranhaSprite);

            baseSpeed = 2f;

            EXP = 0;

            loot = typeof(MysteryMeat);
            lootChance = 1f;

            SLEEPING = new PiranhaSleeping(this);
            WANDERING = new PiranhaWandering(this);
            HUNTING = new PiranhaHunting(this);

            state = SLEEPING;

            properties.Add(Property.BLOB_IMMUNE);

            HP = HT = 10 + Dungeon.depth * 5;
            defenseSkill = 10 + Dungeon.depth * 2;
        }

        public override bool Act()
        {
            if (!Dungeon.level.water[pos])
            {
                Die(null);
                return true;
            }

            return base.Act();
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(Dungeon.depth, 4 + Dungeon.depth * 2);
        }

        public override int AttackSkill(Character target)
        {
            return 20 + Dungeon.depth * 2;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, Dungeon.depth);
        }

        public override bool SurprisedBy(Character enemy)
        {
            if (enemy == Dungeon.hero && ((Hero)enemy).CanSurpriseAttack())
            {
                if (fieldOfView == null || fieldOfView.Length != Dungeon.level.Length())
                {
                    fieldOfView = new bool[Dungeon.level.Length()];
                    Dungeon.level.UpdateFieldOfView(this, fieldOfView);
                }
                return state == SLEEPING || !fieldOfView[enemy.pos] || enemy.invisible > 0;
            }

            return base.SurprisedBy(enemy);
        }

        public override void Die(object cause)
        {
            base.Die(cause);

            ++Statistics.piranhasKilled;
            BadgesExtensions.ValidatePiranhasKilled();
        }

        public override float SpawningWeight()
        {
            return 0;
        }

        public override bool Reset()
        {
            return true;
        }

        public override bool GetCloser(int target)
        {
            if (rooted)
                return false;

            var step = Dungeon.FindStep(this, target, Dungeon.level.water, fieldOfView, true);
            if (step != -1)
            {
                Move(step);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool GetFurther(int target)
        {
            var step = Dungeon.Flee(this, target, Dungeon.level.water, fieldOfView, true);
            if (step != -1)
            {
                Move(step);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void InitInstance()
        {
            immunities.Add(typeof(Burning));
        }

        //if there is not a path to the enemy, piranhas act as if they can't see them
        private class PiranhaSleeping : Mob.Sleeping
        {
            public PiranhaSleeping(Mob mob)
                : base(mob)
            { }

            public override bool Act(bool enemyInFOV, bool justAlerted)
            {
                Piranha piranha = (Piranha)mob;
                if (enemyInFOV)
                {
                    PathFinder.BuildDistanceMap(piranha.enemy.pos, Dungeon.level.water, piranha.viewDistance);
                    enemyInFOV = PathFinder.distance[piranha.pos] != int.MaxValue;
                }

                return base.Act(enemyInFOV, justAlerted);
            }
        }

        private class PiranhaWandering : Mob.Wandering
        {
            public PiranhaWandering(Mob mob)
                : base(mob)
            { }

            public override bool Act(bool enemyInFOV, bool justAlerted)
            {
                Piranha piranha = (Piranha)mob;
                if (enemyInFOV)
                {
                    PathFinder.BuildDistanceMap(piranha.enemy.pos, Dungeon.level.water, piranha.viewDistance);
                    enemyInFOV = PathFinder.distance[piranha.pos] != int.MaxValue;
                }

                return base.Act(enemyInFOV, justAlerted);
            }
        }

        private class PiranhaHunting : Mob.Hunting
        {
            public PiranhaHunting(Mob mob)
                : base(mob)
            { }

            public override bool Act(bool enemyInFOV, bool justAlerted)
            {
                Piranha piranha = (Piranha)mob;
                if (enemyInFOV)
                {
                    PathFinder.BuildDistanceMap(piranha.enemy.pos, Dungeon.level.water, piranha.viewDistance);
                    enemyInFOV = PathFinder.distance[piranha.pos] != int.MaxValue;
                }

                return base.Act(enemyInFOV, justAlerted);
            }
        }
}
}