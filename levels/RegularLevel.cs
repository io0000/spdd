using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors;
using spdd.actors.mobs;
using spdd.items;
using spdd.items.artifacts;
using spdd.items.journal;
using spdd.items.keys;
using spdd.journal;
using spdd.levels.builders;
using spdd.levels.painters;
using spdd.levels.rooms;
using spdd.levels.rooms.secret;
using spdd.levels.rooms.special;
using spdd.levels.rooms.standard;
using spdd.levels.traps;

namespace spdd.levels
{
    public abstract class RegularLevel : Level
    {
        public List<Room> rooms;

        protected Builder builder;

        public Room roomEntrance;
        public Room roomExit;

        public int secretDoors;

        public override bool Build()
        {
            builder = Builder();

            List<Room> initRooms = InitRooms();
            Rnd.Shuffle(initRooms);

            do
            {
                foreach (Room r in initRooms)
                {
                    r.neighbors.Clear();
                    r.connected.Clear();
                }
                rooms = builder.Build(new List<Room>(initRooms));
            }
            while (rooms == null);

            return Painter().Paint(this, rooms);
        }

        public virtual List<Room> InitRooms()
        {
            List<Room> initRooms = new List<Room>();
            initRooms.Add(roomEntrance = new EntranceRoom());
            initRooms.Add(roomExit = new ExitRoom());

            //initRooms.Add(new MagicWellRoom());       //test
            //initRooms.Add(new LaboratoryRoom());      //test
            //initRooms.Add(new VaultRoom());           //test

            int standards = StandardRooms();
            for (int i = 0; i < standards; ++i)
            {
                StandardRoom s;
                do
                {
                    s = StandardRoom.CreateRoom();
                }
                while (!s.SetSizeCat(standards - i));

                i += s.sizeCat.roomValue - 1;
                initRooms.Add(s);
            }

            if (Dungeon.ShopOnLevel())
                initRooms.Add(new ShopRoom());

            int specials = SpecialRooms();
            SpecialRoom.InitForFloor();
            for (int i = 0; i < specials; ++i)
            {
                SpecialRoom s = SpecialRoom.CreateSpecialRoom();
                if (s is PitRoom)
                    ++specials;
                initRooms.Add(s);
            }

            int secrets = SecretRoom.SecretsForFloor(Dungeon.depth);
            for (int i = 0; i < secrets; ++i)
            {
                initRooms.Add(SecretRoom.CreateSecretRoom());
            }

            return initRooms;
        }

        protected virtual int StandardRooms()
        {
            return 0;
        }

        protected virtual int SpecialRooms()
        {
            return 0;
        }

        protected virtual Builder Builder()
        {
            return new LoopBuilder()
                    .SetLoopShape(2,
                            Rnd.Float(0.4f, 0.7f),
                            Rnd.Float(0f, 0.5f));
        }

        protected abstract Painter Painter();

        protected virtual int NTraps()
        {
            return Rnd.NormalIntRange(2, 3 + (Dungeon.depth / 5));
        }

        protected virtual Type[] TrapClasses()
        {
            return new Type[] { typeof(WornDartTrap) };
        }

        protected virtual float[] TrapChances()
        {
            return new float[] { 1 };
        }

        public override int NMobs()
        {
            switch (Dungeon.depth)
            {
                case 1:
                    //mobs are not randomly spawned on floor 1.
                    return 0;
                default:
                    return 3 + Dungeon.depth % 5 + Rnd.Int(3);
            }
        }

        public override void CreateMobs()
        {
            //on floor 1, 8 pre-set mobs are created so the player can get level 2.
            int mobsToSpawn = Dungeon.depth == 1 ? 8 : NMobs();

            List<Room> stdRooms = new List<Room>();
            foreach (Room room in rooms)
            {
                if (room is StandardRoom && room != roomEntrance)
                {
                    for (int i = 0; i < ((StandardRoom)room).sizeCat.roomValue; ++i)
                    {
                        stdRooms.Add(room);
                    }
                }
            }

            //tt
            //stdRooms.Clear();
            //stdRooms.Add(roomEntrance);

            Rnd.Shuffle(stdRooms);

            var stdRoomIter = stdRooms.GetEnumerator();
            while (mobsToSpawn > 0)
            {
                Mob mob = CreateMob();
                Room roomToSpawn;

                if (!stdRoomIter.MoveNext())
                {
                    stdRoomIter = stdRooms.GetEnumerator();
                    stdRoomIter.MoveNext();
                }

                roomToSpawn = stdRoomIter.Current;

                int tries = 30;
                do
                {
                    mob.pos = PointToCell(roomToSpawn.Random());
                    --tries;
                }
                while (tries >= 0 &&
                         (FindMob(mob.pos) != null ||
                         !passable[mob.pos] ||
                         mob.pos == exit ||
                         (!openSpace[mob.pos] && mob.Properties().Contains(Character.Property.LARGE))));

                if (tries >= 0)
                {
                    --mobsToSpawn;
                    mobs.Add(mob);

                    //add a second mob to this room
                    if (mobsToSpawn > 0 && Rnd.Int(4) == 0)
                    {
                        mob = CreateMob();

                        tries = 30;
                        do
                        {
                            mob.pos = PointToCell(roomToSpawn.Random());
                            --tries;
                        }
                        while (tries >= 0 &&
                                 (FindMob(mob.pos) != null ||
                                 !passable[mob.pos] ||
                                 mob.pos == exit ||
                                 (!openSpace[mob.pos] && mob.Properties().Contains(Character.Property.LARGE))));

                        if (tries >= 0)
                        {
                            --mobsToSpawn;
                            mobs.Add(mob);
                        }
                    }
                }
            }

            foreach (Mob m in mobs)
            {
                if (map[m.pos] == Terrain.HIGH_GRASS ||
                    map[m.pos] == Terrain.FURROWED_GRASS)
                {
                    map[m.pos] = Terrain.GRASS;
                    losBlocking[m.pos] = false;
                }
            }
        }

