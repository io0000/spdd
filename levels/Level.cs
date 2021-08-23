using System;
using System.Collections.Generic;
using System.Linq;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.blobs;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.actors.mobs.npcs;
using spdd.effects.particles;
using spdd.items;
using spdd.items.artifacts;
using spdd.items.food;
using spdd.items.potions;
using spdd.items.scrolls;
using spdd.items.stones;
using spdd.items.wands;
using spdd.levels.features;
using spdd.levels.painters;
using spdd.levels.traps;
using spdd.mechanics;
using spdd.messages;
using spdd.plants;
using spdd.scenes;
using spdd.sprites;
using spdd.tiles;
using spdd.utils;

namespace spdd.levels
{
    public abstract class Level : IBundlable
    {
        public enum Feeling
        {
            NONE,
            CHASM,
            WATER,
            GRASS,
            DARK
        }

        public int width;
        public int height;
        public int length;

        public const float TIME_TO_RESPAWN = 50;

        public int version;

        public int[] map;
        public bool[] visited;
        public bool[] mapped;
        public bool[] discoverable;

        public int viewDistance = Dungeon.IsChallenged(Challenges.DARKNESS) ? 2 : 8;

        public bool[] heroFOV;

        public bool[] passable;
        public bool[] losBlocking;
        public bool[] flamable;
        public bool[] secret;
        public bool[] solid;
        public bool[] avoid;
        public bool[] water;
        public bool[] pit;

        public bool[] openSpace;

        public Feeling feeling = Feeling.NONE;

        public int entrance;
        public int exit;

        //when a boss level has become locked.
        public bool locked;

        public HashSet<Mob> mobs;
        public SparseArray<Heap> heaps;
        public Dictionary<Type, Blob> blobs;
        public SparseArray<Plant> plants;
        public SparseArray<Trap> traps;
        public HashSet<CustomTilemap> customTiles;
        public HashSet<CustomTilemap> customWalls;

        protected List<Item> itemsToSpawn = new List<Item>();

        protected Group visuals;

        public Color color1 = new Color(0x00, 0x44, 0x00, 0xFF);
        public Color color2 = new Color(0x88, 0xCC, 0x44, 0xFF);

        private const string VERSION = "version";
        private const string WIDTH = "width";
        private const string HEIGHT = "height";
        private const string MAP = "map";
        private const string VISITED = "visited";
        private const string MAPPED = "mapped";
        private const string ENTRANCE = "entrance";
        private const string EXIT = "exit";
        private const string LOCKED = "locked";
        private const string HEAPS = "heaps";
        private const string PLANTS = "plants";
        private const string TRAPS = "traps";
        private const string CUSTOM_TILES = "customTiles";
        private const string CUSTOM_WALLS = "customWalls";
        private const string MOBS = "mobs";
        private const string BLOBS = "blobs";
        private const string FEELING = "feeling";

        public virtual void Create()
        {
            Rnd.PushGenerator(Dungeon.SeedCurDepth());

            if (!Dungeon.BossLevel())
            {
                if (Dungeon.IsChallenged(Challenges.NO_FOOD))
                {
                    AddItemToSpawn(new SmallRation());
                }
                else
                {
                    AddItemToSpawn(Generator.Random(Generator.Category.FOOD));
                }

                if (Dungeon.IsChallenged(Challenges.DARKNESS))
                {
                    AddItemToSpawn(new Torch());
                }

                if (Dungeon.PosNeeded())
                {
                    AddItemToSpawn(new PotionOfStrength());
                    ++Dungeon.LimitedDrops.STRENGTH_POTIONS.count;
                }

                if (Dungeon.SouNeeded())
                {
                    AddItemToSpawn(new ScrollOfUpgrade());
                    ++Dungeon.LimitedDrops.UPGRADE_SCROLLS.count;
                }

                if (Dungeon.AsNeeded())
                {
                    AddItemToSpawn(new Stylus());
                    ++Dungeon.LimitedDrops.ARCANE_STYLI.count;
                }

                //one scroll of transmutation is guaranteed to spawn somewhere on chapter 2-4
                int enchChapter = (int)((Dungeon.seed / 10) % 3) + 1;
                if (Dungeon.depth / 5 == enchChapter &&
                    Dungeon.seed % 4 + 1 == Dungeon.depth % 5)
                {
                    AddItemToSpawn(new StoneOfEnchantment());
                }

                if (Dungeon.depth == ((Dungeon.seed % 3) + 1))
                {
                    AddItemToSpawn(new StoneOfIntuition());
                }

                if (Dungeon.depth > 1)
                {
                    switch (Rnd.Int(10))
                    {
                        case 0:
                            if (!Dungeon.BossLevel(Dungeon.depth + 1))
                                feeling = Feeling.CHASM;
                            break;
                        case 1:
                            feeling = Feeling.WATER;
                            break;
                        case 2:
                            feeling = Feeling.GRASS;
                            break;
                        case 3:
                            feeling = Feeling.DARK;
                            AddItemToSpawn(new Torch());
                            viewDistance = (int)Math.Round(viewDistance / 2f, MidpointRounding.AwayFromZero);
                            break;
                    }
                }
            }

            do
            {
                width = height = length = 0;

                mobs = new HashSet<Mob>();
                heaps = new SparseArray<Heap>();
                blobs = new Dictionary<Type, Blob>();
                plants = new SparseArray<Plant>();
                traps = new SparseArray<Trap>();
                customTiles = new HashSet<CustomTilemap>();
                customWalls = new HashSet<CustomTilemap>();
            }
            while (!Build());

            BuildFlagMaps();
            CleanWalls();

            CreateMobs();
            CreateItems();

            // tt
            //Drop(new Amulet(), entrance + 1);

            Rnd.PopGenerator();
        }

