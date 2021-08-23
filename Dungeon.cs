using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using watabou.noosa;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.actors.mobs.npcs;
using spdd.items;
using spdd.items.artifacts;
using spdd.items.potions;
using spdd.items.rings;
using spdd.items.scrolls;
using spdd.journal;
using spdd.levels;
using spdd.levels.rooms.secret;
using spdd.levels.rooms.special;
using spdd.mechanics;
using spdd.messages;
using spdd.scenes;
using spdd.ui;
using spdd.utils;

namespace spdd
{
    public class Dungeon
    {
        //enum of items which have limited spawns, records how many have spawned
        //could all be their own separate numbers, but this allows iterating, much nicer for bundling/initializing.
        public class LimitedDrops
        {
            //limited world drops
            public static LimitedDrops STRENGTH_POTIONS = new LimitedDrops("STRENGTH_POTIONS");
            public static LimitedDrops UPGRADE_SCROLLS = new LimitedDrops("UPGRADE_SCROLLS");
            public static LimitedDrops ARCANE_STYLI = new LimitedDrops("ARCANE_STYLI");

            //Health potion sources
            //enemies
            public static LimitedDrops SWARM_HP = new LimitedDrops("SWARM_HP");
            public static LimitedDrops NECRO_HP = new LimitedDrops("NECRO_HP");
            public static LimitedDrops BAT_HP = new LimitedDrops("BAT_HP");
            public static LimitedDrops WARLOCK_HP = new LimitedDrops("WARLOCK_HP");
            //Demon spawners are already limited in their spawnrate, no need to limit their health drops
            //alchemy
            public static LimitedDrops COOKING_HP = new LimitedDrops("COOKING_HP");
            public static LimitedDrops BLANDFRUIT_SEED = new LimitedDrops("BLANDFRUIT_SEED");

            //Other limited enemy drops
            public static LimitedDrops SLIME_WEP = new LimitedDrops("SLIME_WEP");
            public static LimitedDrops SKELE_WEP = new LimitedDrops("SKELE_WEP");
            public static LimitedDrops THEIF_MISC = new LimitedDrops("THEIF_MISC");
            public static LimitedDrops GUARD_ARM = new LimitedDrops("GUARD_ARM");
            public static LimitedDrops SHAMAN_WAND = new LimitedDrops("SHAMAN_WAND");
            public static LimitedDrops DM200_EQUIP = new LimitedDrops("DM200_EQUIP");
            public static LimitedDrops GOLEM_EQUIP = new LimitedDrops("GOLEM_EQUIP");

            //containers
            public static LimitedDrops DEW_VIAL = new LimitedDrops("DEW_VIAL");
            public static LimitedDrops VELVET_POUCH = new LimitedDrops("VELVET_POUCH");
            public static LimitedDrops SCROLL_HOLDER = new LimitedDrops("SCROLL_HOLDER");
            public static LimitedDrops POTION_BANDOLIER = new LimitedDrops("POTION_BANDOLIER");
            public static LimitedDrops MAGICAL_HOLSTER = new LimitedDrops("MAGICAL_HOLSTER");

            public static IEnumerable<LimitedDrops> Values()
            {
                yield return STRENGTH_POTIONS;
                yield return UPGRADE_SCROLLS;
                yield return ARCANE_STYLI;
                yield return SWARM_HP;
                yield return NECRO_HP;
                yield return BAT_HP;
                yield return WARLOCK_HP;
                yield return COOKING_HP;
                yield return BLANDFRUIT_SEED;
                yield return SLIME_WEP;
                yield return SKELE_WEP;
                yield return THEIF_MISC;
                yield return GUARD_ARM;
                yield return SHAMAN_WAND;
                yield return DM200_EQUIP;
                yield return GOLEM_EQUIP;
                yield return DEW_VIAL;
                yield return VELVET_POUCH;
                yield return SCROLL_HOLDER;
                yield return POTION_BANDOLIER;
                yield return MAGICAL_HOLSTER;
            }

