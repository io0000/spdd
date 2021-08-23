using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.items;
using spdd.items.bags;
using spdd.items.potions;
using spdd.items.rings;
using spdd.items.scrolls;
using spdd.journal;
using spdd.messages;
using spdd.ui;

namespace spdd
{
    public class Rankings
    {
        private static Rankings instance;
        public static Rankings Instance
        {
            get
            {
                if (instance == null)
                    instance = new Rankings();

                return instance;
            }
        }

        public const int TABLE_SIZE = 11;

        public const string RANKINGS_FILE = "rankings.dat";

        public List<Record> records;
        public int lastRecord;
        public int totalNumber;
        public int wonNumber;

        public void Submit(bool win, Type cause)
        {
            Load();

            var rec = new Record();

            rec.cause = cause;
            rec.win = win;
            rec.heroClass = Dungeon.hero.heroClass;
            rec.armorTier = Dungeon.hero.Tier();
            rec.herolevel = Dungeon.hero.lvl;
            rec.depth = Dungeon.depth;
            rec.score = Score(win);

            Instance.SaveGameData(rec);

            //rec.gameID = UUID.randomUUID().toString();
            rec.gameID = System.Guid.NewGuid().ToString();

            records.Add(rec);

            records.Sort(scoreComparator);

            lastRecord = records.IndexOf(rec);
            var size = records.Count;
            while (size > TABLE_SIZE)
            {
                if (lastRecord == size - 1)
                {
                    records.Remove(records[size - 2]);
                    --lastRecord;
                }
                else
                {
                    records.Remove(records[size - 1]);
                }

                size = records.Count;
            }

            ++totalNumber;
            if (win)
                ++wonNumber;

            BadgesExtensions.ValidateGamesPlayed();

            Save();
        }

        private static int Score(bool win)
        {
            return (Statistics.goldCollected + Dungeon.hero.lvl * (win ? 26 : Dungeon.depth) * 100) * (win ? 2 : 1);
        }

        public const string HERO = "hero";
        public const string STATS = "stats";
        public const string BADGES = "badges";
        public const string HANDLERS = "handlers";
        public const string CHALLENGES = "challenges";

        public void SaveGameData(Record rec)
        {
            rec.gameData = new Bundle();

            Belongings belongings = Dungeon.hero.belongings;

            //save the hero and belongings
            //List<Item> allItems = (List<Item>) belongings.backpack.items.clone();
            List<Item> allItems = new List<Item>(belongings.backpack.items);

            //remove items that won't show up in the rankings screen
            foreach (Item item in belongings.backpack.items.ToArray())
            {
                if (item is Bag)
                {
                    foreach (Item bagItem in ((Bag)item).items.ToArray())
                    {
                        if (Dungeon.quickslot.Contains(bagItem))
                            belongings.backpack.items.Add(bagItem);
                    }
                    belongings.backpack.items.Remove(item);
                }
                else if (!Dungeon.quickslot.Contains(item))
                {
                    belongings.backpack.items.Remove(item);
                }
            }

            //remove all buffs (ones tied to equipment will be re-applied)
            foreach (Buff b in Dungeon.hero.Buffs())
                Dungeon.hero.Remove(b);

            rec.gameData.Put(HERO, Dungeon.hero);

            //save stats
            Bundle stats = new Bundle();
            Statistics.StoreInBundle(stats);
            rec.gameData.Put(STATS, stats);

            //save badges
            Bundle badges = new Bundle();
            BadgesExtensions.SaveLocal(badges);
            rec.gameData.Put(BADGES, badges);

            //save handler information
            Bundle handler = new Bundle();
            Scroll.SaveSelectively(handler, belongings.backpack.items);
            Potion.SaveSelectively(handler, belongings.backpack.items);
            //include potentially worn rings
            if (belongings.misc != null)
                belongings.backpack.items.Add(belongings.misc);
            if (belongings.ring != null)
                belongings.backpack.items.Add(belongings.ring);
            Ring.SaveSelectively(handler, belongings.backpack.items);
            rec.gameData.Put(HANDLERS, handler);

            //restore items now that we're done saving
            belongings.backpack.items = allItems;

            //save challenges
            rec.gameData.Put(CHALLENGES, Dungeon.challenges);
        }

