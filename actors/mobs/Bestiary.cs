using System;
using System.Collections.Generic;
using watabou.utils;

namespace spdd.actors.mobs
{
    public class Bestiary
    {
        public static List<Type> GetMobRotation(int depth)
        {
            List<Type> mobs = StandardMobRotation(depth);
            AddRareMobs(depth, mobs);
            SwapMobAlts(mobs);
            Rnd.Shuffle(mobs);
            return mobs;
        }

        //returns a rotation of standard mobs, unshuffled.
        private static List<Type> StandardMobRotation(int depth)
        {
            switch (depth)
            {
                // Sewers
                case 1:
                default:
                    //3x rat, 1x snake
                    return new List<Type>()
                    {
                        //typeof(Eye)
                        typeof(Rat),
                        typeof(Rat),
                        typeof(Rat),
                        typeof(Snake)
                    };
                case 2:
                    //2x rat, 1x snake, 2x gnoll
                    return new List<Type>()
                    {
                        typeof(Rat),
                        typeof(Rat),
                        typeof(Snake),
                        typeof(Gnoll),
                        typeof(Gnoll)
                    };
                case 3:
                    //1x rat, 1x snake, 3x gnoll, 1x swarm, 1x crab
                    return new List<Type>()
                    {
                        typeof(Rat),
                        typeof(Snake),
                        typeof(Gnoll),
                        typeof(Gnoll),
                        typeof(Gnoll),
                        typeof(Swarm),
                        typeof(Crab)
                    };

                case 4:
                case 5:
                    //1x gnoll, 1x swarm, 2x crab, 2x slime
                    return new List<Type>()
                    {
                        typeof(Gnoll),
                        typeof(Swarm),
                        typeof(Crab),
                        typeof(Crab),
                        typeof(Slime),
                        typeof(Slime)
                    };

                // Prison
                case 6:
                    //3x skeleton, 1x thief, 1x swarm
                    return new List<Type>()
                    {
                        typeof(Skeleton),
                        typeof(Skeleton),
                        typeof(Skeleton),
                        typeof(Thief),
                        typeof(Swarm)
                    };
                case 7:
                    //3x skeleton, 1x thief, 1x DM-100, 1x guard
                    return new List<Type>()
                    {
                        typeof(Skeleton),
                        typeof(Skeleton),
                        typeof(Skeleton),
                        typeof(Thief),
                        typeof(DM100),
                        typeof(Guard)
                    };

                case 8:
                    //2x skeleton, 1x thief, 2x DM-100, 2x guard, 1x necromancer
                    return new List<Type>()
                    {
                        typeof(Skeleton),
                        typeof(Skeleton),
                        typeof(Thief),
                        typeof(DM100),
                        typeof(DM100),
                        typeof(Guard),
                        typeof(Guard),
                        typeof(Necromancer)
                    };

                case 9:
                case 10:
                    //1x skeleton, 1x thief, 2x DM-100, 2x guard, 2x necromancer
                    return new List<Type>()
                    {
                        typeof(Skeleton),
                        typeof(Thief),
                        typeof(DM100),
                        typeof(DM100),
                        typeof(Guard),
                        typeof(Guard),
                        typeof(Necromancer),
                        typeof(Necromancer)
                    };

                // Caves
                case 11:
                    //3x bat, 1x brute, 1x shaman
                    return new List<Type>()
                    {
                        typeof(Bat),
                        typeof(Bat),
                        typeof(Bat),
                        typeof(Brute),
                        Shaman.Random()
                    };

                case 12:
                    //2x bat, 2x brute, 1x shaman, 1x spinner
                    return new List<Type>()
                    {
                        typeof(Bat),
                        typeof(Bat),
                        typeof(Brute),
                        typeof(Brute),
                        Shaman.Random(),
                        typeof(Spinner)
                    };

                case 13:
                    //1x bat, 2x brute, 2x shaman, 2x spinner, 1x DM-200
                    return new List<Type>()
                    {
                        typeof(Bat),
                        typeof(Brute),
                        typeof(Brute),
                        Shaman.Random(),
                        Shaman.Random(),
                        typeof(Spinner),
                        typeof(Spinner),
                        typeof(DM200),
                    };

                case 14:
                case 15:
                    //1x bat, 1x brute, 2x shaman, 2x spinner, 2x DM-200
                    return new List<Type>()
                    {
                        typeof(Bat),
                        typeof(Brute),
                        Shaman.Random(),
                        Shaman.Random(),
                        typeof(Spinner),
                        typeof(Spinner),
                        typeof(DM200),
                        typeof(DM200),
                    };

                // City
                case 16:
                    //2x ghoul, 2x elemental, 1x warlock
                    return new List<Type>()
                    {
                        typeof(Ghoul),
                        typeof(Ghoul),
                        Elemental.Random(),
                        Elemental.Random(),
                        typeof(Warlock)
                    };

                case 17:
                    //1x ghoul, 2x elemental, 1x warlock, 1x monk
                    return new List<Type>()
                    {
                        typeof(Ghoul),
                        Elemental.Random(),
                        Elemental.Random(),
                        typeof(Warlock),
                        typeof(Monk),
                    };

                case 18:
                    //1x ghoul, 1x elemental, 2x warlock, 2x monk, 1x golem
                    return new List<Type>()
                    {
                        typeof(Ghoul),
                        Elemental.Random(),
                        typeof(Warlock),
                        typeof(Warlock),
                        typeof(Monk),
                        typeof(Monk),
                        typeof(Golem)
                    };

                case 19:
                case 20:
                    //1x elemental, 2x warlock, 2x monk, 3x golem
                    return new List<Type>()
                    {
                        Elemental.Random(),
                        typeof(Warlock),
                        typeof(Warlock),
                        typeof(Monk),
                        typeof(Monk),
                        typeof(Golem),
                        typeof(Golem),
                        typeof(Golem)
                    };

                // Halls
                case 21:
                    //2x succubus, 1x evil eye
                    return new List<Type>()
                    {
                        typeof(Succubus),
                        typeof(Succubus),
                        typeof(Eye)
                    };

                case 22:
                    //1x succubus, 1x evil eye
                    return new List<Type>()
                    {
                        typeof(Succubus),
                        typeof(Eye)
                    };

                case 23:
                    //1x succubus, 2x evil eye, 1x scorpio
                    return new List<Type>()
                    {
                        typeof(Succubus),
                        typeof(Eye),
                        typeof(Eye),
                        typeof(Scorpio)
                    };

                case 24:
                case 25:
                case 26:
                    //1x succubus, 2x evil eye, 3x scorpio
                    return new List<Type>()
                    {
                        typeof(Succubus),
                        typeof(Eye),
                        typeof(Eye),
                        typeof(Scorpio),
                        typeof(Scorpio),
                        typeof(Scorpio)
                    };
            }
        }

