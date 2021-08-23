using System.IO;
using watabou.utils;

namespace spdd.journal
{
    public class Journal
    {
        public const string JOURNAL_FILE = "journal.dat";

        private static bool loaded;

        public static void LoadGlobal()
        {
            if (loaded)
                return;

            Bundle bundle;
            try
            {
                bundle = FileUtils.BundleFromFile(JOURNAL_FILE);
            }
            catch (IOException)
            {
                bundle = new Bundle();
            }

            CatalogExtensions.Restore(bundle);
            Document.Restore(bundle);

            loaded = true;
        }

        //package-private
        public static bool saveNeeded;

        public static void SaveGlobal()
        {
            if (!saveNeeded)
                return;

            Bundle bundle = new Bundle();

            CatalogExtensions.Store(bundle);
            Document.Store(bundle);

            try
            {
                FileUtils.BundleToFile(JOURNAL_FILE, bundle);
                saveNeeded = false;
            }
            catch (IOException e)
            {
                ShatteredPixelDungeonDash.ReportException(e);
            }
        }
    }
}