        public void LoadGameData(Record rec)
        {
            Bundle data = rec.gameData;

            Actor.Clear();
            Dungeon.hero = null;
            Dungeon.level = null;
            Generator.FullReset();
            Notes.Reset();
            Dungeon.quickslot.Reset();
            QuickSlotButton.Reset();

            Bundle handler = data.GetBundle(HANDLERS);
            Scroll.Restore(handler);
            Potion.Restore(handler);
            Ring.Restore(handler);

            BadgesExtensions.LoadLocal(data.GetBundle(BADGES));

            Dungeon.hero = (Hero)data.Get(HERO);

            Statistics.RestoreFromBundle(data.GetBundle(STATS));

            Dungeon.challenges = data.GetInt(CHALLENGES);
        }

        private const string RECORDS = "records";
        private const string LATEST = "latest";
        private const string TOTAL = "total";
        private const string WON = "won";

        public void Save()
        {
            var bundle = new Bundle();
            bundle.Put(RECORDS, records);
            bundle.Put(LATEST, lastRecord);
            bundle.Put(TOTAL, totalNumber);
            bundle.Put(WON, wonNumber);

            try
            {
                FileUtils.BundleToFile(RANKINGS_FILE, bundle);
            }
            catch (Exception e)
            {
                ShatteredPixelDungeonDash.ReportException(e);
            }
        }

        public void Load()
        {
            if (records != null)
                return;

            records = new List<Record>();

            try
            {
                Bundle bundle = FileUtils.BundleFromFile(RANKINGS_FILE);

                foreach (var record in bundle.GetCollection(RECORDS))
                    records.Add((Record)record);

                lastRecord = bundle.GetInt(LATEST);

                totalNumber = bundle.GetInt(TOTAL);
                if (totalNumber == 0)
                    totalNumber = records.Count;

                wonNumber = bundle.GetInt(WON);
                if (wonNumber == 0)
                {
                    foreach (Record rec in records)
                    {
                        if (rec.win)
                            ++wonNumber;
                    }
                }
            }
            catch (Exception)
            { }
        }

        [SPDStatic]
        public class Record : IBundlable
        {
            private const string CAUSE = "cause";
            private const string WIN = "win";
            private const string SCORE = "score";
            private const string TIER = "tier";
            private const string LEVEL = "level";
            private const string DEPTH = "depth";
            private const string DATA = "gameData";
            private const string ID = "gameID";

            public Type cause;
            public bool win;

            public HeroClass heroClass;
            public int armorTier;
            public int herolevel;
            public int depth;

            public Bundle gameData;
            public string gameID;

            public int score;

            public string Desc()
            {
                if (cause == null)
                {
                    return Messages.Get(this, "something");
                }
                else
                {
                    string result = Messages.Get(cause, "rankings_desc", (Messages.Get(cause, "name")));
                    if (result.Contains("!!!NO TEXT FOUND!!!"))
                    {
                        return Messages.Get(this, "something");
                    }
                    else
                    {
                        return result;
                    }
                }
            }

            public void RestoreFromBundle(Bundle bundle)
            {
                if (bundle.Contains(CAUSE))
                    cause = bundle.GetClass(CAUSE);
                else
                    cause = null;

                win = bundle.GetBoolean(WIN);
                score = bundle.GetInt(SCORE);

                heroClass = HeroClassExtensions.RestoreInBundle(bundle);
                armorTier = bundle.GetInt(TIER);

                if (bundle.Contains(DATA))
                    gameData = bundle.GetBundle(DATA);
                if (bundle.Contains(ID))
                    gameID = bundle.GetString(ID);

                if (gameID == null)
                {
                    //gameID = UUID.randomUUID().toString();
                    gameID = System.Guid.NewGuid().ToString();
                }

                depth = bundle.GetInt(DEPTH);
                herolevel = bundle.GetInt(LEVEL);
            }

            public void StoreInBundle(Bundle bundle)
            {
                if (cause != null)
                    bundle.Put(CAUSE, cause);

                bundle.Put(WIN, win);
                bundle.Put(SCORE, score);

                heroClass.StoreInBundle(bundle);
                bundle.Put(TIER, armorTier);
                bundle.Put(LEVEL, herolevel);
                bundle.Put(DEPTH, depth);

                if (gameData != null)
                    bundle.Put(DATA, gameData);
                bundle.Put(ID, gameID);
            }
        }

        public static ScoreComparator scoreComparator = new ScoreComparator();

        public class ScoreComparator : IComparer<Record>
        {
            public int Compare(Record lhs, Record rhs)
            {
                int result = Math.Sign(rhs.score - lhs.score);
                if (result == 0)
                {
                    return Math.Sign(rhs.gameID.GetHashCode() - lhs.gameID.GetHashCode());
                }
                else
                {
                    return result;
                }
            }
        }
    }
}