        public void SetSize(int w, int h)
        {
            width = w;
            height = h;
            length = w * h;

            map = new int[length];
            //Arrays.fill(map, feeling == Level.Feeling.CHASM ? Terrain.CHASM : Terrain.WALL);
            var fillValue = feeling == Level.Feeling.CHASM ? Terrain.CHASM : Terrain.WALL;
            Array.Fill(map, fillValue);

            visited = new bool[length];
            mapped = new bool[length];

            heroFOV = new bool[length];

            passable = new bool[length];
            losBlocking = new bool[length];
            flamable = new bool[length];
            secret = new bool[length];
            solid = new bool[length];
            avoid = new bool[length];
            water = new bool[length];
            pit = new bool[length];

            openSpace = new bool[length];

            PathFinder.SetMapSize(w, h);
        }

        public void Reset()
        {
            foreach (var mob in mobs.ToArray())
            {
                if (!mob.Reset())
                    mobs.Remove(mob);
            }

            CreateMobs();
        }

        // interface
        public virtual void RestoreFromBundle(Bundle bundle)
        {
            version = bundle.GetInt(VERSION);

            ////saves from before v0.7.3b are not supported
            //if (version < ShatteredPixelDungeon.v0_7_3b)
            //{
            //    throw new RuntimeException("old save");
            //}

            SetSize(bundle.GetInt(WIDTH), bundle.GetInt(HEIGHT));

            mobs = new HashSet<Mob>();
            heaps = new SparseArray<Heap>();
            blobs = new Dictionary<Type, Blob>();
            plants = new SparseArray<Plant>();
            traps = new SparseArray<Trap>();
            customTiles = new HashSet<CustomTilemap>();
            customWalls = new HashSet<CustomTilemap>();

            map = bundle.GetIntArray(MAP);

            visited = bundle.GetBooleanArray(VISITED);
            mapped = bundle.GetBooleanArray(MAPPED);

            entrance = bundle.GetInt(ENTRANCE);
            exit = bundle.GetInt(EXIT);

            locked = bundle.GetBoolean(LOCKED);

            var collection = bundle.GetCollection(HEAPS);
            foreach (var h in collection)
            {
                var heap = (Heap)h;
                if (!heap.IsEmpty())
                    heaps.Add(heap.pos, heap);
            }

            collection = bundle.GetCollection(PLANTS);
            foreach (var p in collection)
            {
                var plant = (Plant)p;
                plants.Add(plant.pos, plant);
            }

            collection = bundle.GetCollection(TRAPS);
            foreach (var p in collection)
            {
                Trap trap = (Trap)p;
                traps.Add(trap.pos, trap);
            }

            collection = bundle.GetCollection(CUSTOM_TILES);
            foreach (var p in collection)
            {
                CustomTilemap vis = (CustomTilemap)p;
                customTiles.Add(vis);
            }

            collection = bundle.GetCollection(CUSTOM_WALLS);
            foreach (var p in collection)
            {
                CustomTilemap vis = (CustomTilemap)p;
                customWalls.Add(vis);
            }

            collection = bundle.GetCollection(MOBS);
            foreach (var m in collection)
            {
                var mob = (Mob)m;
                if (mob != null)
                    mobs.Add(mob);
            }

            collection = bundle.GetCollection(BLOBS);
            foreach (var b in collection)
            {
                var blob = (Blob)b;
                blobs.Add(blob.GetType(), blob);
            }

            feeling = bundle.GetEnum<Feeling>(FEELING);
            if (feeling == Feeling.DARK)
                viewDistance = (int)Math.Round(viewDistance / 2f, MidpointRounding.AwayFromZero);

            if (bundle.Contains("mobs_to_spawn"))
            {
                foreach (var mob in bundle.GetClassArray("mobs_to_spawn"))
                {
                    if (mob != null)
                        mobsToSpawn.Add(mob);
                }
            }

            if (bundle.Contains("respawner"))
                respawner = (Respawner)bundle.Get("respawner");

            BuildFlagMaps();
            CleanWalls();

            ////compat with pre-0.8.0 saves
            //for (Heap h : heaps.valueList())
            //{
            //    if (h.type == Heap.Type.MIMIC)
            //    {
            //        heaps.remove(h.pos);
            //        mobs.add(Mimic.spawnAt(h.pos, h.items));
            //    }
            //}
        }