        public override int RandomRespawnCell(Character ch)
        {
            int count = 0;
            int cell = -1;

            while (true)
            {
                if (++count > 30)
                    return -1;

                Room room = RandomRoom(typeof(StandardRoom));
                if (room == null || room == roomEntrance)
                    continue;

                cell = PointToCell(room.Random(1));
                if (!heroFOV[cell] &&
                    Actor.FindChar(cell) == null &&
                    passable[cell] &&
                    (!Character.HasProp(ch, Character.Property.LARGE) || openSpace[cell]) &&
                    room.CanPlaceCharacter(CellToPoint(cell), this) &&
                    cell != exit)
                {
                    return cell;
                }
            }
        }

        public override int RandomDestination(Character ch)
        {
            int count = 0;
            int cell = -1;

            while (true)
            {
                if (++count > 30)
                    return -1;

                Room room = Rnd.Element(rooms);
                if (room == null)
                    continue;

                cell = PointToCell(room.Random());
                if (passable[cell] &&
                    (!Character.HasProp(ch, Character.Property.LARGE) || openSpace[cell]))
                {
                    return cell;
                }
            }
        }

        public override void CreateItems()
        {
            // drops 3/4/5 items 60%/30%/10% of the time
            int nItems = 3 + Rnd.Chances(new float[] { 6, 3, 1 });

            for (int i = 0; i < nItems; ++i)
            {
                Item toDrop = Generator.Random();
                if (toDrop == null)
                    continue;

                int cell = RandomDropCell();
                if (map[cell] == Terrain.HIGH_GRASS || map[cell] == Terrain.FURROWED_GRASS)
                {
                    map[cell] = Terrain.GRASS;
                    losBlocking[cell] = false;
                }

                Heap.Type type = Heap.Type.HEAP;  // null을 허용하지 않음
                switch (Rnd.Int(20))
                {
                    case 0:
                        type = Heap.Type.SKELETON;
                        break;
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        type = Heap.Type.CHEST;
                        break;
                    case 5:
                        if (Dungeon.depth > 1 && FindMob(cell) == null)
                        {
                            mobs.Add(Mimic.SpawnAt(cell, toDrop));
                            continue;
                        }
                        type = Heap.Type.CHEST;
                        break;
                    default:
                        type = Heap.Type.HEAP;
                        break;
                }

                if ((toDrop is Artifact && Rnd.Int(2) == 0) ||
                    (toDrop.IsUpgradable() && Rnd.Int(4 - toDrop.GetLevel()) == 0))
                {
                    if (Dungeon.depth > 1 && Rnd.Int(10) == 0 && FindMob(cell) == null)
                    {
                        mobs.Add(Mimic.SpawnAt(cell, toDrop, typeof(GoldenMimic)));
                    }
                    else
                    {
                        Heap dropped = Drop(toDrop, cell);
                        if (heaps[cell] == dropped)
                        {
                            dropped.type = Heap.Type.LOCKED_CHEST;
                            AddItemToSpawn(new GoldenKey(Dungeon.depth));
                        }
                    }
                }
                else
                {
                    Heap dropped = Drop(toDrop, cell);
                    dropped.type = type;
                    if (type == Heap.Type.SKELETON)
                        dropped.SetHauntedIfCursed();
                }
            }

            foreach (Item item1 in itemsToSpawn)
            {
                int cell = RandomDropCell();
                Drop(item1, cell).type = Heap.Type.HEAP;
                if (map[cell] == Terrain.HIGH_GRASS || map[cell] == Terrain.FURROWED_GRASS)
                {
                    map[cell] = Terrain.GRASS;
                    losBlocking[cell] = false;
                }
            }

            //use a separate generator for this to prevent held items and meta progress from affecting levelgen
            Rnd.PushGenerator(Dungeon.SeedCurDepth());

            Item item = Bones.Get();
            if (item != null)
            {
                int cell = RandomDropCell();
                if (map[cell] == Terrain.HIGH_GRASS || map[cell] == Terrain.FURROWED_GRASS)
                {
                    map[cell] = Terrain.GRASS;
                    losBlocking[cell] = false;
                }
                Drop(item, cell).SetHauntedIfCursed().type = Heap.Type.REMAINS;
            }

            DriedRose rose = Dungeon.hero.belongings.GetItem<DriedRose>();
            if (rose != null && rose.IsIdentified() && !rose.cursed)
            {
                //aim to drop 1 petal every 2 floors
                int petalsNeeded = (int)Math.Ceiling((float)((Dungeon.depth / 2) - rose.droppedPetals) / 3);

                for (int i = 1; i <= petalsNeeded; ++i)
                {
                    //the player may miss a single petal and still max their rose.
                    if (rose.droppedPetals < 11)
                    {
                        item = new DriedRose.Petal();
                        int cell = RandomDropCell();
                        Drop(item, cell).type = Heap.Type.HEAP;
                        if (map[cell] == Terrain.HIGH_GRASS || map[cell] == Terrain.FURROWED_GRASS)
                        {
                            map[cell] = Terrain.GRASS;
                            losBlocking[cell] = false;
                        }
                        ++rose.droppedPetals;
                    }
                }
            }

            //guide pages
            List<string> allPages = Document.ADVENTURERS_GUIDE.Pages();
            List<string> missingPages = new List<string>();
            foreach (var page in allPages)
            {
                if (!Document.ADVENTURERS_GUIDE.HasPage(page))
                {
                    missingPages.Add(page);
                }
            }

            //a total of 8 pages drop randomly, 2 pages are specially dropped
            missingPages.Remove(Document.GUIDE_INTRO_PAGE);
            missingPages.Remove(Document.GUIDE_SEARCH_PAGE);

            //chance to find a page scales with pages missing and depth
            float dropChance = (missingPages.Count + Dungeon.depth - 1) / (float)(allPages.Count - 2);
            if (missingPages.Count > 0 && Rnd.Float() < dropChance)
            {
                GuidePage p = new GuidePage();
                p.Page(missingPages[0]);
                int cell = RandomDropCell();
                if (map[cell] == Terrain.HIGH_GRASS || map[cell] == Terrain.FURROWED_GRASS)
                {
                    map[cell] = Terrain.GRASS;
                    losBlocking[cell] = false;
                }
                Drop(p, cell);
            }

            Rnd.PopGenerator();
        }

