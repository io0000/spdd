using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.actors.mobs.npcs;
using spdd.items;
using spdd.items.scrolls;
using spdd.scenes;

namespace spdd.levels.traps
{
    public class DistortionTrap : Trap
    {
        private const float DELAY = 2f;

        public DistortionTrap()
        {
            color = TEAL;
            shape = LARGE_DOT;
        }

        private static readonly List<Type> RARE = new List<Type> {
            typeof(Albino), 
            typeof(CausticSlime),
            typeof(Bandit),
            typeof(ArmoredBrute), 
            typeof(DM201),
            typeof(Elemental.ChaosElemental), 
            typeof(Senior),
            typeof(Acidic)
        };

        public override void Activate()
        {
            int nMobs = 3;
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
                {
                    candidates.Add(p);
                }
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

            int summoned = 0;
            foreach (int point in respawnPoints)
            {
                ++summoned;
                Mob mob;

                switch (summoned)
                {
                    case 1:
                        if (Dungeon.depth != 5 && Rnd.Int(100) == 0)
                        {
                            mob = new RatKing();
                            break;
                        }
                        goto case 3;
                    case 3:
                    case 5:
                    default:
                        int floor;
                        do
                        {
                            floor = Rnd.Int(25);
                        }
                        while (Dungeon.BossLevel(floor));

                        mob = (Mob)Reflection.NewInstance(Bestiary.GetMobRotation(floor)[0]);
                        break;
                    case 2:
                        switch (Rnd.Int(4))
                        {
                            case 0:
                            default:
                                Wraith.SpawnAt(point);
                                continue; //wraiths spawn themselves, no need to do more

                            case 1:
                                //yes it's intended that these are likely to die right away
                                mob = new Piranha();
                                break;
                            case 2:
                                mob = Mimic.SpawnAt(point, new List<Item>());
                                ((Mimic)mob).StopHiding();
                                mob.alignment = Character.Alignment.ENEMY;
                                break;
                            case 3:
                                mob = Statue.Random();
                                break;
                        }
                        break;
                    case 4:
                        mob = (Mob)Reflection.NewInstance(Rnd.Element(RARE));
                        break;
                }

                if (Character.HasProp(mob, Character.Property.LARGE) && !Dungeon.level.openSpace[point])
                    continue;

                mob.maxLvl = Hero.MAX_LEVEL;
                mob.state = mob.WANDERING;
                mob.pos = point;
                GameScene.Add(mob, DELAY);
                mobs.Add(mob);
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
