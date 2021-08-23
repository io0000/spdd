namespace spdd.services.updates
{
    public class AvailableUpdateData
    {
        public string versionName;
        public int versionCode;
        public string desc;
        public string URL;
    }

    public class Updates
    {
        public static bool SupportsUpdates()
        {
            return false;
        }

        public static bool IsUpdateable()
        {
            return false;
        }

        public static void CheckForUpdate()
        {
        }

        public static void LaunchUpdate(AvailableUpdateData data)
        {
        }

        private static AvailableUpdateData updateData = null;

        public static bool UpdateAvailable()
        {
            return updateData != null;
        }

        public static AvailableUpdateData UpdateData()
        {
            return updateData;
        }

        public static void ClearUpdate()
        {
        }

        public static bool IsInstallable()
        {
            return false;
        }

        public static void LaunchInstall()
        {
        }
    }
}