        public List<Room> Rooms()
        {
            return new List<Room>(rooms);
        }

        //FIXME pit rooms shouldn't be problematic enough to warrant this
        public bool HasPitRoom()
        {
            foreach (Room r in rooms)
            {
                if (r is PitRoom)
                {
                    return true;
                }
            }
            return false;
        }

        protected Room RandomRoom(Type type) // Class<?extends Room> 
        {
            Rnd.Shuffle(rooms);
            foreach (var r in rooms)
            {
                // if (type.isInstance(r))
                if (type.IsAssignableFrom(r.GetType()))
                {
                    return r;
                }
            }
            return null;
        }

        public Room Room(int pos)
        {
            var pt = CellToPoint(pos);

            foreach (var room in rooms)
            {
                if (room.Inside(pt))
                    return room;
            }

            return null;
        }

        protected int RandomDropCell()
        {
            while (true)
            {
                Room room = RandomRoom(typeof(StandardRoom));
                if (room != null && room != roomEntrance)
                {
                    int pos = PointToCell(room.Random());
                    if (passable[pos] &&
                        pos != exit &&
                        heaps[pos] == null &&
                        FindMob(pos) == null)
                    {
                        Trap t = traps[pos];

                        //items cannot spawn on traps which destroy items
                        if (t == null ||
                            !(t is BurningTrap ||
                              t is BlazingTrap ||
                              t is ChillingTrap ||
                              t is FrostTrap ||
                              t is ExplosiveTrap ||
                              t is DisintegrationTrap))
                        {
                            return pos;
                        }
                    }
                }
            }
        }

        public override int FallCell(bool fallIntoPit)
        {
            if (fallIntoPit)
            {
                foreach (Room room in rooms)
                {
                    if (room is PitRoom)
                    {
                        int result;
                        do
                        {
                            result = PointToCell(room.Random());
                        }
                        while (traps[result] != null ||
                               FindMob(result) != null ||
                               heaps[result] != null);

                        return result;
                    }
                }
            }

            return base.FallCell(false);
        }

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put("rooms", rooms);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);

            rooms = new List<Room>();
            foreach (var r in bundle.GetCollection("rooms"))
            {
                rooms.Add((Room)r);
            }

            foreach (Room r in rooms)
            {
                r.OnLevelLoad(this);
                if (r is EntranceRoom)
                    roomEntrance = r;
                else if (r is ExitRoom)
                    roomExit = r;
            }
        }
    }
}