        public static void AddRareMobs(int depth, List<Type> rotation)
        {
            switch (depth)
            {
                // Sewers
                default:
                    return;
                case 4:
                    if (Rnd.Float() < 0.025f)
                        rotation.Add(typeof(Thief));
                    return;

                // Prison
                case 9:
                    if (Rnd.Float() < 0.025f)
                        rotation.Add(typeof(Bat));
                    return;

                // Caves
                case 14:
                    if (Rnd.Float() < 0.025f)
                        rotation.Add(typeof(Ghoul));
                    return;

                // City
                case 19:
                    if (Rnd.Float() < 0.025f)
                        rotation.Add(typeof(Succubus));
                    return;
            }
        }

        //switches out regular mobs for their alt versions when appropriate
        private static void SwapMobAlts(List<Type> rotation)
        {
            for (int i = 0; i < rotation.Count; ++i)
            {
                if (Rnd.Int(50) == 0)
                {
                    Type cl = rotation[i];
                    if (cl.Equals(typeof(Rat)))
                    {
                        cl = typeof(Albino);
                    }
                    else if (cl.Equals(typeof(Slime)))
                    {
                        cl = typeof(CausticSlime);
                    }
                    else if (cl.Equals(typeof(Thief)))
                    {
                        cl = typeof(Bandit);
                    }
                    else if (cl.Equals(typeof(Brute)))
                    {
                        cl = typeof(ArmoredBrute);
                    }
                    else if (cl.Equals(typeof(DM200)))
                    {
                        cl = typeof(DM201);
                    }
                    else if (cl.Equals(typeof(Monk)))
                    {
                        cl = typeof(Senior);
                    }
                    else if (cl.Equals(typeof(Scorpio)))
                    {
                        cl = typeof(Acidic);
                    }
                    rotation[i] = cl;
                }
            }
        }
    }
}