            public LimitedDrops(string name)
            {
                this.name = name;
            }

            public int count;
            private string name;

            //for items which can only be dropped once, should directly access count otherwise.
            public bool Dropped()
            {
                return count != 0;
            }

            public void Drop()
            {
                count = 1;
            }

            public static void Reset()
            {
                foreach (LimitedDrops lim in Values())
                    lim.count = 0;
            }

            public static void Store(Bundle bundle)
            {
                foreach (LimitedDrops lim in Values())
                    bundle.Put(lim.name, lim.count);
            }

            public static void Restore(Bundle bundle)
            {
                foreach (LimitedDrops lim in Values())
                {
                    if (bundle.Contains(lim.name))
                        lim.count = bundle.GetInt(lim.name);
                    else
                        lim.count = 0;
                }
            }
        }

        public static int challenges;

        public static Hero hero;
        public static Level level;

        public static QuickSlot quickslot = new QuickSlot();

        public static int depth;
        public static int gold;

        public static HashSet<int> chapters;

        public static SparseArray<List<Item>> droppedItems;
        public static SparseArray<List<Item>> portedItems;

        public static int version;

        // public static long seed;
        public static int seed;

        public static void Init()
        {
            version = Game.versionCode;
            challenges = SPDSettings.Challenges();

            seed = Rnd.Int(int.MaxValue); //DungeonSeed.RandomSeed();

            Actor.Clear();
            Actor.ResetNextID();

            Rnd.PushGenerator(seed);
            {
                Scroll.InitLabels();
                Potion.InitColors();
                Ring.InitGems();

                SpecialRoom.InitForRunSpecial();
                SecretRoom.InitForRunSecret();
            }
            Rnd.ResetGenerators();

            Statistics.Reset();
            Notes.Reset();

            quickslot.Reset();
            QuickSlotButton.Reset();

            depth = 0;
            gold = 0;

            droppedItems = new SparseArray<List<Item>>();
            portedItems = new SparseArray<List<Item>>();

            foreach (LimitedDrops a in LimitedDrops.Values())
                a.count = 0;

            chapters = new HashSet<int>();

            Ghost.Quest.Reset();
            Wandmaker.Quest.Reset();
            Blacksmith.Quest.Reset();
            Imp.Quest.Reset();

            Generator.FullReset();
            hero = new Hero();
            hero.Live();

            BadgesExtensions.Reset();

            GamesInProgress.selectedClass?.InitHero(hero);
        }

        public static bool IsChallenged(int mask)
        {
            return (challenges & mask) != 0;
        }

        public static Level NewLevel()
        {
            Dungeon.level = null;
            Actor.Clear();

            ++depth;
            if (depth > Statistics.deepestFloor)
            {
                Statistics.deepestFloor = depth;

                if (Statistics.qualifiedForNoKilling)
                    Statistics.completedWithNoKilling = true;
                else
                    Statistics.completedWithNoKilling = false;
            }

            Level level;
            switch (depth)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    level = new SewerLevel();
                    break;
                case 5:
                    level = new SewerBossLevel();
                    break;
                case 6:
                case 7:
                case 8:
                case 9:
                    level = new PrisonLevel();
                    break;
                case 10:
                    level = new NewPrisonBossLevel();
                    break;
                case 11:
                case 12:
                case 13:
                case 14:
                    level = new CavesLevel();
                    break;
                case 15:
                    level = new NewCavesBossLevel();
                    break;
                case 16:
                case 17:
                case 18:
                case 19:
                    level = new CityLevel();
                    break;
                case 20:
                    level = new NewCityBossLevel();
                    break;
                case 21:
                    //logic for old city boss levels, need to spawn a shop on floor 21
                    try
                    {
                        Bundle bundle = FileUtils.BundleFromFile(GamesInProgress.DepthFile(GamesInProgress.curSlot, 20));
                        Type cls = bundle.GetBundle(LEVEL).GetClass("__className");
                        if (cls == typeof(NewCityBossLevel))
                        {
                            level = new HallsLevel();
                        }
                        else
                        {
                            level = new LastShopLevel();
                        }
                    }
                    catch (Exception e)
                    {
                        ShatteredPixelDungeonDash.ReportException(e);
                        level = new HallsLevel();
                    }
                    break;
                case 22:
                case 23:
                case 24:
                    level = new HallsLevel();
                    break;
                case 25:
                    level = new NewHallsBossLevel();
                    break;
                case 26:
                    level = new LastLevel();
                    break;
                default:
                    level = new DeadEndLevel();
                    --Statistics.deepestFloor;
                    break;
            }

