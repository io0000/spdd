using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.items.keys;
using spdd.messages;

namespace spdd.journal
{
    public class Notes
    {
        public abstract class Record : IBundlable
        {
            public abstract int Depth();
            public abstract string Desc();
            public abstract void RestoreFromBundle(Bundle bundle);
            public abstract void StoreInBundle(Bundle bundle);
        }

        public enum Landmark
        {
            WELL_OF_HEALTH,
            WELL_OF_AWARENESS,
            WELL_OF_TRANSMUTATION,
            ALCHEMY,
            GARDEN,
            STATUE,
            SHOP,

            GHOST,
            WANDMAKER,
            TROLL,
            IMP
        };

        [SPDStatic]
        public class LandmarkRecord : Record, IEquatable<LandmarkRecord>
        {
            protected Landmark landmark;
            public int depth;

            public LandmarkRecord()  // serializeø°º≠ »£√‚µ 
            { }

            public LandmarkRecord(Landmark landmark, int depth)
            {
                this.landmark = landmark;
                this.depth = depth;
            }

            public override int Depth()
            {
                return depth;
            }

            public override string Desc()
            {
                return Messages.Get(typeof(Landmark), landmark.ToString());
            }

            private const string DEPTH = "depth";
            private const string LANDMARK = "landmark";

            public override void RestoreFromBundle(Bundle bundle)
            {
                depth = bundle.GetInt(DEPTH);
                landmark = bundle.GetEnum<Landmark>(LANDMARK);
            }

            public override void StoreInBundle(Bundle bundle)
            {
                bundle.Put(DEPTH, depth);
                bundle.Put(LANDMARK, landmark.ToString());
            }

            public override bool Equals(object obj)
            {
                return obj is LandmarkRecord && Equals((LandmarkRecord)obj);
            }

            public bool Equals(LandmarkRecord other)
            {
                return (landmark == other.landmark) &&
                    (Depth() == other.Depth());
            }

            public override int GetHashCode()
            {
                return landmark.GetHashCode() ^ depth.GetHashCode();
            }

            public static bool operator ==(LandmarkRecord left, LandmarkRecord right)
            {
                if (((object)left) == null || ((object)right) == null)
                    return object.Equals(left, right);

                return left.Equals(right);
            }

            public static bool operator !=(LandmarkRecord left, LandmarkRecord right)
            {
                if (((object)left) == null || ((object)right) == null)
                    return !object.Equals(left, right);

                return !(left == right);
            }
        }

        [SPDStatic]
        public class KeyRecord : Record, IEquatable<KeyRecord>
        {
            protected Key key;

            public KeyRecord()
            { }

            public KeyRecord(Key key)
            {
                this.key = key;
            }

            public override int Depth()
            {
                return key.depth;
            }

            public override string Desc()
            {
                return key.ToString();
            }

            public Type Type()
            {
                return key.GetType();
            }

            public int Quantity()
            {
                return key.Quantity();
            }

            public void Quantity(int num)
            {
                key.Quantity(num);
            }

            private const string KEY = "key";

            public override void RestoreFromBundle(Bundle bundle)
            {
                key = (Key)bundle.Get(KEY);
            }

            public override void StoreInBundle(Bundle bundle)
            {
                bundle.Put(KEY, key);
            }

            public override bool Equals(object obj)
            {
                return obj is KeyRecord && Equals((KeyRecord)obj);
            }

            public bool Equals(KeyRecord other)
            {
                //return Depth() == other.Depth();
                return key.IsSimilar(other.key);
            }

            public override int GetHashCode()
            {
                return key.GetHashCode();
            }

            public static bool operator ==(KeyRecord left, KeyRecord right)
            {
                if (((object)left) == null || ((object)right) == null)
                    return object.Equals(left, right);

                return left.Equals(right);
            }

            public static bool operator !=(KeyRecord left, KeyRecord right)
            {
                if (((object)left) == null || ((object)right) == null)
                    return !object.Equals(left, right);

                return !(left == right);
            }
        }

        public static List<Notes.LandmarkRecord> landmarkRecords;
        public static List<Notes.KeyRecord> keyRecords;

        public static void Reset()
        {
            landmarkRecords = new List<Notes.LandmarkRecord>();
            keyRecords = new List<Notes.KeyRecord>();
        }

        private const string LANDMARK_RECORDS = "landmark_records";
        private const string KEY_RECORDS = "key_records";

        public static void StoreInBundle(Bundle bundle)
        {
            bundle.Put(LANDMARK_RECORDS, landmarkRecords);
            bundle.Put(KEY_RECORDS, keyRecords);
        }

        public static void RestoreFromBundle(Bundle bundle)
        {
            landmarkRecords = new List<Notes.LandmarkRecord>();
            keyRecords = new List<Notes.KeyRecord>();

            foreach (var rec in bundle.GetCollection(LANDMARK_RECORDS))
                landmarkRecords.Add((Notes.LandmarkRecord)rec);
            foreach (var rec in bundle.GetCollection(KEY_RECORDS))
                keyRecords.Add((Notes.KeyRecord)rec);
        }

        public static bool Add(Landmark landmark)
        {
            LandmarkRecord l = new LandmarkRecord(landmark, Dungeon.depth);
            if (!landmarkRecords.Contains(l))
            {
                landmarkRecords.Add(l);
                landmarkRecords.Sort((r1, r2) => r2.Depth() - r1.Depth());
                return true;
            }
            return false;
        }

        public static bool Remove(Landmark landmark)
        {
            LandmarkRecord l = new LandmarkRecord(landmark, Dungeon.depth);
            return landmarkRecords.Remove(l);
        }

        public static bool Add(Key key)
        {
            KeyRecord k = new KeyRecord(key);
            int index = keyRecords.IndexOf(k);

            if (index == -1)
            {
                keyRecords.Add(k);
                keyRecords.Sort((r1, r2) => r2.Depth() - r1.Depth());
                return true;
            }
            else
            {
                k = keyRecords[index];
                k.Quantity(k.Quantity() + key.Quantity());
                return true;
            }
        }

        public static bool Remove(Key key)
        {
            KeyRecord k = new KeyRecord(key);
            int index = keyRecords.IndexOf(k);

            if (index != -1)
            {
                k = keyRecords[index];
                k.Quantity(k.Quantity() - key.Quantity());
                if (k.Quantity() <= 0)
                    keyRecords.Remove(k);

                return true;
            }
            return false;
        }

        public static int KeyCount(Key key)
        {
            KeyRecord k = new KeyRecord(key);
            int index = keyRecords.IndexOf(k);

            if (index != -1)
            {
                k = keyRecords[index];
                return k.Quantity();
            }
            else
            {
                return 0;
            }
        }

        public static List<LandmarkRecord> GetLandmarkRecords()
        {
            return landmarkRecords;
        }

        public static List<KeyRecord> GetKeyRecords()
        {
            return keyRecords;
        }
    }
}