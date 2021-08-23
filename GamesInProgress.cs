using System;
using System.Collections.Generic;
using System.IO;
using watabou.utils;
using spdd.actors.hero;
using spdd.messages;

namespace spdd
{
    public class GamesInProgress
    {
        public const int MAX_SLOTS = 4;

        //null means we have loaded info and it is empty, no entry means unknown.
        private static Dictionary<int, Info> slotStates = new Dictionary<int, Info>();
        public static int curSlot;

        public static HeroClass? selectedClass;

        private const string GAME_FOLDER = "game%d";
        private const string GAME_FILE = "game.dat";
        private const string DEPTH_FILE = "depth%d.dat";

        public static bool GameExists(int slot)
        {
            //return FileUtils.DirExists(Messages.Format(GAME_FOLDER, slot));
            return FileUtils.DirExists(GameFolder(slot));
        }

        public static string GameFolder(int slot)
        {
            return Messages.Format(GAME_FOLDER, slot);
        }

        public static string GameFile(int slot)
        {
            return GameFolder(slot) + "/" + GAME_FILE;
        }

        public static string DepthFile(int slot, int depth)
        {
            return GameFolder(slot) + "/" + Messages.Format(DEPTH_FILE, depth);
        }

        public static int FirstEmpty()
        {
            for (int i = 1; i <= MAX_SLOTS; ++i)
            {
                if (Check(i) == null)
                    return i;
            }
            return -1;
        }

        public static List<Info> CheckAll()
        {
            List<Info> result = new List<Info>();
            for (int i = 0; i <= MAX_SLOTS; ++i)
            {
                Info curr = Check(i);
                if (curr != null)
                    result.Add(curr);
            }
            result.Sort(scoreComparator);
            return result;
        }

        public static Info Check(int slot)
        {
            if (slotStates.ContainsKey(slot))
            {
                return slotStates[slot];
            }
            else if (!GameExists(slot))
            {
                slotStates[slot] = null;
                return null;
            }
            else
            {

                Info info;
                try
                {
                    var bundle = FileUtils.BundleFromFile(GameFile(slot));
                    info = new Info();
                    info.slot = slot;
                    Dungeon.Preview(info, bundle);

                    ////saves from before v0.7.3b are not supported
                    //if (info.version < ShatteredPixelDungeon.v0_7_3b)
                    //{
                    //    info = null;
                    //}
                }
                catch (IOException)
                {
                    info = null;
                }
                catch (Exception e)
                {
                    ShatteredPixelDungeonDash.ReportException(e);
                    info = null;
                }

                slotStates[slot] = info;
                return info;
            }
        }

        public static void Set(int slot, int depth, int challenges, Hero hero)
        {
            Info info = new Info();
            info.slot = slot;

            info.depth = depth;
            info.challenges = challenges;

            info.level = hero.lvl;
            info.str = hero.STR;
            info.exp = hero.exp;
            info.hp = hero.HP;
            info.ht = hero.HT;
            info.shld = hero.Shielding();
            info.heroClass = hero.heroClass;
            info.subClass = hero.subClass;
            info.armorTier = hero.Tier();

            info.goldCollected = Statistics.goldCollected;
            info.maxDepth = Statistics.deepestFloor;

            slotStates[slot] = info;
        }

        public static void SetUnknown(int slot)
        {
            slotStates.Remove(slot);
        }

        public static void Delete(int slot)
        {
            slotStates[slot] = null;
        }

        public class Info
        {
            public int slot;

            public int depth;
            public int version;
            public int challenges;

            public int level;
            public int str;
            public int exp;
            public int hp;
            public int ht;
            public int shld;
            public HeroClass heroClass;
            public HeroSubClass subClass;
            public int armorTier;

            public int goldCollected;
            public int maxDepth;
        }

        public static ScoreComparator scoreComparator = new ScoreComparator();

        public class ScoreComparator : IComparer<GamesInProgress.Info>
        {
            public int Compare(GamesInProgress.Info lhs, GamesInProgress.Info rhs)
            {
                int lScore = (lhs.level * lhs.maxDepth * 100) + lhs.goldCollected;
                int rScore = (rhs.level * rhs.maxDepth * 100) + rhs.goldCollected;

                return Math.Sign(rScore - lScore);
            }
        }
    }
}