        public virtual void StoreInBundle(Bundle bundle)
        {
            bundle.Put(VERSION, Game.versionCode);
            bundle.Put(WIDTH, width);
            bundle.Put(HEIGHT, height);
            bundle.Put(MAP, map);
            bundle.Put(VISITED, visited);
            bundle.Put(MAPPED, mapped);
            bundle.Put(ENTRANCE, entrance);
            bundle.Put(EXIT, exit);
            bundle.Put(LOCKED, locked);
            bundle.Put(HEAPS, heaps.Values.ToList());
            bundle.Put(PLANTS, plants.Values.ToList());
            bundle.Put(TRAPS, traps.Values.ToList());
            bundle.Put(CUSTOM_TILES, customTiles);
            bundle.Put(CUSTOM_WALLS, customWalls);
            bundle.Put(MOBS, mobs);
            bundle.Put(BLOBS, blobs.Values);
            bundle.Put(FEELING, feeling.ToString());    // enum
            bundle.Put("mobs_to_spawn", mobsToSpawn.ToArray());
            bundle.Put("respawner", respawner);
        }

        public int TunnelTile()
        {
            return feeling == Feeling.CHASM ? Terrain.EMPTY_SP : Terrain.EMPTY;
        }

        public int Width()
        {
            return width;
        }

        public int Height()
        {
            return height;
        }

        public int Length()
        {
            return length;
        }

        public virtual string TilesTex()
        {
            return null;
        }

        public virtual string WaterTex()
        {
            return null;
        }

        public abstract bool Build();

        // Type - Class<?extends Mob>
        private List<Type> mobsToSpawn = new List<Type>();

        public virtual Mob CreateMob()
        {
            if (mobsToSpawn == null || mobsToSpawn.Count == 0)
                mobsToSpawn = Bestiary.GetMobRotation(Dungeon.depth);

            Type type = mobsToSpawn[0];
            mobsToSpawn.RemoveAt(0);

            return (Mob)Reflection.NewInstance(type);
        }

        public abstract void CreateMobs();

        public abstract void CreateItems();

        public virtual void Seal()
        {
            if (!locked)
            {
                locked = true;
                Buff.Affect<LockedFloor>(Dungeon.hero);
            }
        }

        public virtual void Unseal()
        {
            if (locked)
            {
                locked = false;
                var buff = Dungeon.hero.FindBuff<LockedFloor>();
                if (buff != null)
                    buff.Detach();
            }
        }

        public virtual Group AddVisuals()
        {
            if (visuals == null || visuals.parent == null)
            {
                visuals = new Group();
            }
            else
            {
                visuals.Clear();
                visuals.camera = null;
            }

            for (int i = 0; i < Length(); ++i)
            {
                if (pit[i])
                {
                    visuals.Add(new WindParticle.Wind(i));
                    if (i >= Width() && water[i - Width()])
                    {
                        visuals.Add(new FlowParticle.Flow(i - Width()));
                    }
                }
            }
            return visuals;
        }

        public virtual int NMobs()
        {
            return 0;
        }

        public Mob FindMob(int pos)
        {
            foreach (Mob mob in mobs)
            {
                if (mob.pos == pos)
                    return mob;
            }
            return null;
        }

        private Respawner respawner;

        public virtual Actor AddRespawner()
        {
            if (respawner == null)
            {
                respawner = new Respawner();
                Actor.AddDelayed(respawner, RespawnCooldown());
            }
            else
            {
                Actor.Add(respawner);
            }
            return respawner;
        }

        [SPDStatic]
        public class Respawner : Actor
        {
            public Respawner()
            {
                actPriority = BUFF_PRIO; //as if it were a buff.
            }

            public override bool Act()
            {
                float count = 0;

                foreach (Mob mob in Dungeon.level.mobs.ToArray())
                {
                    if (mob.alignment == Character.Alignment.ENEMY && !mob.Properties().Contains(Character.Property.MINIBOSS))
                    {
                        count += mob.SpawningWeight();
                    }
                }

                if (count < Dungeon.level.NMobs())
                {
                    PathFinder.BuildDistanceMap(Dungeon.hero.pos, BArray.Or(Dungeon.level.passable, Dungeon.level.avoid, null));

                    Mob mob = Dungeon.level.CreateMob();
                    mob.state = mob.WANDERING;
                    mob.pos = Dungeon.level.RandomRespawnCell(null);
                    if (Dungeon.hero.IsAlive() && mob.pos != -1 && PathFinder.distance[mob.pos] >= 12)
                    {
                        GameScene.Add(mob);
                        if (Statistics.amuletObtained)
                            mob.Beckon(Dungeon.hero.pos);

                        Spend(Dungeon.level.RespawnCooldown());
                    }
                    else
                    {
                        //try again in 1 turn
                        Spend(TICK);
                    }
                }
                else
                {
                    Spend(Dungeon.level.RespawnCooldown());
                }

                return true;
            }
        }

        public float RespawnCooldown()
        {
            if (Statistics.amuletObtained)
                return TIME_TO_RESPAWN / 2f;
            else if (Dungeon.level.feeling == Feeling.DARK)
                return 2 * TIME_TO_RESPAWN / 3f;
            else
                return TIME_TO_RESPAWN;
        }

        public virtual int RandomRespawnCell(Character ch)
        {
            int cell;
            do
            {
                cell = Rnd.Int(Length());
            }
            while ((Dungeon.level == this && heroFOV[cell]) ||
                    !passable[cell] ||
                    (Character.HasProp(ch, Character.Property.LARGE) && !openSpace[cell]) ||
                    Actor.FindChar(cell) != null);

            return cell;
        }