            level.Create();

            Statistics.qualifiedForNoKilling = !BossLevel();

            return level;
        }

        public static void ResetLevel()
        {
            Actor.Clear();

            level.Reset();
            SwitchLevel(level, level.entrance);
        }

        public static int SeedCurDepth()
        {
            return SeedForDepth(depth);
        }

        public static int SeedForDepth(int depth)
        {
            int result = 0;

            Rnd.PushGenerator(seed);
            {
                for (int i = 0; i < depth; ++i)
                {
                    //Random.Long(); //we don't care about these values, just need to go through them
                    Rnd.Int(int.MaxValue); //we don't care about these values, just need to go through them
                }
                //long result = Random.Long();
                result = Rnd.Int(int.MaxValue);
            }
            Rnd.PopGenerator();

            return result;
        }

        public static bool ShopOnLevel()
        {
            return depth == 6 || depth == 11 || depth == 16;
        }

        public static bool BossLevel()
        {
            return BossLevel(depth);
        }

        public static bool BossLevel(int depth)
        {
            return depth == 5 || depth == 10 || depth == 15 || depth == 20 || depth == 25;
        }

        public static void SwitchLevel(Level level, int pos)
        {
            if (pos == -2)
            {
                pos = level.exit;
            }
            else if (pos < 0 || pos >= level.Length())
            {
                pos = level.entrance;
            }

            PathFinder.SetMapSize(level.Width(), level.Height());

            Dungeon.level = level;
            Mob.RestoreAllies(level, pos);
            Actor.Init();

            level.AddRespawner();

            hero.pos = pos;

            foreach (Mob m in level.mobs)
            {
                if (m.pos == hero.pos)
                {
                    //displace mob
                    foreach (int i in PathFinder.NEIGHBORS8)
                    {
                        if (Actor.FindChar(m.pos + i) == null && level.passable[m.pos + i])
                        {
                            m.pos += i;
                            break;
                        }
                    }
                }
            }

            Light light = hero.FindBuff<Light>();
            hero.viewDistance = light == null ? level.viewDistance : Math.Max(Light.DISTANCE, level.viewDistance);

            hero.curAction = hero.lastAction = null;

            Observe();
            try
            {
                SaveAll();
            }
            catch (IOException e)
            {
                ShatteredPixelDungeonDash.ReportException(e);
                /*This only catches IO errors. Yes, this means things can go wrong, and they can go wrong catastrophically.
                But when they do the user will get a nice 'report this issue' dialogue, and I can fix the bug.*/
            }
        }

        public static void DropToChasm(Item item)
        {
            int depth = Dungeon.depth + 1;
            List<Item> dropped = Dungeon.droppedItems[depth];
            if (dropped == null)
                Dungeon.droppedItems.Add(depth, dropped = new List<Item>());

            dropped.Add(item);
        }

        public static bool PosNeeded()
        {
            //2 POS each floor set
            int posLeftThisSet = 2 - (LimitedDrops.STRENGTH_POTIONS.count - (depth / 5) * 2);
            if (posLeftThisSet <= 0)
                return false;

            int floorThisSet = (depth % 5);

            //pos drops every two floors, (numbers 1-2, and 3-4) with a 50% chance for the earlier one each time.
            int targetPOSLeft = 2 - floorThisSet / 2;
            if (floorThisSet % 2 == 1 && Rnd.Int(2) == 0)
                --targetPOSLeft;

            if (targetPOSLeft < posLeftThisSet)
                return true;
            else
                return false;
        }

