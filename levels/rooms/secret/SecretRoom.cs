using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors.hero;
using spdd.levels.rooms.special;

namespace spdd.levels.rooms.secret
{
    public abstract class SecretRoom : SpecialRoom
    {
        private static List<Type> ALL_SECRETS = new List<Type>() {
            typeof(SecretGardenRoom),
            typeof(SecretLaboratoryRoom),
            typeof(SecretLibraryRoom),
            typeof(SecretLarderRoom),
            typeof(SecretWellRoom),
            typeof(SecretRunestoneRoom),
            typeof(SecretArtilleryRoom),
            typeof(SecretChestChasmRoom),
            typeof(SecretHoneypotRoom),
            typeof(SecretHoardRoom),
            typeof(SecretMazeRoom),
            typeof(SecretSummoningRoom)
        };

        public static List<Type> runSecrets = new List<Type>();

        //this is the number of secret rooms per region (whole value),
        // plus the chance for an extra secret room (fractional value)
        private static float[] baseRegionSecrets = new float[] { 1.4f, 1.8f, 2.2f, 2.6f, 3.0f };
        private static int[] regionSecretsThisRun = new int[5];

        //public static void initForRun()
        public static void InitForRunSecret()
        {
            float[] regionChances = (float[])baseRegionSecrets.Clone();

            if (GamesInProgress.selectedClass == HeroClass.ROGUE)
            {
                for (int i = 0; i < regionChances.Length; ++i)
                {
                    regionChances[i] += 0.6f;
                }
            }

            for (int i = 0; i < regionSecretsThisRun.Length; ++i)
            {
                regionSecretsThisRun[i] = (int)regionChances[i];
                if (Rnd.Float() < regionChances[i] % 1f)
                {
                    ++regionSecretsThisRun[i];
                }
            }

            runSecrets = new List<Type>(ALL_SECRETS);
            Rnd.Shuffle(runSecrets);
        }

        public static int SecretsForFloor(int depth)
        {
            if (depth == 1)
                return 0;

            int region = depth / 5;
            int floor = depth % 5;

            int floorsLeft = 5 - floor;

            float secrets;
            if (floorsLeft == 0)
            {
                secrets = regionSecretsThisRun[region];
            }
            else
            {
                secrets = regionSecretsThisRun[region] / (float)floorsLeft;
                if (Rnd.Float() < secrets % 1f)
                {
                    secrets = (float)Math.Ceiling(secrets);
                }
                else
                {
                    secrets = (float)Math.Floor(secrets);
                }
            }

            regionSecretsThisRun[region] -= (int)secrets;
            return (int)secrets;
        }

        //public static SecretRoom createRoom()
        public static SecretRoom CreateSecretRoom()
        {
            SecretRoom r = null;
            int index = runSecrets.Count;
            for (int i = 0; i < 4; ++i)
            {
                int newidx = Rnd.Int(runSecrets.Count);
                if (newidx < index)
                    index = newidx;
            }

            r = (SecretRoom)Reflection.NewInstance(runSecrets[index]);

            var toRemove = runSecrets[index];
            runSecrets.RemoveAt(index);

            runSecrets.Add(toRemove);

            return r;
        }

        private const string ROOMS = "secret_rooms";
        private const string REGIONS = "region_secrets";

        public static void RestoreSecretRoomsFromBundle(Bundle bundle)
        {
            runSecrets.Clear();
            if (bundle.Contains(ROOMS))
            {
                foreach (var type in bundle.GetClassArray(ROOMS))
                {
                    if (type != null)
                        runSecrets.Add(type);
                }
                regionSecretsThisRun = bundle.GetIntArray(REGIONS);
            }
            else
            {
                InitForRunSecret();
                ShatteredPixelDungeonDash.ReportException(new Exception("secrets array didn't exist!"));
            }
        }

        public static void StoreSecretRoomsInBundle(Bundle bundle)
        {
            bundle.Put(ROOMS, runSecrets.ToArray());
            bundle.Put(REGIONS, regionSecretsThisRun);
        }
    }
}