        public virtual int RandomDestination(Character ch)
        {
            int cell;
            do
            {
                cell = Rnd.Int(Length());
            }
            while (!passable[cell] ||
                   (Character.HasProp(ch, Character.Property.LARGE) && !openSpace[cell]));

            return cell;
        }

        public void AddItemToSpawn(Item item)
        {
            if (item != null)
                itemsToSpawn.Add(item);
        }

        public Item FindPrizeItem()
        {
            return FindPrizeItem(null);
        }

        public Item FindPrizeItem(Type match)
        {
            if (itemsToSpawn.Count == 0)
                return null;

            if (match == null)
            {
                Item item = Rnd.Element(itemsToSpawn);
                itemsToSpawn.Remove(item);
                return item;
            }

            foreach (Item item in itemsToSpawn)
            {
                // if (match.isInstance(item))
                if (match.IsAssignableFrom(item.GetType()))
                {
                    itemsToSpawn.Remove(item);
                    return item;
                }
            }

            return null;
        }

        public void BuildFlagMaps()
        {
            for (var i = 0; i < Length(); ++i)
            {
                var flags = Terrain.flags[map[i]];

                passable[i] = (flags & Terrain.PASSABLE) != 0;
                losBlocking[i] = (flags & Terrain.LOS_BLOCKING) != 0;
                flamable[i] = (flags & Terrain.FLAMABLE) != 0;
                secret[i] = (flags & Terrain.SECRET) != 0;
                solid[i] = (flags & Terrain.SOLID) != 0;
                avoid[i] = (flags & Terrain.AVOID) != 0;
                water[i] = (flags & Terrain.LIQUID) != 0;
                pit[i] = (flags & Terrain.PIT) != 0;
            }

            var s = (SmokeScreen)GetBlob(typeof(SmokeScreen));
            if (s != null && s.volume > 0)
            {
                for (int i = 0; i < Length(); ++i)
                    losBlocking[i] = losBlocking[i] || s.cur[i] > 0;
            }

            var w = (Web)GetBlob(typeof(Web));
            if (w != null && w.volume > 0)
            {
                for (int i = 0; i < Length(); ++i)
                    solid[i] = solid[i] || w.cur[i] > 0;
            }

            int lastRow = Length() - Width();
            for (var i = 0; i < Width(); ++i)
            {
                passable[i] = avoid[i] = false;
                losBlocking[i] = solid[i] = true;
                passable[lastRow + i] = avoid[lastRow + i] = false;
                losBlocking[lastRow + i] = solid[lastRow + i] = true;
            }

            for (int i = Width(); i < lastRow; i += Width())
            {
                passable[i] = avoid[i] = false;
                losBlocking[i] = solid[i] = true;
                passable[i + Width() - 1] = avoid[i + Width() - 1] = false;
                losBlocking[i + Width() - 1] = solid[i + Width() - 1] = true;
            }

            //an open space is large enough to fit large mobs. A space is open when it is not solid
            // and there is and open corner with both adjacent cells opens
            for (int i = 0; i < Length(); ++i)
            {
                if (solid[i])
                {
                    openSpace[i] = false;
                }
                else
                {
                    for (int j = 1; j < PathFinder.CIRCLE8.Length; j += 2)
                    {
                        if (solid[i + PathFinder.CIRCLE8[j]])
                        {
                            openSpace[i] = false;
                        }
                        else if (!solid[i + PathFinder.CIRCLE8[(j + 1) % 8]] &&
                                 !solid[i + PathFinder.CIRCLE8[(j + 2) % 8]])
                        {
                            openSpace[i] = true;
                            break;
                        }
                    }
                }
            }
        }

        public void Destroy(int pos)
        {
            Set(pos, Terrain.EMBERS);
        }

        public void CleanWalls()
        {
            if (discoverable == null || discoverable.Length != length)
            {
                discoverable = new bool[Length()];
            }

            for (int i = 0; i < Length(); ++i)
            {
                bool d = false;

                for (int j = 0; j < PathFinder.NEIGHBORS9.Length; ++j)
                {
                    int n = i + PathFinder.NEIGHBORS9[j];
                    if (n >= 0 && n < Length() && map[n] != Terrain.WALL && map[n] != Terrain.WALL_DECO)
                    {
                        d = true;
                        break;
                    }
                }

                discoverable[i] = d;
            }
        }

        public static void Set(int cell, int terrain)
        {
            Set(cell, terrain, Dungeon.level);
        }

