using System.Collections.Generic;
using watabou.utils;
using spdd.scenes;
using spdd.items.scrolls;
using spdd.actors;
using spdd.actors.mobs;

namespace spdd.levels.traps
{
    public class SummoningTrap : Trap
    {
        private const float DELAY = 2f;

        public SummoningTrap()
        {
            color = TEAL;
            shape = WAVES;
        }

        public override void Activate()
        {
            int nMobs = 1;
            if (Rnd.Int(2) == 0)
            {
                ++nMobs;
                if (Rnd.Int(2) == 0)
                    ++nMobs;
            }

            List<int> candidates = new List<int>();

            for (int i = 0; i < PathFinder.NEIGHBORS8.Length; ++i)
            {
                int p = pos + PathFinder.NEIGHBORS8[i];
                if (Actor.FindChar(p) == null && (Dungeon.level.passable[p] || Dungeon.level.avoid[p]))
                    candidates.Add(p);
            }

            List<int> respawnPoints = new List<int>();

            while (nMobs > 0 && candidates.Count > 0)
            {
                int index = Rnd.Index(candidates);

                var toRemove = candidates[index];
                candidates.RemoveAt(index);
                respawnPoints.Add(toRemove);
                --nMobs;
            }

            List<Mob> mobs = new List<Mob>();

            foreach (int point in respawnPoints)
            {
                Mob mob = Dungeon.level.CreateMob();
                while (Character.HasProp(mob, Character.Property.LARGE) && !Dungeon.level.openSpace[point])
                    mob = Dungeon.level.CreateMob();

                if (mob != null)
                {
                    mob.state = mob.WANDERING;
                    mob.pos = point;
                    GameScene.Add(mob, DELAY);
                    mobs.Add(mob);
                }
            }

            //important to process the visuals and pressing of cells last, so spawned mobs have a chance to occupy cells first
            Trap t;
            foreach (Mob mob in mobs)
            {
                //manually trigger traps first to avoid sfx spam
                if ((t = Dungeon.level.traps[mob.pos]) != null && t.active)
                {
                    t.Disarm();
                    t.Reveal();
                    t.Activate();
                }
                ScrollOfTeleportation.Appear(mob, mob.pos);
                Dungeon.level.OccupyCell(mob);
            }
        }
    }
}