        public static bool SouNeeded()
        {
            int souLeftThisSet;
            //3 SOU each floor set, 1.5 (rounded) on forbidden runes challenge
            if (IsChallenged(Challenges.NO_SCROLLS))
            {
                souLeftThisSet = (int)Math.Round(1.5f - (LimitedDrops.UPGRADE_SCROLLS.count - (depth / 5) * 1.5f), MidpointRounding.AwayFromZero);
            }
            else
            {
                souLeftThisSet = 3 - (LimitedDrops.UPGRADE_SCROLLS.count - (depth / 5) * 3);
            }
            if (souLeftThisSet <= 0)
                return false;

            int floorThisSet = (depth % 5);
            //chance is floors left / scrolls left
            return Rnd.Int(5 - floorThisSet) < souLeftThisSet;
        }

        public static bool AsNeeded()
        {
            //1 AS each floor set
            int asLeftThisSet = 1 - (LimitedDrops.ARCANE_STYLI.count - (depth / 5));
            if (asLeftThisSet <= 0)
                return false;

            int floorThisSet = (depth % 5);
            //chance is floors left / scrolls left
            return Rnd.Int(5 - floorThisSet) < asLeftThisSet;
        }

        private const string VERSION = "version";
        private const string SEED = "seed";
        private const string CHALLENGES = "challenges";
        private const string HERO = "hero";
        private const string GOLD = "gold";
        private const string DEPTH = "depth";
        private const string DROPPED = "dropped%d";
        private const string PORTED = "ported%d";
        private const string LEVEL = "level";
        private const string LIMDROPS = "limited_drops";
        private const string CHAPTERS = "chapters";
        private const string QUESTS = "quests";
        private const string BADGES = "badges";

        public static void SaveGame(int save)
        {
            try
            {
                Bundle bundle = new Bundle();

                version = Game.versionCode;
                bundle.Put(VERSION, version);
                bundle.Put(SEED, seed);
                bundle.Put(CHALLENGES, challenges);
                bundle.Put(HERO, hero);
                bundle.Put(GOLD, gold);
                bundle.Put(DEPTH, depth);

                foreach (var pair in droppedItems)
                {
                    int d = pair.Key;
                    var value = pair.Value;
                    bundle.Put(Messages.Format(DROPPED, d), value);
                }

                foreach (var pair in portedItems)
                {
                    int p = pair.Key;
                    var value = pair.Value;
                    bundle.Put(Messages.Format(PORTED, p), value);
                }

                quickslot.StorePlaceholders(bundle);

                Bundle limDrops = new Bundle();
                LimitedDrops.Store(limDrops);
                bundle.Put(LIMDROPS, limDrops);

                int count = 0;
                int[] ids = new int[chapters.Count];
                foreach (int id in chapters)
                    ids[count++] = id;

                bundle.Put(CHAPTERS, ids);

                Bundle quests = new Bundle();
                Ghost.Quest.StoreInBundle(quests);
                Wandmaker.Quest.StoreInBundle(quests);
                Blacksmith.Quest.StoreInBundle(quests);
                Imp.Quest.StoreInBundle(quests);
                bundle.Put(QUESTS, quests);

                SpecialRoom.StoreSpecialRoomsInBundle(bundle);
                SecretRoom.StoreSecretRoomsInBundle(bundle);

                Statistics.StoreInBundle(bundle);
                Notes.StoreInBundle(bundle);
                Generator.StoreInBundle(bundle);

                Scroll.Save(bundle);
                Potion.Save(bundle);
                Ring.Save(bundle);

                Actor.StoreNextID(bundle);

                Bundle badges = new Bundle();
                BadgesExtensions.SaveLocal(badges);
                bundle.Put(BADGES, badges);

                FileUtils.BundleToFile(GamesInProgress.GameFile(save), bundle);
            }
            catch (IOException e)
            {
                GamesInProgress.SetUnknown(save);
                ShatteredPixelDungeonDash.ReportException(e);
            }
        }