        public static void Set(int cell, int terrain, Level level)
        {
            Painter.Set(level, cell, terrain);

            if (terrain != Terrain.TRAP && terrain != Terrain.SECRET_TRAP && terrain != Terrain.INACTIVE_TRAP)
                level.traps.Remove(cell);

            int flags = Terrain.flags[terrain];
            level.passable[cell] = (flags & Terrain.PASSABLE) != 0;
            level.losBlocking[cell] = (flags & Terrain.LOS_BLOCKING) != 0;
            level.flamable[cell] = (flags & Terrain.FLAMABLE) != 0;
            level.secret[cell] = (flags & Terrain.SECRET) != 0;
            level.solid[cell] = (flags & Terrain.SOLID) != 0;
            level.avoid[cell] = (flags & Terrain.AVOID) != 0;
            level.pit[cell] = (flags & Terrain.PIT) != 0;
            level.water[cell] = terrain == Terrain.WATER;

            var s = (SmokeScreen)level.GetBlob(typeof(SmokeScreen));
            if (s != null && s.volume > 0)
            {
                level.losBlocking[cell] = level.losBlocking[cell] || s.cur[cell] > 0;
            }

            foreach (int ii in PathFinder.NEIGHBORS9)
            {
                int i = cell + ii;
                if (level.solid[i])
                {
                    level.openSpace[i] = false;
                }
                else
                {
                    for (int j = 1; j < PathFinder.CIRCLE8.Length; j += 2)
                    {
                        if (level.solid[i + PathFinder.CIRCLE8[j]])
                        {
                            level.openSpace[i] = false;
                        }
                        else if (!level.solid[i + PathFinder.CIRCLE8[(j + 1) % 8]] &&
                                 !level.solid[i + PathFinder.CIRCLE8[(j + 2) % 8]])
                        {
                            level.openSpace[i] = true;
                            break;
                        }
                    }
                }
            }
        }

        public virtual Heap Drop(Item item, int cell)
        {
            if (item == null || Challenges.IsItemBlocked(item))
            {
                //create a dummy heap, give it a dummy sprite, don't add it to the game, and return it.
                //effectively nullifies whatever the logic calling this wants to do, including dropping items.
                Heap h = new Heap();
                ItemSprite sprite = h.sprite = new ItemSprite();
                sprite.Link(h);
                return h;
            }

            Heap heap = heaps[cell];
            if (heap == null)
            {
                heap = new Heap();
                heap.seen = Dungeon.level == this && heroFOV[cell];
                heap.pos = cell;
                heap.Drop(item);
                if (map[cell] == Terrain.CHASM || (Dungeon.level != null && pit[cell]))
                {
                    Dungeon.DropToChasm(item);
                    GameScene.Discard(heap);
                }
                else
                {
                    heaps[cell] = heap;
                    GameScene.Add(heap);
                }
            }
            else if (heap.type == Heap.Type.LOCKED_CHEST || heap.type == Heap.Type.CRYSTAL_CHEST)
            {
                int n;
                do
                {
                    n = cell + PathFinder.NEIGHBORS8[Rnd.Int(8)];
                }
                while (!passable[n] && !avoid[n]);

                return Drop(item, n);
            }
            else
            {
                heap.Drop(item);
            }

            if (Dungeon.level != null && ShatteredPixelDungeonDash.Scene() is GameScene)
            {
                PressCell(cell);
            }

            return heap;
        }

        public Plant Plant(Plant.Seed seed, int pos)
        {
            if (Dungeon.IsChallenged(Challenges.NO_HERBALISM))
                return null;

            Plant plant = plants[pos];
            if (plant != null)
            {
                plant.Wither();
            }

            if (map[pos] == Terrain.HIGH_GRASS ||
                map[pos] == Terrain.FURROWED_GRASS ||
                map[pos] == Terrain.EMPTY ||
                map[pos] == Terrain.EMBERS ||
                map[pos] == Terrain.EMPTY_DECO)
            {
                Set(pos, Terrain.GRASS, this);
                GameScene.UpdateMap(pos);
            }

            plant = seed.Couch(pos, this);
            plants.Add(pos, plant);

            GameScene.PlantSeed(pos);

            foreach (var ch in Actor.Chars())
            {
                if (ch is WandOfRegrowth.Lotus &&
                    ((WandOfRegrowth.Lotus)ch).InRange(pos) &&
                    Actor.FindChar(pos) != null)
                {
                    plant.Trigger();
                    return null;
                }
            }

            return plant;
        }

        public void Uproot(int pos)
        {
            plants.Remove(pos);
            GameScene.UpdateMap(pos);
        }

        public Trap SetTrap(Trap trap, int pos)
        {
            Trap existingTrap = traps[pos];
            if (existingTrap != null)
                traps.Remove(pos);

            trap.Set(pos);
            traps.Add(pos, trap);
            GameScene.UpdateMap(pos);
            return trap;
        }

        public void DisarmTrap(int pos)
        {
            Set(pos, Terrain.INACTIVE_TRAP);
            GameScene.UpdateMap(pos);
        }

        public void Discover(int cell)
        {
            Set(cell, Terrain.Discover(map[cell]));
            Trap trap = traps[cell];
            if (trap != null)
                trap.Reveal();
            GameScene.UpdateMap(cell);
        }

