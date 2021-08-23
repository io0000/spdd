using System;
using System.Collections.Generic;
using watabou.utils;

namespace spdd.levels.rooms.special
{
    public abstract class SpecialRoom : Room
    {
        public override int MinWidth()
        {
            return 5;
        }

        public override int MaxWidth()
        {
            return 10;
        }

        public override int MinHeight()
        {
            return 5;
        }

        public override int MaxHeight()
        {
            return 10;
        }

        public override int MaxConnections(int direction)
        {
            return 1;
        }

        private Door entrance;

        public virtual Door Entrance()
        {
            if (entrance == null)
            {
                if (connected.Count == 0)
                {
                    return null;
                }
                else
                {
                    //entrance = connected.values().iterator().next();
                    var it = connected.Values.GetEnumerator();
                    it.MoveNext();
                    entrance = it.Current;
                }
            }
            return entrance;
        }

        private const string ENTRANCE = "entrance";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            if (Entrance() != null)
            {
                bundle.Put(ENTRANCE, Entrance());
            }
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            if (bundle.Contains(ENTRANCE))
            {
                entrance = (Door)bundle.Get(ENTRANCE);
            }
        }

        private static readonly List<Type> ALL_SPEC = new List<Type> {
            typeof(WeakFloorRoom),
            typeof(MagicWellRoom),
            typeof(CryptRoom),
            typeof(PoolRoom),
            typeof(GardenRoom),
            typeof(LibraryRoom),
            typeof(ArmoryRoom),
            typeof(TreasuryRoom),
            typeof(TrapsRoom),
            typeof(StorageRoom),
            typeof(StatueRoom),
            typeof(VaultRoom),
            typeof(RunestoneRoom)
        };

        public static List<Type> runSpecials = new List<Type>();
        public static List<Type> floorSpecials = new List<Type>();

        private static int pitNeededDepth = -1;

        //public static void initForRun()
        public static void InitForRunSpecial()
        {
            runSpecials = new List<Type>(ALL_SPEC);

            pitNeededDepth = -1;
            Rnd.Shuffle(runSpecials);
        }

        public static void InitForFloor()
        {
            floorSpecials = new List<Type>(runSpecials);

            //laboratory rooms spawn at set intervals every chapter
            if (Dungeon.depth % 5 == (Dungeon.seed % 3 + 2))
            {
                floorSpecials.Insert(0, typeof(LaboratoryRoom));
            }
        }

        private static void UseType(Type type)
        {
            floorSpecials.Remove(type);
            if (runSpecials.Remove(type))
            {
                runSpecials.Add(type);
            }
        }

        public static void ResetPitRoom(int depth)
        {
            if (pitNeededDepth == depth)
            {
                pitNeededDepth = -1;
            }
        }

        public static SpecialRoom CreateSpecialRoom()
        {
            if (Dungeon.depth == pitNeededDepth)
            {
                pitNeededDepth = -1;

                floorSpecials.Remove(typeof(ArmoryRoom));
                floorSpecials.Remove(typeof(CryptRoom));
                floorSpecials.Remove(typeof(LibraryRoom));
                floorSpecials.Remove(typeof(RunestoneRoom));
                floorSpecials.Remove(typeof(StatueRoom));
                floorSpecials.Remove(typeof(TreasuryRoom));
                floorSpecials.Remove(typeof(VaultRoom));
                floorSpecials.Remove(typeof(WeakFloorRoom));

                return new PitRoom();
            }
            else if (floorSpecials.Contains(typeof(LaboratoryRoom)))
            {
                UseType(typeof(LaboratoryRoom));
                return new LaboratoryRoom();
            }
            else
            {
                if (Dungeon.BossLevel(Dungeon.depth + 1))
                {
                    floorSpecials.Remove(typeof(WeakFloorRoom));
                }

                Room r = null;
                int index = floorSpecials.Count;
                for (int i = 0; i < 4; ++i)
                {
                    int newidx = Rnd.Int(floorSpecials.Count);
                    if (newidx < index)
                        index = newidx;
                }

                r = (Room)Reflection.NewInstance(floorSpecials[index]);

                if (r is WeakFloorRoom)
                    pitNeededDepth = Dungeon.depth + 1;

                UseType(r.GetType());
                return (SpecialRoom)r;
            }
        }

        private const string ROOMS = "special_rooms";
        private const string PIT = "pit_needed";

        //public static void restoreRoomsFromBundle( Bundle bundle )
        public static void RestoreSpecialRoomsFromBundle(Bundle bundle)
        {
            runSpecials.Clear();
            if (bundle.Contains(ROOMS))
            {
                foreach (Type type in bundle.GetClassArray(ROOMS))
                {
                    runSpecials.Add(type);
                }
            }
            else
            {
                InitForRunSpecial();
                ShatteredPixelDungeonDash.ReportException(new Exception("specials array didn't exist!"));
            }
            pitNeededDepth = bundle.GetInt(PIT);
        }

        // public static void storeRoomsInBundle( Bundle bundle )
        public static void StoreSpecialRoomsInBundle(Bundle bundle)
        {
            bundle.Put(ROOMS, runSpecials.ToArray());
            bundle.Put(PIT, pitNeededDepth);
        }
    }
}