        public static void SaveLevel(int save)
        {
            Bundle bundle = new Bundle();
            bundle.Put(LEVEL, level);

            FileUtils.BundleToFile(GamesInProgress.DepthFile(save, depth), bundle);
        }

        public static void SaveAll()
        {
            if (hero != null && hero.IsAlive())
            {
                Actor.FixTime();
                SaveGame(GamesInProgress.curSlot);
                SaveLevel(GamesInProgress.curSlot);

                GamesInProgress.Set(GamesInProgress.curSlot, depth, challenges, hero);
            }
        }

        public static void LoadGame(int save)
        {
            LoadGame(save, true);
        }

        public static void LoadGame(int save, bool fullLoad)
        {
            Bundle bundle = FileUtils.BundleFromFile(GamesInProgress.GameFile(save));

            version = bundle.GetInt(VERSION);

            //seed = bundle.contains(SEED) ? bundle.getLong(SEED) : DungeonSeed.randomSeed();
            seed = bundle.Contains(SEED) ? bundle.GetInt(SEED) : Rnd.Int(int.MaxValue); //DungeonSeed.randomSeed();

            Actor.RestoreNextID(bundle);

            quickslot.Reset();
            QuickSlotButton.Reset();

            Dungeon.challenges = bundle.GetInt(CHALLENGES);

            Dungeon.level = null;
            Dungeon.depth = -1;

            Scroll.Restore(bundle);
            Potion.Restore(bundle);
            Ring.Restore(bundle);

            quickslot.RestorePlaceholders(bundle);

            if (fullLoad)
            {
                LimitedDrops.Restore(bundle.GetBundle(LIMDROPS));

                chapters = new HashSet<int>();
                int[] ids = bundle.GetIntArray(CHAPTERS);
                if (ids != null)
                {
                    foreach (int id in ids)
                        chapters.Add(id);
                }

                Bundle quests = bundle.GetBundle(QUESTS);
                if (!quests.IsNull())
                {
                    Ghost.Quest.RestoreFromBundle(quests);
                    Wandmaker.Quest.RestoreFromBundle(quests);
                    Blacksmith.Quest.RestoreFromBundle(quests);
                    Imp.Quest.RestoreFromBundle(quests);
                }
                else
                {
                    Ghost.Quest.Reset();
                    Wandmaker.Quest.Reset();
                    Blacksmith.Quest.Reset();
                    Imp.Quest.Reset();
                }

                SpecialRoom.RestoreSpecialRoomsFromBundle(bundle);
                SecretRoom.RestoreSecretRoomsFromBundle(bundle);
            }

            Bundle badges = bundle.GetBundle(BADGES);
            if (!badges.IsNull())
                BadgesExtensions.LoadLocal(badges);
            else
                BadgesExtensions.Reset();

            Notes.RestoreFromBundle(bundle);

            hero = null;                        // 제거하면 안됨( bundle.Get 내부에서 null check로 분기하는 코드가 있음 )
            hero = (Hero)bundle.Get(HERO);

            gold = bundle.GetInt(GOLD);
            depth = bundle.GetInt(DEPTH);

            Statistics.RestoreFromBundle(bundle);
            Generator.RestoreFromBundle(bundle);

            droppedItems = new SparseArray<List<Item>>();
            portedItems = new SparseArray<List<Item>>();
            for (int i = 1; i <= 26; ++i)
            {
                //dropped items
                List<Item> items = new List<Item>();
                if (bundle.Contains(Messages.Format(DROPPED, i)))
                {
                    foreach (var b in bundle.GetCollection(Messages.Format(DROPPED, i)))
                        items.Add((Item)b);
                }
                if (items.Count > 0)
                    droppedItems.Add(i, items);

                //ported items
                items = new List<Item>();
                if (bundle.Contains(Messages.Format(PORTED, i)))
                {
                    foreach (var b in bundle.GetCollection(Messages.Format(PORTED, i)))
                        items.Add((Item)b);
                }
                if (items.Count > 0)
                    portedItems.Add(i, items);
            }
        }