        public virtual bool SetCellToWater(bool includeTraps, int cell)
        {
            Point p = CellToPoint(cell);

            //if a custom tilemap is over that cell, don't put water there
            foreach (CustomTilemap cust in customTiles)
            {
                Point custPoint = new Point(p);
                custPoint.x -= cust.tileX;
                custPoint.y -= cust.tileY;
                if (custPoint.x >= 0 &&
                    custPoint.y >= 0 &&
                    custPoint.x < cust.tileW &&
                    custPoint.y < cust.tileH)
                {
                    if (cust.Image(custPoint.x, custPoint.y) != null)
                        return false;
                }
            }

            int terr = map[cell];
            if (terr == Terrain.EMPTY ||
                terr == Terrain.GRASS ||
                terr == Terrain.EMBERS ||
                terr == Terrain.EMPTY_SP ||
                terr == Terrain.HIGH_GRASS ||
                terr == Terrain.FURROWED_GRASS ||
                terr == Terrain.EMPTY_DECO)
            {
                Set(cell, Terrain.WATER);
                GameScene.UpdateMap(cell);
                return true;
            }
            else if (includeTraps &&
                    (terr == Terrain.SECRET_TRAP ||
                     terr == Terrain.TRAP ||
                     terr == Terrain.INACTIVE_TRAP))
            {
                Set(cell, Terrain.WATER);
                Dungeon.level.traps.Remove(cell);
                GameScene.UpdateMap(cell);
                return true;
            }

            return false;
        }

        public virtual int FallCell(bool fallIntoPit)
        {
            int result;
            do
            {
                result = RandomRespawnCell(null);
            }
            while (traps[result] != null || FindMob(result) != null);

            return result;
        }

        public virtual void OccupyCell(Character ch)
        {
            if (!ch.IsImmune(typeof(Web)) && Blob.VolumeAt(ch.pos, typeof(Web)) > 0)
            {
                GetBlob(typeof(Web)).Clear(ch.pos);
                Web.AffectChar(ch);
            }

            if (!ch.flying)
            {
                if (pit[ch.pos])
                {
                    if (ch == Dungeon.hero)
                    {
                        Chasm.HeroFall(ch.pos);
                    }
                    else if (ch is Mob)
                    {
                        Chasm.MobFall((Mob)ch);
                    }
                    return;
                }

                //characters which are not the hero or a sheep 'soft' press cells
                PressCell(ch.pos, ch is Hero || ch is Sheep);
            }
            else
            {
                if (map[ch.pos] == Terrain.DOOR)
                    Door.Enter(ch.pos);
            }
        }

        //public method for forcing the hard press of a cell. e.g. when an item lands on it
        public void PressCell(int cell)
        {
            PressCell(cell, true);
        }

        //a 'soft' press ignores hidden traps
        //a 'hard' press triggers all things
        private void PressCell(int cell, bool hard)
        {
            Trap trap = null;

            switch (map[cell])
            {
                case Terrain.SECRET_TRAP:
                    if (hard)
                    {
                        trap = traps[cell];
                        GLog.Information(Messages.Get(typeof(Level), "hidden_trap", trap.name));
                    }
                    break;

                case Terrain.TRAP:
                    trap = traps[cell];
                    break;

                case Terrain.HIGH_GRASS:
                case Terrain.FURROWED_GRASS:
                    HighGrass.Trample(this, cell);
                    break;

                case Terrain.WELL:
                    WellWater.AffectCell(cell);
                    break;

                case Terrain.DOOR:
                    Door.Enter(cell);
                    break;
            }

            if (trap != null)
            {
                var timeFreeze = Dungeon.hero.FindBuff<TimekeepersHourglass.TimeFreeze>();
                var bubble = Dungeon.hero.FindBuff<Swiftthistle.TimeBubble>();

                if (bubble != null)
                {
                    Sample.Instance.Play(Assets.Sounds.TRAP);

                    Discover(cell);

                    bubble.SetDelayedPress(cell);
                }
                else if (timeFreeze != null)
                {
                    Sample.Instance.Play(Assets.Sounds.TRAP);

                    Discover(cell);

                    timeFreeze.SetDelayedPress(cell);
                }
                else
                {
                    if (Dungeon.hero.pos == cell)
                        Dungeon.hero.Interrupt();

                    trap.Trigger();
                }
            }

            Plant plant = plants[cell];
            if (plant != null)
                plant.Trigger();

            if (hard && Blob.VolumeAt(cell, typeof(Web)) > 0)
                GetBlob(typeof(Web)).Clear(cell);
        }

        public void UpdateFieldOfView(Character c, bool[] fieldOfView)
        {
            int cx = c.pos % Width();
            int cy = c.pos / Width();

            bool sighted = c.FindBuff<Blindness>() == null &&
                           c.FindBuff<Shadows>() == null &&
                           c.FindBuff<TimekeepersHourglass.TimeStasis>() == null &&
                           c.IsAlive();
            if (sighted)
            {
                bool[] blocking;

                if ((c is Hero && ((Hero)c).subClass == HeroSubClass.WARDEN) ||
                    c is YogFist.SoiledFist)
                {
                    blocking = (bool[])Dungeon.level.losBlocking.Clone();
                    for (int i = 0; i < blocking.Length; ++i)
                    {
                        if (blocking[i] && (Dungeon.level.map[i] == Terrain.HIGH_GRASS || Dungeon.level.map[i] == Terrain.FURROWED_GRASS))
                        {
                            blocking[i] = false;
                        }
                    }
                }
                else
                {
                    blocking = Dungeon.level.losBlocking;
                }

                int viewDist = c.viewDistance;
                if (c is Hero && ((Hero)c).subClass == HeroSubClass.SNIPER)
                    viewDist = (int)(viewDist * 1.5f);

                ShadowCaster.CastShadow(cx, cy, fieldOfView, blocking, viewDist);
            }
            else
            {
                BArray.SetFalse(fieldOfView);
            }

            int sense = 1;
            //Currently only the hero can get mind vision
            if (c.IsAlive() && c == Dungeon.hero)
            {
                foreach (Buff b in c.Buffs<MindVision>())
                    sense = Math.Max(MindVision.DISTANCE, sense);

                if (c.FindBuff<MagicalSight>() != null)
                    sense = 8;

                if (((Hero)c).subClass == HeroSubClass.SNIPER)
                    sense = (int)(sense * 1.5f);
            }

            //uses rounding
            if (!sighted || sense > 1)
            {
                int[][] rounding = ShadowCaster.rounding;

                int left, right;
                int pos;
                for (int y = Math.Max(0, cy - sense); y <= Math.Min(Height() - 1, cy + sense); ++y)
                {
                    if (rounding[sense][Math.Abs(cy - y)] < Math.Abs(cy - y))
                    {
                        left = cx - rounding[sense][Math.Abs(cy - y)];
                    }
                    else
                    {
                        left = sense;
                        while (rounding[sense][left] < rounding[sense][Math.Abs(cy - y)])
                            --left;

                        left = cx - left;
                    }
                    right = Math.Min(Width() - 1, cx + cx - left);
                    left = Math.Max(0, left);
                    pos = left + y * Width();
                    Array.Copy(discoverable, pos, fieldOfView, pos, right - left + 1);
                }
            }

            //Currently only the hero can get mind vision or awareness
            if (c.IsAlive() && c == Dungeon.hero)
            {
                Dungeon.hero.mindVisionEnemies.Clear();
                if (c.FindBuff<MindVision>() != null)
                {
                    foreach (Mob mob in mobs)
                    {
                        int p = mob.pos;

                        if (!fieldOfView[p])
                            Dungeon.hero.mindVisionEnemies.Add(mob);
                    }
                }
                else if (((Hero)c).heroClass == HeroClass.HUNTRESS)
                {
                    foreach (Mob mob in mobs)
                    {
                        int p = mob.pos;
                        if (Distance(c.pos, p) == 2)
                        {
                            if (!fieldOfView[p])
                                Dungeon.hero.mindVisionEnemies.Add(mob);
                        }
                    }
                }

                foreach (Mob m in Dungeon.hero.mindVisionEnemies)
                {
                    foreach (int i in PathFinder.NEIGHBORS9)
                        fieldOfView[m.pos + i] = true;
                }

                if (c.FindBuff<Awareness>() != null)
                {
                    foreach (Heap heap in heaps.Values)
                    {
                        int p = heap.pos;
                        foreach (int i in PathFinder.NEIGHBORS9)
                            fieldOfView[p + i] = true;
                    }
                }

                foreach (var a in c.Buffs<TalismanOfForesight.CharAwareness>())
                {
                    if (Dungeon.depth != a.depth)
                        continue;
                    Character ch = (Character)Actor.FindById(a.charID);
                    if (ch == null)
                    {
                        a.Detach();
                        continue;
                    }
                    int p = ch.pos;
                    foreach (int i in PathFinder.NEIGHBORS9)
                        fieldOfView[p + i] = true;
                }

                foreach (var h in c.Buffs<TalismanOfForesight.HeapAwareness>())
                {
                    if (Dungeon.depth != h.depth)
                        continue;
                    foreach (int i in PathFinder.NEIGHBORS9)
                        fieldOfView[h.pos + i] = true;
                }

                foreach (Mob m in mobs)
                {
                    if (m is WandOfWarding.Ward || m is WandOfRegrowth.Lotus)
                    {
                        if (m.fieldOfView == null || m.fieldOfView.Length != Length())
                        {
                            m.fieldOfView = new bool[Length()];
                            Dungeon.level.UpdateFieldOfView(m, m.fieldOfView);
                        }
                        foreach (Mob m1 in mobs)
                        {
                            if (m.fieldOfView[m1.pos] &&
                                !fieldOfView[m1.pos] &&
                                !Dungeon.hero.mindVisionEnemies.Contains(m1))
                            {
                                Dungeon.hero.mindVisionEnemies.Add(m1);
                            }
                        }
                        BArray.Or(fieldOfView, m.fieldOfView, fieldOfView);
                    }
                }
            }

            if (c == Dungeon.hero)
            {
                foreach (var pair in heaps)
                {
                    var heap = pair.Value;
                    if (!heap.seen && fieldOfView[heap.pos])
                        heap.seen = true;
                }
            }
        }

        public int Distance(int a, int b)
        {
            var ax = a % Width();
            var ay = a / Width();
            var bx = b % Width();
            var by = b / Width();
            return Math.Max(Math.Abs(ax - bx), Math.Abs(ay - by));
        }

        public bool Adjacent(int a, int b)
        {
            return Distance(a, b) == 1;
        }

        //uses pythagorean theorum for true distance, as if there was no movement grid
        public float TrueDistance(int a, int b)
        {
            var ax = a % Width();
            var ay = a / Width();
            var bx = b % Width();
            var by = b / Width();
            return (float)Math.Sqrt(Math.Pow(Math.Abs(ax - bx), 2) + Math.Pow(Math.Abs(ay - by), 2));
        }