        public static Level LoadLevel(int save)
        {
            Dungeon.level = null;
            Actor.Clear();

            Bundle bundle = FileUtils.BundleFromFile(GamesInProgress.DepthFile(save, depth));

            Level level = (Level)bundle.Get(LEVEL);

            if (level == null)
            {
                throw new IOException();
            }
            else
            {
                return level;
            }
        }

        public static void DeleteGame(int save, bool deleteLevels)
        {
            FileUtils.DeleteFile(GamesInProgress.GameFile(save));

            if (deleteLevels)
            {
                FileUtils.DeleteDir(GamesInProgress.GameFolder(save));
            }

            GamesInProgress.Delete(save);
        }

        public static void Preview(GamesInProgress.Info info, Bundle bundle)
        {
            info.depth = bundle.GetInt(DEPTH);
            info.version = bundle.GetInt(VERSION);
            info.challenges = bundle.GetInt(CHALLENGES);
            Hero.Preview(info, bundle.GetBundle(HERO));
            Statistics.Preview(info, bundle);
        }

        public static void Fail(Type cause)
        {
            if (hero.belongings.GetItem<Ankh>() == null)
            {
                Rankings.Instance.Submit(false, cause);
            }
        }

        public static void Win(Type cause)
        {
            hero.belongings.Identify();

            int chCount = 0;
            foreach (int ch in Challenges.MASKS)
            {
                if ((challenges & ch) != 0)
                    ++chCount;
            }

            if (chCount != 0)
                BadgesExtensions.ValidateChampion(chCount);

            Rankings.Instance.Submit(true, cause);
        }

        //TODO hero max vision is now separate from shadowcaster max vision. Might want to adjust.
        public static void Observe()
        {
            Observe(ShadowCaster.MAX_DISTANCE + 1);
        }

        public static void Observe(int dist)
        {
            if (level == null)
                return;

            level.UpdateFieldOfView(hero, level.heroFOV);

            int x = hero.pos % level.Width();
            int y = hero.pos / level.Width();

            //left, right, top, bottom
            int l = Math.Max(0, x - dist);
            int r = Math.Min(x + dist, level.Width() - 1);
            int t = Math.Max(0, y - dist);
            int b = Math.Min(y + dist, level.Height() - 1);

            int width = r - l + 1;
            int height = b - t + 1;

            int pos = l + t * level.Width();

            for (int i = t; i <= b; ++i)
            {
                BArray.Or(level.visited, level.heroFOV, pos, width, level.visited);
                pos += level.Width();
            }

            GameScene.UpdateFog(l, t, width, height);

            if (hero.FindBuff<MindVision>() != null)
            {
                foreach (Mob m in level.mobs.ToArray())
                {
                    BArray.Or(level.visited, level.heroFOV, m.pos - 1 - level.Width(), 3, level.visited);
                    BArray.Or(level.visited, level.heroFOV, m.pos, 3, level.visited);
                    BArray.Or(level.visited, level.heroFOV, m.pos - 1 + level.Width(), 3, level.visited);
                    //updates adjacent cells too
                    GameScene.UpdateFog(m.pos, 2);
                }
            }

            if (hero.FindBuff<Awareness>() != null)
            {
                foreach (Heap h in level.heaps.Values)
                {
                    BArray.Or(level.visited, level.heroFOV, h.pos - 1 - level.Width(), 3, level.visited);
                    BArray.Or(level.visited, level.heroFOV, h.pos - 1, 3, level.visited);
                    BArray.Or(level.visited, level.heroFOV, h.pos - 1 + level.Width(), 3, level.visited);
                    GameScene.UpdateFog(h.pos, 2);
                }
            }

            foreach (var c in hero.Buffs<TalismanOfForesight.CharAwareness>())
            {
                if (Dungeon.depth != c.depth)
                    continue;
                var ch = (Character)Actor.FindById(c.charID);
                if (ch == null)
                    continue;
                BArray.Or(level.visited, level.heroFOV, ch.pos - 1 - level.Width(), 3, level.visited);
                BArray.Or(level.visited, level.heroFOV, ch.pos - 1, 3, level.visited);
                BArray.Or(level.visited, level.heroFOV, ch.pos - 1 + level.Width(), 3, level.visited);
                GameScene.UpdateFog(ch.pos, 2);
            }

            foreach (var h in hero.Buffs<TalismanOfForesight.HeapAwareness>())
            {
                if (Dungeon.depth != h.depth)
                    continue;
                BArray.Or(level.visited, level.heroFOV, h.pos - 1 - level.Width(), 3, level.visited);
                BArray.Or(level.visited, level.heroFOV, h.pos - 1, 3, level.visited);
                BArray.Or(level.visited, level.heroFOV, h.pos - 1 + level.Width(), 3, level.visited);
                GameScene.UpdateFog(h.pos, 2);
            }

            GameScene.AfterObserve();
        }