        //returns true if the input is a valid tile within the level
        public bool InsideMap(int tile)
        {
            //top and bottom row and beyond
            return !((tile < width || tile >= length - width) ||
                    //left and right column
                    (tile % width == 0 || tile % width == width - 1));
        }

        public Point CellToPoint(int cell)
        {
            return new Point(cell % Width(), cell / Width());
        }

        public int PointToCell(Point p)
        {
            return p.x + p.y * Width();
        }

        public virtual string TileName(int tile)
        {
            switch (tile)
            {
                case Terrain.CHASM:
                    return Messages.Get(typeof(Level), "chasm_name");
                case Terrain.EMPTY:
                case Terrain.EMPTY_SP:
                case Terrain.EMPTY_DECO:
                case Terrain.SECRET_TRAP:
                    return Messages.Get(typeof(Level), "floor_name");
                case Terrain.GRASS:
                    return Messages.Get(typeof(Level), "grass_name");
                case Terrain.WATER:
                    return Messages.Get(typeof(Level), "water_name");
                case Terrain.WALL:
                case Terrain.WALL_DECO:
                case Terrain.SECRET_DOOR:
                    return Messages.Get(typeof(Level), "wall_name");
                case Terrain.DOOR:
                    return Messages.Get(typeof(Level), "closed_door_name");
                case Terrain.OPEN_DOOR:
                    return Messages.Get(typeof(Level), "open_door_name");
                case Terrain.ENTRANCE:
                    return Messages.Get(typeof(Level), "entrace_name");
                case Terrain.EXIT:
                    return Messages.Get(typeof(Level), "exit_name");
                case Terrain.EMBERS:
                    return Messages.Get(typeof(Level), "embers_name");
                case Terrain.FURROWED_GRASS:
                    return Messages.Get(typeof(Level), "furrowed_grass_name");
                case Terrain.LOCKED_DOOR:
                    return Messages.Get(typeof(Level), "locked_door_name");
                case Terrain.PEDESTAL:
                    return Messages.Get(typeof(Level), "pedestal_name");
                case Terrain.BARRICADE:
                    return Messages.Get(typeof(Level), "barricade_name");
                case Terrain.HIGH_GRASS:
                    return Messages.Get(typeof(Level), "high_grass_name");
                case Terrain.LOCKED_EXIT:
                    return Messages.Get(typeof(Level), "locked_exit_name");
                case Terrain.UNLOCKED_EXIT:
                    return Messages.Get(typeof(Level), "unlocked_exit_name");
                case Terrain.SIGN:
                    return Messages.Get(typeof(Level), "sign_name");
                case Terrain.WELL:
                    return Messages.Get(typeof(Level), "well_name");
                case Terrain.EMPTY_WELL:
                    return Messages.Get(typeof(Level), "empty_well_name");
                case Terrain.STATUE:
                case Terrain.STATUE_SP:
                    return Messages.Get(typeof(Level), "statue_name");
                case Terrain.INACTIVE_TRAP:
                    return Messages.Get(typeof(Level), "inactive_trap_name");
                case Terrain.BOOKSHELF:
                    return Messages.Get(typeof(Level), "bookshelf_name");
                case Terrain.ALCHEMY:
                    return Messages.Get(typeof(Level), "alchemy_name");
                default:
                    return Messages.Get(typeof(Level), "default_name");
            }
        }

        public virtual string TileDesc(int tile)
        {
            switch (tile)
            {
                case Terrain.CHASM:
                    return Messages.Get(typeof(Level), "chasm_desc");
                case Terrain.WATER:
                    return Messages.Get(typeof(Level), "water_desc");
                case Terrain.ENTRANCE:
                    return Messages.Get(typeof(Level), "entrance_desc");
                case Terrain.EXIT:
                case Terrain.UNLOCKED_EXIT:
                    return Messages.Get(typeof(Level), "exit_desc");
                case Terrain.EMBERS:
                    return Messages.Get(typeof(Level), "embers_desc");
                case Terrain.HIGH_GRASS:
                case Terrain.FURROWED_GRASS:
                    return Messages.Get(typeof(Level), "high_grass_desc");
                case Terrain.LOCKED_DOOR:
                    return Messages.Get(typeof(Level), "locked_door_desc");
                case Terrain.LOCKED_EXIT:
                    return Messages.Get(typeof(Level), "locked_exit_desc");
                case Terrain.BARRICADE:
                    return Messages.Get(typeof(Level), "barricade_desc");
                case Terrain.SIGN:
                    return Messages.Get(typeof(Level), "sign_desc");
                case Terrain.INACTIVE_TRAP:
                    return Messages.Get(typeof(Level), "inactive_trap_desc");
                case Terrain.STATUE:
                case Terrain.STATUE_SP:
                    return Messages.Get(typeof(Level), "statue_desc");
                case Terrain.ALCHEMY:
                    return Messages.Get(typeof(Level), "alchemy_desc");
                case Terrain.EMPTY_WELL:
                    return Messages.Get(typeof(Level), "empty_well_desc");
                default:
                    return "";
            }
        }

        public Blob GetBlob(Type t)
        {
            if (blobs.TryGetValue(t, out Blob blob))
                return blob;
            else
                return null;
        }
    }
}