        //we store this to avoid having to re-allocate the array with each pathfind
        private static bool[] passable;

        private static void SetupPassable()
        {
            if (passable == null || passable.Length != Dungeon.level.Length())
                passable = new bool[Dungeon.level.Length()];
            else
                BArray.SetFalse(passable);
        }

        public static PathFinder.Path FindPath(Character ch, int to, bool[] pass, bool[] vis, bool chars)
        {
            SetupPassable();
            if (ch.flying || ch.FindBuff<Amok>() != null)
            {
                BArray.Or(pass, Dungeon.level.avoid, passable);
            }
            else
            {
                Array.Copy(pass, 0, passable, 0, Dungeon.level.Length());
            }

            if (Character.HasProp(ch, Character.Property.LARGE))
            {
                BArray.And(pass, Dungeon.level.openSpace, passable);
            }

            if (chars)
            {
                foreach (var c in Actor.Chars())
                {
                    if (vis[c.pos])
                        passable[c.pos] = false;
                }
            }

            return PathFinder.Find(ch.pos, to, passable);
        }

        public static int FindStep(Character ch, int to, bool[] pass, bool[] visible, bool chars)
        {
            if (Dungeon.level.Adjacent(ch.pos, to))
            {
                return Actor.FindChar(to) == null && (pass[to] || Dungeon.level.avoid[to]) ? to : -1;
            }

            SetupPassable();
            if (ch.flying || ch.FindBuff<Amok>() != null)
            {
                BArray.Or(pass, Dungeon.level.avoid, passable);
            }
            else
            {
                Array.Copy(pass, 0, passable, 0, Dungeon.level.Length());
            }

            if (Character.HasProp(ch, Character.Property.LARGE))
            {
                BArray.And(pass, Dungeon.level.openSpace, passable);
            }

            if (chars)
            {
                foreach (Character c in Actor.Chars())
                {
                    if (visible[c.pos])
                    {
                        passable[c.pos] = false;
                    }
                }
            }

            return PathFinder.GetStep(ch.pos, to, passable);
        }

        public static int Flee(Character ch, int from, bool[] pass, bool[] visible, bool chars)
        {
            SetupPassable();
            if (ch.flying)
            {
                BArray.Or(pass, Dungeon.level.avoid, passable);
            }
            else
            {
                Array.Copy(pass, 0, passable, 0, Dungeon.level.Length());
            }

            if (Character.HasProp(ch, Character.Property.LARGE))
            {
                BArray.And(pass, Dungeon.level.openSpace, passable);
            }

            if (chars)
            {
                foreach (var c in Actor.Chars())
                {
                    if (visible[c.pos])
                    {
                        passable[c.pos] = false;
                    }
                }
            }
            passable[ch.pos] = true;

            return PathFinder.GetStepBack(ch.pos, from, passable);
        }
    }
}